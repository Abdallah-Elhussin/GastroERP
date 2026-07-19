using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Interfaces.Inventory;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.DTOs;
using GastroErp.Application.Features.Finance.Services;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Entities.Sales.BackOffice;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Sales.BackOffice.Invoices;

public interface IBackOfficeSalesAccountingService
{
    Task<Result> PostInvoiceAsync(Guid invoiceId, Guid userId, CancellationToken ct = default);
    Task<Result> UnpostInvoiceAsync(Guid invoiceId, Guid userId, CancellationToken ct = default);
}

public sealed class BackOfficeSalesAccountingService(
    IApplicationDbContext context,
    IInventoryMovementPipeline pipeline,
    IJournalPostingService journalPosting,
    GastroErp.Application.Common.Interfaces.Logging.IAuditLogger auditLogger,
    ILogger<BackOfficeSalesAccountingService> logger) : IBackOfficeSalesAccountingService
{
    public async Task<Result> PostInvoiceAsync(Guid invoiceId, Guid userId, CancellationToken ct = default)
    {
        var inv = await context.BackOfficeSalesInvoices.Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == invoiceId, ct);
        if (inv is null)
            return Result.Failure("InvoiceNotFound", "Sales invoice not found.");
        if (inv.Status != BackOfficeSalesDocumentStatus.Approved)
            return Result.Failure(ErrorCodes.SalesInvalidStatusTransition, "Invoice must be approved before posting.");

        var settings = await context.AccountingSettings.AsNoTracking()
            .FirstOrDefaultAsync(s => s.TenantId == inv.TenantId && s.CompanyId == null, ct);
        if (settings?.SalesRevenueAccountId is null)
            return Result.Failure("AccountsNotMapped", "Sales revenue account must be mapped.");

        var debitAccount = inv.PaymentMode == BackOfficeSalesPaymentMode.Cash
            ? settings.CashAccountId ?? settings.BankAccountId
            : settings.AccountsReceivableAccountId;
        if (debitAccount is null)
            return Result.Failure("AccountsNotMapped",
                inv.PaymentMode == BackOfficeSalesPaymentMode.Cash
                    ? "Cash/bank account must be mapped for cash sales."
                    : "Accounts receivable must be mapped for credit sales.");

        var goodsNet = Math.Max(0, inv.SubTotal - inv.DiscountAmount);
        var journalLines = new List<JournalLineDto>
        {
            new(null, debitAccount.Value, inv.CostCenterId, inv.TotalAmount, 0,
                inv.PaymentMode == BackOfficeSalesPaymentMode.Cash ? "Cash / Bank" : "Customer AR"),
            new(null, settings.SalesRevenueAccountId.Value, inv.CostCenterId, 0, goodsNet, "Sales revenue")
        };
        if (inv.TaxAmount > 0)
        {
            if (settings.VatOutputAccountId is null)
                return Result.Failure("AccountsNotMapped", "VAT output account must be mapped.");
            journalLines.Add(new(null, settings.VatOutputAccountId.Value, null, 0, inv.TaxAmount, "Output VAT"));
        }

        var journal = await journalPosting.CreateAndPostAsync(inv.TenantId, userId, new CreateJournalDto(
            inv.InvoiceDate, $"SI {inv.InvoiceNumber}", PostingSource.Sales,
            SourceDocumentId: inv.Id, Reference: inv.InvoiceNumber, Lines: journalLines), ct);
        if (!journal.IsSuccess)
            return Result.Failure(journal.ErrorCode!, journal.ErrorMessage!);

        Guid? cogsJournalId = null;
        var stockLines = inv.Lines
            .Where(l => l.AffectsInventory && l.InventoryItemId.HasValue && l.UnitId.HasValue)
            .ToList();

        if (stockLines.Count > 0)
        {
            var movementLines = new List<InventoryMovementLine>();
            foreach (var line in stockLines)
            {
                var warehouseId = line.LineWarehouseId ?? inv.WarehouseId;
                if (warehouseId is null)
                    return Result.Failure("RequiredField", "Warehouse is required for inventory lines.");
                movementLines.Add(new InventoryMovementLine(
                    line.InventoryItemId!.Value, warehouseId.Value, line.UnitId!.Value,
                    line.Quantity, line.UnitCost > 0 ? line.UnitCost : null));
            }

            var stock = await pipeline.ApplyMovementAsync(new InventoryMovementRequest(
                inv.TenantId,
                InventoryMovementType.OUT,
                TransactionType.SalesConsumption,
                inv.Id,
                inv.InvoiceNumber,
                movementLines,
                inv.Notes), ct);
            if (stock.IsFailure)
                return Result.Failure(stock.ErrorCode!, stock.ErrorMessage);

            var cogsAmount = stockLines.Sum(l => l.Quantity * Math.Max(0, l.UnitCost));
            if (cogsAmount > 0 && settings.CogsAccountId is not null && settings.InventoryAccountId is not null)
            {
                var cogsJournal = await journalPosting.CreateAndPostAsync(inv.TenantId, userId, new CreateJournalDto(
                    inv.InvoiceDate, $"COGS {inv.InvoiceNumber}", PostingSource.Sales,
                    SourceDocumentId: inv.Id, Reference: inv.InvoiceNumber,
                    Lines:
                    [
                        new(null, settings.CogsAccountId.Value, inv.CostCenterId, cogsAmount, 0, "COGS"),
                        new(null, settings.InventoryAccountId.Value, null, 0, cogsAmount, "Inventory OUT")
                    ]), ct);
                if (!cogsJournal.IsSuccess)
                    return Result.Failure(cogsJournal.ErrorCode!, cogsJournal.ErrorMessage!);
                cogsJournalId = cogsJournal.Data!.Id;
            }
        }

        inv.MarkPosted(journal.Data!.Id, userId, cogsJournalId);
        context.BackOfficeSalesInvoices.Update(inv);

        await RegisterInvoicedQuantitiesAsync(inv, reverse: false, ct);

        await context.SaveChangesAsync(ct);
        logger.LogInformation("Back-office sales invoice posted: {InvoiceId}", inv.Id);
        auditLogger.LogAction("Post", nameof(BackOfficeSalesInvoice), inv.Id.ToString(),
            new { inv.InvoiceNumber, inv.CustomerId, UserId = userId });
        return Result.Success();
    }

    public async Task<Result> UnpostInvoiceAsync(Guid invoiceId, Guid userId, CancellationToken ct = default)
    {
        var inv = await context.BackOfficeSalesInvoices.Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == invoiceId, ct);
        if (inv is null)
            return Result.Failure("InvoiceNotFound", "Sales invoice not found.");
        if (inv.Status != BackOfficeSalesDocumentStatus.Posted)
            return Result.Failure(ErrorCodes.SalesInvalidStatusTransition, "Only posted invoices can be reversed.");
        if (inv.PaidAmount > 0)
            return Result.Failure("HasPayments", "Cannot reverse an invoice that has payments applied.");
        if (inv.Lines.Any(l => l.ReturnedQuantity > 0))
            return Result.Failure("HasReturns", "Cannot reverse an invoice that has returns.");

        var stockLines = inv.Lines
            .Where(l => l.AffectsInventory && l.InventoryItemId.HasValue && l.UnitId.HasValue)
            .ToList();
        if (stockLines.Count > 0)
        {
            var movementLines = new List<InventoryMovementLine>();
            foreach (var line in stockLines)
            {
                var warehouseId = line.LineWarehouseId ?? inv.WarehouseId;
                if (warehouseId is null)
                    return Result.Failure("RequiredField", "Warehouse is required to reverse inventory.");
                movementLines.Add(new InventoryMovementLine(
                    line.InventoryItemId!.Value, warehouseId.Value, line.UnitId!.Value,
                    line.Quantity, line.UnitCost > 0 ? line.UnitCost : null));
            }

            var inbound = await pipeline.ApplyMovementAsync(new InventoryMovementRequest(
                inv.TenantId,
                InventoryMovementType.IN,
                TransactionType.Reversal,
                inv.Id,
                $"REV-{inv.InvoiceNumber}",
                movementLines,
                $"Reverse {inv.InvoiceNumber}"), ct);
            if (inbound.IsFailure)
                return Result.Failure(inbound.ErrorCode!, inbound.ErrorMessage);
        }

        Guid? reversalId = null;
        if (inv.CogsJournalEntryId.HasValue)
        {
            var cogsRev = await journalPosting.ReverseAsync(inv.CogsJournalEntryId.Value, userId, ct);
            if (!cogsRev.IsSuccess)
                return Result.Failure(cogsRev.ErrorCode!, cogsRev.ErrorMessage!);
        }

        if (inv.JournalEntryId.HasValue)
        {
            var rev = await journalPosting.ReverseAsync(inv.JournalEntryId.Value, userId, ct);
            if (!rev.IsSuccess)
                return Result.Failure(rev.ErrorCode!, rev.ErrorMessage!);
            reversalId = rev.Data!.Id;
        }

        inv.MarkReversed(reversalId);
        context.BackOfficeSalesInvoices.Update(inv);

        await RegisterInvoicedQuantitiesAsync(inv, reverse: true, ct);

        await context.SaveChangesAsync(ct);
        logger.LogInformation("Back-office sales invoice reversed: {InvoiceId}", inv.Id);
        return Result.Success();
    }

    private async Task RegisterInvoicedQuantitiesAsync(
        Domain.Entities.Sales.BackOffice.BackOfficeSalesInvoice inv, bool reverse, CancellationToken ct)
    {
        if (!inv.BackOfficeSalesOrderId.HasValue) return;

        var order = await context.BackOfficeSalesOrders.Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == inv.BackOfficeSalesOrderId!.Value, ct);
        if (order is null) return;

        foreach (var line in inv.Lines)
        {
            if (line.SalesOrderLineId is null) continue;
            if (reverse)
                order.RegisterInvoiceReversal(line.SalesOrderLineId.Value, line.Quantity);
            else
                order.RegisterInvoice(line.SalesOrderLineId.Value, line.Quantity);
        }
        context.BackOfficeSalesOrders.Update(order);
    }
}
