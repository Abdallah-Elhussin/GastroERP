using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Interfaces.Inventory;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.DTOs;
using GastroErp.Application.Features.Finance.Services;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Entities.Finance;
using GastroErp.Domain.Entities.Inventory.Purchasing;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Inventory.Services;

public interface IPurchaseAccountingService
{
    Task<Result> PostGoodsReceiptAsync(Guid goodsReceiptId, Guid userId, CancellationToken ct = default);
    Task<Result> UnpostGoodsReceiptAsync(Guid goodsReceiptId, Guid userId, CancellationToken ct = default);
    Task<Result> PostPurchaseInvoiceAsync(Guid purchaseInvoiceId, Guid userId, CancellationToken ct = default);
    Task<Result> UnpostPurchaseInvoiceAsync(Guid purchaseInvoiceId, Guid userId, CancellationToken ct = default);
    Task<Result> PostPurchaseReturnAsync(Guid purchaseReturnId, Guid userId, CancellationToken ct = default);
    Task<Result> UnpostPurchaseReturnAsync(Guid purchaseReturnId, Guid userId, CancellationToken ct = default);
}

/// <summary>
/// Accounting integration for the purchasing cycle (GRNI clearing + AP invoice).
/// </summary>
public sealed class PurchaseAccountingService(
    IApplicationDbContext context,
    IInventoryMovementPipeline pipeline,
    IJournalPostingService journalPosting,
    ILogger<PurchaseAccountingService> logger) : IPurchaseAccountingService
{
    public async Task<Result> PostGoodsReceiptAsync(Guid goodsReceiptId, Guid userId, CancellationToken ct = default)
    {
        var gr = await context.GoodsReceipts.Include(g => g.Lines)
            .FirstOrDefaultAsync(g => g.Id == goodsReceiptId, ct);
        if (gr is null)
            return Result.Failure("GoodsReceiptNotFound", "Goods receipt not found.");
        if (gr.Status is not (GoodsReceiptStatus.Draft or GoodsReceiptStatus.Approved))
            return Result.Failure(ErrorCodes.InvalidStatusTransition, "Only draft/approved receipts can be posted.");
        if (!gr.Lines.Any())
            return Result.Failure("NoLines", "Cannot post goods receipt with no lines.");

        if (gr.InspectionResult == InspectionResult.Rejected)
            return Result.Failure(ErrorCodes.InvalidStatusTransition, "Rejected inspection cannot be posted.");

        var settings = await context.AccountingSettings
            .FirstOrDefaultAsync(s => s.TenantId == gr.TenantId && s.CompanyId == null, ct);
        if (settings is null)
            return Result.Failure("AccountsNotMapped", "إعدادات الحسابات غير موجودة. اضبط الربط المحاسبي أولاً.");

        var inventoryAccountId = settings.InventoryAccountId;
        if (inventoryAccountId is null)
            return Result.Failure("AccountsNotMapped", "يجب ربط حساب المخزون في الإعدادات المحاسبية قبل الترحيل.");

        var grniAccountId = settings.GrniAccountId;
        if (grniAccountId is null)
        {
            grniAccountId = await EnsureGrniAccountMappedAsync(settings, ct);
            if (grniAccountId is null)
                return Result.Failure("AccountsNotMapped", "تعذر تجهيز حساب الاستلامات غير المفوترة (GRNI). اربطه من الإعدادات المحاسبية.");
        }

        var stockLines = gr.Lines.Where(l => l.AcceptedQuantity > 0).ToList();
        if (stockLines.Count == 0)
            return Result.Failure(ErrorCodes.InvalidQuantity, "لا توجد كمية مقبولة للترحيل.");

        var stock = await pipeline.ApplyMovementAsync(new InventoryMovementRequest(
            gr.TenantId,
            InventoryMovementType.IN,
            TransactionType.GoodsReceipt,
            gr.Id,
            gr.ReceiptNumber,
            stockLines.Select(l => new InventoryMovementLine(
                l.InventoryItemId, gr.WarehouseId, l.UnitId, l.AcceptedQuantity, l.UnitCost)).ToList(),
            gr.Notes), ct);
        if (stock.IsFailure)
            return Result.Failure(stock.ErrorCode!, stock.ErrorMessage);

        var amount = stockLines.Sum(l => Math.Max(0, (l.AcceptedQuantity * l.UnitCost) - l.DiscountAmount));
        if (amount <= 0)
            return Result.Failure(ErrorCodes.InvalidAmount, "مبلغ الترحيل يجب أن يكون أكبر من صفر.");
        var journalDto = new CreateJournalDto(
            DateOnly.FromDateTime(gr.ReceiptDate.UtcDateTime),
            $"GRN {gr.ReceiptNumber}",
            PostingSource.Purchase,
            SourceDocumentId: gr.Id,
            Reference: gr.ReceiptNumber,
            Lines:
            [
                new JournalLineDto(null, inventoryAccountId.Value, null, amount, 0, "Inventory"),
                new JournalLineDto(null, grniAccountId.Value, null, 0, amount, "GRNI")
            ]);

        var journal = await journalPosting.CreateAndPostAsync(gr.TenantId, userId, journalDto, ct);
        if (!journal.IsSuccess)
            return Result.Failure(journal.ErrorCode!, journal.ErrorMessage!);

        await UpdatePoReceivedQuantitiesAsync(gr, increase: true, ct);

        gr.MarkPosted(journal.Data!.Id, userId);
        context.GoodsReceipts.Update(gr);
        await context.SaveChangesAsync(ct);
        logger.LogInformation("Goods receipt posted with GRNI journal {JournalId}: {ReceiptId}", journal.Data.Id, gr.Id);
        return Result.Success();
    }

    public async Task<Result> UnpostGoodsReceiptAsync(Guid goodsReceiptId, Guid userId, CancellationToken ct = default)
    {
        var gr = await context.GoodsReceipts.Include(g => g.Lines)
            .FirstOrDefaultAsync(g => g.Id == goodsReceiptId, ct);
        if (gr is null)
            return Result.Failure("GoodsReceiptNotFound", "Goods receipt not found.");
        if (gr.Status != GoodsReceiptStatus.Posted)
            return Result.Failure(ErrorCodes.InvalidStatusTransition, "Only posted receipts can be unposted.");
        if (gr.Lines.Any(l => l.InvoicedQuantity > 0))
            return Result.Failure("HasInvoices", "Cannot unpost a receipt that has been invoiced.");

        var outbound = await pipeline.ApplyMovementAsync(new InventoryMovementRequest(
            gr.TenantId,
            InventoryMovementType.OUT,
            TransactionType.Reversal,
            gr.Id,
            $"REV-{gr.ReceiptNumber}",
            gr.Lines.Where(l => l.AcceptedQuantity > 0).Select(l => new InventoryMovementLine(
                l.InventoryItemId, gr.WarehouseId, l.UnitId, l.AcceptedQuantity, l.UnitCost)).ToList(),
            $"Unpost {gr.ReceiptNumber}"), ct);
        if (outbound.IsFailure)
            return Result.Failure(outbound.ErrorCode!, outbound.ErrorMessage);

        Guid? reversalJournalId = null;
        if (gr.JournalEntryId.HasValue && gr.JournalEntryId != Guid.Empty)
        {
            var rev = await journalPosting.ReverseAsync(gr.JournalEntryId.Value, userId, ct);
            if (!rev.IsSuccess)
                return Result.Failure(rev.ErrorCode!, rev.ErrorMessage!);
            reversalJournalId = rev.Data!.Id;
        }

        await UpdatePoReceivedQuantitiesAsync(gr, increase: false, ct);
        gr.MarkReversed(reversalJournalId);
        context.GoodsReceipts.Update(gr);
        await context.SaveChangesAsync(ct);
        logger.LogInformation("Goods receipt unposted: {ReceiptId}", gr.Id);
        return Result.Success();
    }

    public async Task<Result> PostPurchaseInvoiceAsync(Guid purchaseInvoiceId, Guid userId, CancellationToken ct = default)
    {
        var inv = await context.PurchaseInvoices.Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == purchaseInvoiceId, ct);
        if (inv is null)
            return Result.Failure("PurchaseInvoiceNotFound", "Purchase invoice not found.");
        if (!inv.Lines.Any())
            return Result.Failure("NoLines", "Invoice has no lines.");

        if (inv.Kind == PurchaseInvoiceKind.Direct)
        {
            if (inv.Status != PurchasingDocumentStatus.Approved)
                return Result.Failure(ErrorCodes.InvalidStatusTransition, "Direct invoice must be approved before posting.");
        }
        else if (inv.Status is not (PurchasingDocumentStatus.Draft or PurchasingDocumentStatus.Approved))
        {
            return Result.Failure(ErrorCodes.InvalidStatusTransition, "Only draft/approved invoices can be posted.");
        }

        var settings = await context.AccountingSettings.AsNoTracking()
            .FirstOrDefaultAsync(s => s.TenantId == inv.TenantId && s.CompanyId == null, ct);
        if (settings is null)
            return Result.Failure("AccountsNotMapped", "Accounting settings not found.");

        var supplier = await context.Suppliers.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == inv.SupplierId, ct);
        var apAccount = supplier?.ApAccountId ?? settings.AccountsPayableAccountId;
        if (apAccount is null)
            return Result.Failure("AccountsNotMapped", "Accounts payable account is not mapped.");

        if (inv.Kind == PurchaseInvoiceKind.FromReceipt)
        {
            if (settings.GrniAccountId is null)
                return Result.Failure("AccountsNotMapped", "GRNI account must be mapped.");

            var net = inv.SubTotal;
            var tax = inv.TaxAmount;
            var lines = new List<JournalLineDto>
            {
                new(null, settings.GrniAccountId.Value, null, net, 0, "Clear GRNI")
            };
            if (tax > 0)
            {
                if (settings.VatInputAccountId is null)
                    return Result.Failure("AccountsNotMapped", "VAT input account must be mapped.");
                lines.Add(new JournalLineDto(null, settings.VatInputAccountId.Value, null, tax, 0, "Input VAT"));
            }
            lines.Add(new JournalLineDto(null, apAccount.Value, null, 0, inv.TotalAmount, "Supplier AP"));

            var journal = await journalPosting.CreateAndPostAsync(inv.TenantId, userId, new CreateJournalDto(
                inv.InvoiceDate, $"PI {inv.InvoiceNumber}", PostingSource.Purchase,
                SourceDocumentId: inv.Id, Reference: inv.InvoiceNumber, Lines: lines), ct);
            if (!journal.IsSuccess)
                return Result.Failure(journal.ErrorCode!, journal.ErrorMessage!);

            if (inv.GoodsReceiptId.HasValue)
            {
                var receipt = await context.GoodsReceipts.Include(g => g.Lines)
                    .FirstOrDefaultAsync(g => g.Id == inv.GoodsReceiptId.Value, ct);
                if (receipt is not null)
                {
                    foreach (var line in inv.Lines.Where(l => l.GoodsReceiptLineId.HasValue))
                    {
                        var grLine = receipt.Lines.FirstOrDefault(l => l.Id == line.GoodsReceiptLineId);
                        grLine?.AddInvoicedQuantity(line.Quantity);
                    }
                    context.GoodsReceipts.Update(receipt);
                }
            }

            inv.MarkPosted(journal.Data!.Id, userId);
        }
        else
        {
            var postDirect = await PostDirectPurchaseInvoiceAsync(inv, settings, apAccount.Value, userId, ct);
            if (postDirect.IsFailure)
                return postDirect;
        }

        context.PurchaseInvoices.Update(inv);
        await context.SaveChangesAsync(ct);
        logger.LogInformation("Purchase invoice posted: {InvoiceId}", inv.Id);
        return Result.Success();
    }

    public async Task<Result> UnpostPurchaseInvoiceAsync(Guid purchaseInvoiceId, Guid userId, CancellationToken ct = default)
    {
        var inv = await context.PurchaseInvoices.Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == purchaseInvoiceId, ct);
        if (inv is null)
            return Result.Failure("PurchaseInvoiceNotFound", "Purchase invoice not found.");
        if (inv.Status != PurchasingDocumentStatus.Posted)
            return Result.Failure(ErrorCodes.InvalidStatusTransition, "Only posted invoices can be reversed.");
        // Cash settlement is created by posting itself — clear it so reverse is allowed.
        if (inv.PaymentMode == PurchaseInvoicePaymentMode.Cash && inv.PaidAmount > 0)
            inv.ClearSettlement();
        else if (inv.PaidAmount > 0)
            return Result.Failure("HasPayments", "Cannot reverse an invoice that has payments applied.");
        if (inv.Lines.Any(l => l.ReturnedQuantity > 0))
            return Result.Failure("HasReturns", "Cannot reverse an invoice that has returns.");

        if (inv.Kind == PurchaseInvoiceKind.Direct && inv.AffectsInventory)
        {
            var movementLines = new List<InventoryMovementLine>();
            foreach (var line in inv.Lines)
            {
                var warehouseId = line.LineWarehouseId ?? inv.WarehouseId;
                if (warehouseId is null)
                    return Result.Failure("RequiredField", "Warehouse is required to reverse inventory.");
                movementLines.Add(new InventoryMovementLine(
                    line.InventoryItemId, warehouseId.Value, line.UnitId, line.Quantity, line.UnitPrice));
            }

            var outbound = await pipeline.ApplyMovementAsync(new InventoryMovementRequest(
                inv.TenantId,
                InventoryMovementType.OUT,
                TransactionType.Reversal,
                inv.Id,
                $"REV-{inv.InvoiceNumber}",
                movementLines,
                $"Reverse {inv.InvoiceNumber}"), ct);
            if (outbound.IsFailure)
                return Result.Failure(outbound.ErrorCode!, outbound.ErrorMessage);
        }

        if (inv.Kind == PurchaseInvoiceKind.FromReceipt && inv.GoodsReceiptId.HasValue)
        {
            var receipt = await context.GoodsReceipts.Include(g => g.Lines)
                .FirstOrDefaultAsync(g => g.Id == inv.GoodsReceiptId.Value, ct);
            if (receipt is not null)
            {
                foreach (var line in inv.Lines.Where(l => l.GoodsReceiptLineId.HasValue))
                {
                    var grLine = receipt.Lines.FirstOrDefault(l => l.Id == line.GoodsReceiptLineId);
                    grLine?.ReduceInvoicedQuantity(line.Quantity);
                }
                context.GoodsReceipts.Update(receipt);
            }
        }

        Guid? reversalJournalId = null;
        if (inv.JournalEntryId.HasValue && inv.JournalEntryId != Guid.Empty)
        {
            var rev = await journalPosting.ReverseAsync(inv.JournalEntryId.Value, userId, ct);
            if (!rev.IsSuccess)
                return Result.Failure(rev.ErrorCode!, rev.ErrorMessage!);
            reversalJournalId = rev.Data!.Id;
        }

        inv.MarkReversed(reversalJournalId);
        context.PurchaseInvoices.Update(inv);
        await context.SaveChangesAsync(ct);
        logger.LogInformation("Purchase invoice reversed: {InvoiceId}", inv.Id);
        return Result.Success();
    }

    private async Task<Result> PostDirectPurchaseInvoiceAsync(
        PurchaseInvoice inv,
        Domain.Entities.Finance.AccountingSettings settings,
        Guid apAccountId,
        Guid userId,
        CancellationToken ct)
    {
        if (inv.AffectsInventory)
        {
            var movementLines = new List<InventoryMovementLine>();
            foreach (var line in inv.Lines)
            {
                var warehouseId = line.LineWarehouseId ?? inv.WarehouseId;
                if (warehouseId is null)
                    return Result.Failure("RequiredField", "Warehouse is required for inventory purchases.");
                var unitCost = line.Quantity <= 0 ? 0 : line.LineNet / line.Quantity;
                movementLines.Add(new InventoryMovementLine(
                    line.InventoryItemId, warehouseId.Value, line.UnitId, line.Quantity, unitCost));
            }

            var stock = await pipeline.ApplyMovementAsync(new InventoryMovementRequest(
                inv.TenantId,
                InventoryMovementType.IN,
                TransactionType.GoodsReceipt,
                inv.Id,
                inv.InvoiceNumber,
                movementLines,
                inv.Notes), ct);
            if (stock.IsFailure)
                return Result.Failure(stock.ErrorCode!, stock.ErrorMessage);
        }

        var debitAccountResult = ResolveDirectDebitAccount(inv.Nature, settings);
        if (debitAccountResult.IsFailure)
            return Result.Failure(debitAccountResult.ErrorCode!, debitAccountResult.ErrorMessage!);

        var creditAccount = inv.PaymentMode == PurchaseInvoicePaymentMode.Cash
            ? settings.CashAccountId ?? settings.BankAccountId
            : apAccountId;
        if (creditAccount is null)
            return Result.Failure("AccountsNotMapped",
                inv.PaymentMode == PurchaseInvoicePaymentMode.Cash
                    ? "Cash/bank account must be mapped for cash purchases."
                    : "AP account must be mapped.");

        var goodsNet = Math.Max(0, inv.SubTotal - inv.DiscountAmount);
        var lines = new List<JournalLineDto>
        {
            new(null, debitAccountResult.Data, inv.CostCenterId, goodsNet, 0, DebitLabel(inv.Nature))
        };
        if (inv.TaxAmount > 0)
        {
            if (settings.VatInputAccountId is null)
                return Result.Failure("AccountsNotMapped", "VAT input account must be mapped.");
            lines.Add(new JournalLineDto(null, settings.VatInputAccountId.Value, null, inv.TaxAmount, 0, "Input VAT"));
        }
        lines.Add(new JournalLineDto(null, creditAccount.Value, null, 0, inv.TotalAmount,
            inv.PaymentMode == PurchaseInvoicePaymentMode.Cash ? "Cash / Bank" : "Supplier AP"));

        var journal = await journalPosting.CreateAndPostAsync(inv.TenantId, userId, new CreateJournalDto(
            inv.InvoiceDate, $"DPI {inv.InvoiceNumber}", PostingSource.Purchase,
            SourceDocumentId: inv.Id, Reference: inv.InvoiceNumber, Lines: lines), ct);
        if (!journal.IsSuccess)
            return Result.Failure(journal.ErrorCode!, journal.ErrorMessage!);

        inv.MarkPosted(journal.Data!.Id, userId);
        // Cash purchase settles immediately (journal already credits cash/bank).
        if (inv.PaymentMode == PurchaseInvoicePaymentMode.Cash && inv.TotalAmount > 0)
            inv.ApplyPayment(inv.TotalAmount);

        return Result.Success();
    }

    private static Result<Guid> ResolveDirectDebitAccount(
        DirectPurchaseNature nature,
        Domain.Entities.Finance.AccountingSettings settings) => nature switch
    {
        DirectPurchaseNature.Inventory when settings.InventoryAccountId is not null
            => Result<Guid>.Success(settings.InventoryAccountId.Value),
        DirectPurchaseNature.Services when settings.PurchaseAccountId is not null
            => Result<Guid>.Success(settings.PurchaseAccountId.Value),
        DirectPurchaseNature.FixedAssets when (settings.FixedAssetAccountId ?? settings.PurchaseAccountId) is Guid fa
            => Result<Guid>.Success(fa),
        DirectPurchaseNature.Inventory
            => Result<Guid>.Failure("AccountsNotMapped", "Inventory account must be mapped."),
        DirectPurchaseNature.Services
            => Result<Guid>.Failure("AccountsNotMapped", "Purchase/expense account must be mapped for services."),
        DirectPurchaseNature.FixedAssets
            => Result<Guid>.Failure("AccountsNotMapped", "Fixed asset (or purchase) account must be mapped."),
        _ => Result<Guid>.Failure("AccountsNotMapped", "Unable to resolve debit account for invoice nature.")
    };

    private static string DebitLabel(DirectPurchaseNature nature) => nature switch
    {
        DirectPurchaseNature.Services => "Purchase expense / services",
        DirectPurchaseNature.FixedAssets => "Fixed asset",
        _ => "Inventory"
    };

    public async Task<Result> PostPurchaseReturnAsync(Guid purchaseReturnId, Guid userId, CancellationToken ct = default)
    {
        var ret = await context.PurchaseReturns.Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.Id == purchaseReturnId, ct);
        if (ret is null)
            return Result.Failure("PurchaseReturnNotFound", "Purchase return not found.");
        if (ret.Status != PurchasingDocumentStatus.Approved)
            return Result.Failure(ErrorCodes.InvalidStatusTransition, "Only approved returns can be posted.");
        if (!ret.Lines.Any())
            return Result.Failure("NoLines", "Cannot post a return with no lines.");

        var settings = await context.AccountingSettings.AsNoTracking()
            .FirstOrDefaultAsync(s => s.TenantId == ret.TenantId && s.CompanyId == null, ct);
        if (settings?.InventoryAccountId is null)
            return Result.Failure("AccountsNotMapped", "Inventory account must be mapped.");

        var stock = await pipeline.ApplyMovementAsync(new InventoryMovementRequest(
            ret.TenantId,
            InventoryMovementType.OUT,
            TransactionType.PurchaseReturn,
            ret.Id,
            ret.ReturnNumber,
            ret.Lines.Select(l => new InventoryMovementLine(
                l.InventoryItemId, ret.WarehouseId, l.UnitId, l.ReturnQuantity, l.UnitCost)).ToList(),
            ret.Notes), ct);
        if (stock.IsFailure)
            return Result.Failure(stock.ErrorCode!, stock.ErrorMessage);

        var net = ret.SubTotal;
        var tax = ret.TaxAmount;
        var total = ret.TotalAmount;
        var journalLines = new List<JournalLineDto>();

        if (ret.ReturnType == PurchaseReturnType.BeforeInvoice)
        {
            if (settings.GrniAccountId is null)
                return Result.Failure("AccountsNotMapped", "GRNI account must be mapped.");
            journalLines.Add(new(null, settings.GrniAccountId.Value, null, net, 0, "Reverse GRNI"));
            journalLines.Add(new(null, settings.InventoryAccountId.Value, null, 0, net, "Inventory OUT"));
        }
        else
        {
            PurchaseInvoice? inv = null;
            if (ret.PurchaseInvoiceId.HasValue)
                inv = await context.PurchaseInvoices.AsNoTracking()
                    .FirstOrDefaultAsync(i => i.Id == ret.PurchaseInvoiceId.Value, ct);

            var isCash = inv?.PaymentMode == PurchaseInvoicePaymentMode.Cash
                         && ret.ReturnType == PurchaseReturnType.Direct;

            Guid debitAccount;
            if (isCash)
            {
                if (settings.CashAccountId is null)
                    return Result.Failure("AccountsNotMapped", "Cash account must be mapped for cash returns.");
                debitAccount = settings.CashAccountId.Value;
            }
            else
            {
                if (settings.AccountsPayableAccountId is null)
                    return Result.Failure("AccountsNotMapped", "AP account must be mapped.");
                debitAccount = settings.AccountsPayableAccountId.Value;
            }

            journalLines.Add(new(null, debitAccount, null, total, 0,
                isCash ? "Cash refund" : "Supplier credit / AP reduction"));
            journalLines.Add(new(null, settings.InventoryAccountId.Value, null, 0, net, "Inventory OUT"));
            if (tax > 0)
            {
                if (settings.VatInputAccountId is null)
                    return Result.Failure("AccountsNotMapped", "VAT input account must be mapped.");
                journalLines.Add(new(null, settings.VatInputAccountId.Value, null, 0, tax, "Reverse input VAT"));
            }
        }

        var journal = await journalPosting.CreateAndPostAsync(ret.TenantId, userId, new CreateJournalDto(
            DateOnly.FromDateTime(ret.ReturnDate.UtcDateTime),
            $"PR {ret.ReturnNumber}",
            PostingSource.Purchase,
            SourceDocumentId: ret.Id,
            Reference: ret.ReturnNumber,
            Lines: journalLines), ct);
        if (!journal.IsSuccess)
            return Result.Failure(journal.ErrorCode!, journal.ErrorMessage!);

        // Credit note journal is the same AP/cash debit journal for after-invoice/direct credit.
        Guid? creditNoteJournalId = ret.ReturnType == PurchaseReturnType.BeforeInvoice
            ? null
            : journal.Data!.Id;

        await UpdateSourceReturnedQuantitiesAsync(ret, increase: true, ct);

        ret.MarkPosted(journal.Data!.Id, userId, creditNoteJournalId);
        context.PurchaseReturns.Update(ret);
        await context.SaveChangesAsync(ct);
        logger.LogInformation("Purchase return posted: {ReturnId}", ret.Id);
        return Result.Success();
    }

    public Task<Result> UnpostPurchaseReturnAsync(Guid purchaseReturnId, Guid userId, CancellationToken ct = default)
    {
        // Workflow: Save → Approve → Post. Approval and posting are irreversible.
        return Task.FromResult(Result.Failure(
            ErrorCodes.InvalidStatusTransition,
            "Posted purchase returns cannot be unposted."));
    }

    private async Task UpdateSourceReturnedQuantitiesAsync(PurchaseReturn ret, bool increase, CancellationToken ct)
    {
        var deltaFactor = increase ? 1m : -1m;

        if (ret.ReturnType == PurchaseReturnType.BeforeInvoice && ret.GoodsReceiptId.HasValue)
        {
            var gr = await context.GoodsReceipts.Include(g => g.Lines)
                .FirstOrDefaultAsync(g => g.Id == ret.GoodsReceiptId.Value, ct);
            if (gr is null) return;

            foreach (var line in ret.Lines)
            {
                var grLine = line.GoodsReceiptLineId.HasValue
                    ? gr.Lines.FirstOrDefault(l => l.Id == line.GoodsReceiptLineId.Value)
                    : gr.Lines.FirstOrDefault(l => l.InventoryItemId == line.InventoryItemId && l.UnitId == line.UnitId);
                grLine?.AddReturnedQuantity(line.ReturnQuantity * deltaFactor);
            }

            context.GoodsReceipts.Update(gr);
            return;
        }

        if (ret.PurchaseInvoiceId.HasValue)
        {
            var inv = await context.PurchaseInvoices.Include(i => i.Lines)
                .FirstOrDefaultAsync(i => i.Id == ret.PurchaseInvoiceId.Value, ct);
            if (inv is null) return;

            foreach (var line in ret.Lines)
            {
                var invLine = line.PurchaseInvoiceLineId.HasValue
                    ? inv.Lines.FirstOrDefault(l => l.Id == line.PurchaseInvoiceLineId.Value)
                    : inv.Lines.FirstOrDefault(l => l.InventoryItemId == line.InventoryItemId && l.UnitId == line.UnitId);
                invLine?.AddReturnedQuantity(line.ReturnQuantity * deltaFactor);
            }

            inv.RefreshReturnSettlement();
            context.PurchaseInvoices.Update(inv);
        }
    }

    private async Task UpdatePoReceivedQuantitiesAsync(GoodsReceipt gr, bool increase, CancellationToken ct)
    {
        if (!gr.PurchaseOrderId.HasValue) return;

        var po = await context.PurchaseOrders.Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.Id == gr.PurchaseOrderId.Value, ct);
        if (po is null) return;

        foreach (var grLine in gr.Lines)
        {
            var poLine = grLine.PurchaseOrderLineId.HasValue
                ? po.Lines.FirstOrDefault(l => l.Id == grLine.PurchaseOrderLineId.Value)
                : po.Lines.FirstOrDefault(l => l.InventoryItemId == grLine.InventoryItemId && l.UnitId == grLine.UnitId);

            if (poLine is null) continue;
            var qty = grLine.AcceptedQuantity > 0 ? grLine.AcceptedQuantity : grLine.ReceivedQuantity;
            if (increase)
                poLine.AddReceivedQuantity(qty);
            else
                poLine.AddReceivedQuantity(-qty);
        }

        if (increase)
            po.RecordReceiptDate(gr.ReceiptDate);

        var allReceived = po.Lines.All(l => l.ReceivedQuantity >= l.Quantity);
        var anyReceived = po.Lines.Any(l => l.ReceivedQuantity > 0);
        if (allReceived) po.MarkAsFullyReceived();
        else if (anyReceived) po.MarkAsPartiallyReceived();

        context.PurchaseOrders.Update(po);
    }

    /// <summary>
    /// Finds or creates a GRNI liability account and persists it on accounting settings.
    /// </summary>
    private async Task<Guid?> EnsureGrniAccountMappedAsync(AccountingSettings settings, CancellationToken ct)
    {
        var existing = await context.ChartOfAccounts.AsNoTracking()
            .Where(a => a.TenantId == settings.TenantId && a.IsActive && a.IsPostingAllowed)
            .Where(a =>
                a.AccountNumber == "GRNI" ||
                a.AccountNumber.StartsWith("GRNI") ||
                (a.NameEn != null && a.NameEn.Contains("GRNI")) ||
                a.NameAr.Contains("استلامات غير مفوترة") ||
                a.NameAr.Contains("بضاعة مستلمة غير مفوترة"))
            .Select(a => (Guid?)a.Id)
            .FirstOrDefaultAsync(ct);

        Guid grniId;
        if (existing.HasValue)
        {
            grniId = existing.Value;
        }
        else
        {
            var account = ChartOfAccount.Create(
                settings.TenantId,
                accountNumber: "2120-GRNI",
                nameAr: "استلامات غير مفوترة (GRNI)",
                accountType: AccountType.Liability,
                category: AccountCategory.CurrentLiability,
                isPostingAllowed: true,
                nameEn: "Goods Received Not Invoiced (GRNI)",
                notes: "Auto-created for purchase goods receipt posting");
            context.ChartOfAccounts.Add(account);
            await context.SaveChangesAsync(ct);
            grniId = account.Id;
        }

        settings.SetGrniAccount(grniId);
        context.AccountingSettings.Update(settings);
        await context.SaveChangesAsync(ct);
        logger.LogInformation("Mapped GRNI account {AccountId} for tenant {TenantId}", grniId, settings.TenantId);
        return grniId;
    }
}
