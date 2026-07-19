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

namespace GastroErp.Application.Features.Sales.BackOffice.Fulfillment;

/// <summary>
/// خدمة ترحيل/عكس مستندات المبيعات الإدارية الفرعية:
/// - إذن تسليم: يخرج مخزون + يخفض رصيد أمر البيع الإداري.
/// - مرتجع مبيعات: يدخل مخزون + قيد عكس إيراد/ضريبة + يزيد الكميات المرتجعة على بنود الفاتورة.
/// - إشعار مدين: قيد Debit AR / Credit Revenue.
/// </summary>
public sealed class BackOfficeSalesFulfillmentService(
    IApplicationDbContext context,
    IInventoryMovementPipeline inventoryPipeline,
    IJournalPostingService journalPosting,
    GastroErp.Application.Common.Interfaces.Logging.IAuditLogger auditLogger,
    ILogger<BackOfficeSalesFulfillmentService> logger) : IBackOfficeSalesFulfillmentService
{
    // ─── Delivery Note ──────────────────────────────────────────────────────

    public async Task<Result> PostDeliveryAsync(Guid deliveryNoteId, Guid userId, CancellationToken ct = default)
    {
        var note = await context.BackOfficeSalesDeliveryNotes.Include(d => d.Lines)
            .FirstOrDefaultAsync(d => d.Id == deliveryNoteId, ct);
        if (note is null)
            return Result.Failure("DeliveryNoteNotFound", "Delivery note not found.");
        if (note.Status != BackOfficeSalesDocumentStatus.Approved)
            return Result.Failure(ErrorCodes.SalesInvalidStatusTransition,
                "Delivery note must be approved before posting.");

        var order = await context.BackOfficeSalesOrders.Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == note.OrderId, ct);
        if (order is null)
            return Result.Failure("OrderNotFound", "Related sales order not found.");
        if (order.Status is not (BackOfficeSalesDocumentStatus.Approved or BackOfficeSalesDocumentStatus.Posted))
            return Result.Failure(ErrorCodes.SalesInvalidStatusTransition,
                "Related sales order must be approved before delivery posting.");

        foreach (var line in note.Lines)
        {
            var orderLine = order.Lines.FirstOrDefault(l => l.Id == line.OrderLineId);
            if (orderLine is null)
                return Result.Failure("OrderLineNotFound",
                    $"Order line '{line.OrderLineId}' not found on order '{order.OrderNumber}'.");
            if (line.Quantity > orderLine.RemainingToDeliver + 0.0001m)
                return Result.Failure(ErrorCodes.InvalidQuantity,
                    $"Cannot deliver more than remaining quantity for '{orderLine.Description}'.");
        }

        var stockLines = note.Lines
            .Where(l => l.InventoryItemId.HasValue && l.UnitId.HasValue)
            .ToList();

        if (stockLines.Count > 0)
        {
            var movementLines = stockLines
                .Select(l => new InventoryMovementLine(
                    l.InventoryItemId!.Value,
                    note.WarehouseId,
                    l.UnitId!.Value,
                    l.Quantity,
                    l.UnitCost > 0 ? l.UnitCost : null))
                .ToList();

            var outbound = await inventoryPipeline.ApplyMovementAsync(new InventoryMovementRequest(
                note.TenantId,
                InventoryMovementType.OUT,
                TransactionType.SalesConsumption,
                note.Id,
                note.DeliveryNumber,
                movementLines,
                note.Notes), ct);
            if (outbound.IsFailure)
                return Result.Failure(outbound.ErrorCode!, outbound.ErrorMessage);
        }

        foreach (var line in note.Lines)
            order.RegisterDelivery(line.OrderLineId, line.Quantity);

        note.MarkPosted(journalEntryId: null, userId);

        context.BackOfficeSalesDeliveryNotes.Update(note);
        context.BackOfficeSalesOrders.Update(order);
        await context.SaveChangesAsync(ct);
        logger.LogInformation("Back-office sales delivery note posted: {DeliveryNoteId}", note.Id);
        auditLogger.LogAction("Post", nameof(BackOfficeSalesDeliveryNote), note.Id.ToString(),
            new { note.DeliveryNumber, note.OrderId, UserId = userId });
        return Result.Success();
    }

    public async Task<Result> UnpostDeliveryAsync(Guid deliveryNoteId, Guid userId, CancellationToken ct = default)
    {
        var note = await context.BackOfficeSalesDeliveryNotes.Include(d => d.Lines)
            .FirstOrDefaultAsync(d => d.Id == deliveryNoteId, ct);
        if (note is null)
            return Result.Failure("DeliveryNoteNotFound", "Delivery note not found.");
        if (note.Status != BackOfficeSalesDocumentStatus.Posted)
            return Result.Failure(ErrorCodes.SalesInvalidStatusTransition,
                "Only posted delivery notes can be reversed.");

        var order = await context.BackOfficeSalesOrders.Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == note.OrderId, ct);
        if (order is null)
            return Result.Failure("OrderNotFound", "Related sales order not found.");

        var stockLines = note.Lines
            .Where(l => l.InventoryItemId.HasValue && l.UnitId.HasValue)
            .ToList();

        if (stockLines.Count > 0)
        {
            var movementLines = stockLines
                .Select(l => new InventoryMovementLine(
                    l.InventoryItemId!.Value,
                    note.WarehouseId,
                    l.UnitId!.Value,
                    l.Quantity,
                    l.UnitCost > 0 ? l.UnitCost : null))
                .ToList();

            var inbound = await inventoryPipeline.ApplyMovementAsync(new InventoryMovementRequest(
                note.TenantId,
                InventoryMovementType.IN,
                TransactionType.Reversal,
                note.Id,
                $"REV-{note.DeliveryNumber}",
                movementLines,
                $"Reverse {note.DeliveryNumber}"), ct);
            if (inbound.IsFailure)
                return Result.Failure(inbound.ErrorCode!, inbound.ErrorMessage);
        }

        foreach (var line in note.Lines)
            order.RegisterDeliveryReversal(line.OrderLineId, line.Quantity);

        note.MarkReversed();

        context.BackOfficeSalesDeliveryNotes.Update(note);
        context.BackOfficeSalesOrders.Update(order);
        await context.SaveChangesAsync(ct);
        logger.LogInformation("Back-office sales delivery note reversed: {DeliveryNoteId}", note.Id);
        return Result.Success();
    }

    // ─── Return ─────────────────────────────────────────────────────────────

    public async Task<Result> PostReturnAsync(Guid returnId, Guid userId, CancellationToken ct = default)
    {
        var ret = await context.BackOfficeSalesReturns.Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.Id == returnId, ct);
        if (ret is null)
            return Result.Failure("ReturnNotFound", "Sales return not found.");
        if (ret.Status != BackOfficeSalesDocumentStatus.Approved)
            return Result.Failure(ErrorCodes.SalesInvalidStatusTransition,
                "Return must be approved before posting.");

        var invoice = await context.BackOfficeSalesInvoices.Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == ret.InvoiceId, ct);
        if (invoice is null)
            return Result.Failure("InvoiceNotFound", "Related invoice not found.");
        if (invoice.Status != BackOfficeSalesDocumentStatus.Posted)
            return Result.Failure(ErrorCodes.SalesInvalidStatusTransition,
                "Invoice must be posted before it can be returned.");

        foreach (var line in ret.Lines)
        {
            var invLine = invoice.Lines.FirstOrDefault(l => l.Id == line.InvoiceLineId);
            if (invLine is null)
                return Result.Failure("InvoiceLineNotFound",
                    $"Invoice line '{line.InvoiceLineId}' not found on invoice '{invoice.InvoiceNumber}'.");
            if (line.Quantity > invLine.RemainingToReturn + 0.0001m)
                return Result.Failure(ErrorCodes.InvalidQuantity,
                    $"Cannot return more than remaining quantity for '{invLine.Description}'.");
        }

        var settings = await context.AccountingSettings.AsNoTracking()
            .FirstOrDefaultAsync(s => s.TenantId == ret.TenantId && s.CompanyId == null, ct);
        if (settings?.SalesRevenueAccountId is null)
            return Result.Failure("AccountsNotMapped", "Sales revenue account must be mapped.");

        var creditAccount = invoice.PaymentMode == BackOfficeSalesPaymentMode.Cash
            ? settings.CashAccountId ?? settings.BankAccountId
            : settings.AccountsReceivableAccountId;
        if (creditAccount is null)
            return Result.Failure("AccountsNotMapped",
                invoice.PaymentMode == BackOfficeSalesPaymentMode.Cash
                    ? "Cash/bank account must be mapped."
                    : "Accounts receivable must be mapped.");

        var goodsNet = Math.Max(0, ret.SubTotal - ret.DiscountAmount);
        var journalLines = new List<JournalLineDto>
        {
            new(null, settings.SalesRevenueAccountId.Value, invoice.CostCenterId, goodsNet, 0, "Sales returns"),
            new(null, creditAccount.Value, invoice.CostCenterId, 0, ret.TotalAmount,
                invoice.PaymentMode == BackOfficeSalesPaymentMode.Cash ? "Cash / Bank refund" : "Customer AR reduction")
        };
        if (ret.TaxAmount > 0)
        {
            if (settings.VatOutputAccountId is null)
                return Result.Failure("AccountsNotMapped", "VAT output account must be mapped.");
            journalLines.Insert(1, new(null, settings.VatOutputAccountId.Value, null,
                ret.TaxAmount, 0, "Reverse output VAT"));
        }

        var journal = await journalPosting.CreateAndPostAsync(ret.TenantId, userId, new CreateJournalDto(
            ret.ReturnDate, $"SR {ret.ReturnNumber}", PostingSource.CreditNote,
            SourceDocumentId: ret.Id, Reference: ret.ReturnNumber, Lines: journalLines), ct);
        if (!journal.IsSuccess)
            return Result.Failure(journal.ErrorCode!, journal.ErrorMessage!);

        var stockLines = ret.Lines
            .Where(l => l.InventoryItemId.HasValue && l.UnitId.HasValue
                        && l.LineNature == BackOfficeSalesLineNature.Inventory)
            .ToList();
        if (stockLines.Count > 0 && ret.WarehouseId.HasValue)
        {
            var movementLines = stockLines
                .Select(l => new InventoryMovementLine(
                    l.InventoryItemId!.Value,
                    ret.WarehouseId!.Value,
                    l.UnitId!.Value,
                    l.Quantity,
                    l.UnitCost > 0 ? l.UnitCost : null))
                .ToList();

            var inbound = await inventoryPipeline.ApplyMovementAsync(new InventoryMovementRequest(
                ret.TenantId,
                InventoryMovementType.IN,
                TransactionType.SalesConsumption,
                ret.Id,
                ret.ReturnNumber,
                movementLines,
                ret.Notes), ct);
            if (inbound.IsFailure)
                return Result.Failure(inbound.ErrorCode!, inbound.ErrorMessage);
        }

        foreach (var line in ret.Lines)
        {
            var invLine = invoice.Lines.First(l => l.Id == line.InvoiceLineId);
            invLine.AddReturnedQuantity(line.Quantity);
        }

        ret.MarkPosted(journal.Data!.Id, userId);

        context.BackOfficeSalesReturns.Update(ret);
        context.BackOfficeSalesInvoices.Update(invoice);
        await context.SaveChangesAsync(ct);
        logger.LogInformation("Back-office sales return posted: {ReturnId}", ret.Id);
        auditLogger.LogAction("Post", nameof(BackOfficeSalesReturn), ret.Id.ToString(),
            new { ret.ReturnNumber, ret.InvoiceId, UserId = userId });
        return Result.Success();
    }

    public async Task<Result> UnpostReturnAsync(Guid returnId, Guid userId, CancellationToken ct = default)
    {
        var ret = await context.BackOfficeSalesReturns.Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.Id == returnId, ct);
        if (ret is null)
            return Result.Failure("ReturnNotFound", "Sales return not found.");
        if (ret.Status != BackOfficeSalesDocumentStatus.Posted)
            return Result.Failure(ErrorCodes.SalesInvalidStatusTransition,
                "Only posted returns can be reversed.");

        var invoice = await context.BackOfficeSalesInvoices.Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == ret.InvoiceId, ct);
        if (invoice is null)
            return Result.Failure("InvoiceNotFound", "Related invoice not found.");

        var stockLines = ret.Lines
            .Where(l => l.InventoryItemId.HasValue && l.UnitId.HasValue
                        && l.LineNature == BackOfficeSalesLineNature.Inventory)
            .ToList();
        if (stockLines.Count > 0 && ret.WarehouseId.HasValue)
        {
            var movementLines = stockLines
                .Select(l => new InventoryMovementLine(
                    l.InventoryItemId!.Value,
                    ret.WarehouseId!.Value,
                    l.UnitId!.Value,
                    l.Quantity,
                    l.UnitCost > 0 ? l.UnitCost : null))
                .ToList();

            var outbound = await inventoryPipeline.ApplyMovementAsync(new InventoryMovementRequest(
                ret.TenantId,
                InventoryMovementType.OUT,
                TransactionType.Reversal,
                ret.Id,
                $"REV-{ret.ReturnNumber}",
                movementLines,
                $"Reverse {ret.ReturnNumber}"), ct);
            if (outbound.IsFailure)
                return Result.Failure(outbound.ErrorCode!, outbound.ErrorMessage);
        }

        Guid? reversalJournalId = null;
        if (ret.JournalEntryId.HasValue)
        {
            var rev = await journalPosting.ReverseAsync(ret.JournalEntryId.Value, userId, ct);
            if (!rev.IsSuccess)
                return Result.Failure(rev.ErrorCode!, rev.ErrorMessage!);
            reversalJournalId = rev.Data!.Id;
        }

        foreach (var line in ret.Lines)
        {
            var invLine = invoice.Lines.First(l => l.Id == line.InvoiceLineId);
            invLine.AddReturnedQuantity(-line.Quantity);
        }

        ret.MarkReversed(reversalJournalId);

        context.BackOfficeSalesReturns.Update(ret);
        context.BackOfficeSalesInvoices.Update(invoice);
        await context.SaveChangesAsync(ct);
        logger.LogInformation("Back-office sales return reversed: {ReturnId}", ret.Id);
        return Result.Success();
    }

    // ─── Debit Note ─────────────────────────────────────────────────────────

    public async Task<Result> PostDebitNoteAsync(Guid debitNoteId, Guid userId, CancellationToken ct = default)
    {
        var note = await context.BackOfficeSalesDebitNotes.Include(d => d.Lines)
            .FirstOrDefaultAsync(d => d.Id == debitNoteId, ct);
        if (note is null)
            return Result.Failure("DebitNoteNotFound", "Debit note not found.");
        if (note.Status != BackOfficeSalesDocumentStatus.Approved)
            return Result.Failure(ErrorCodes.SalesInvalidStatusTransition,
                "Debit note must be approved before posting.");

        var settings = await context.AccountingSettings.AsNoTracking()
            .FirstOrDefaultAsync(s => s.TenantId == note.TenantId && s.CompanyId == null, ct);
        if (settings?.SalesRevenueAccountId is null || settings.AccountsReceivableAccountId is null)
            return Result.Failure("AccountsNotMapped",
                "Sales revenue and accounts receivable accounts must be mapped.");

        var journalLines = new List<JournalLineDto>
        {
            new(null, settings.AccountsReceivableAccountId.Value, null, note.TotalAmount, 0,
                $"Customer AR — Debit note {note.DebitNoteNumber}"),
            new(null, settings.SalesRevenueAccountId.Value, null, 0, note.SubTotal,
                $"Additional revenue — {note.DebitNoteNumber}")
        };
        if (note.TaxAmount > 0)
        {
            if (settings.VatOutputAccountId is null)
                return Result.Failure("AccountsNotMapped", "VAT output account must be mapped.");
            journalLines.Add(new(null, settings.VatOutputAccountId.Value, null, 0, note.TaxAmount, "Output VAT"));
        }

        var journal = await journalPosting.CreateAndPostAsync(note.TenantId, userId, new CreateJournalDto(
            note.DebitDate, $"SDN {note.DebitNoteNumber}", PostingSource.DebitNote,
            SourceDocumentId: note.Id, Reference: note.DebitNoteNumber, Lines: journalLines), ct);
        if (!journal.IsSuccess)
            return Result.Failure(journal.ErrorCode!, journal.ErrorMessage!);

        note.MarkPosted(journal.Data!.Id, userId);
        context.BackOfficeSalesDebitNotes.Update(note);
        await context.SaveChangesAsync(ct);
        logger.LogInformation("Back-office sales debit note posted: {DebitNoteId}", note.Id);
        auditLogger.LogAction("Post", nameof(BackOfficeSalesDebitNote), note.Id.ToString(),
            new { note.DebitNoteNumber, note.InvoiceId, UserId = userId });
        return Result.Success();
    }

    public async Task<Result> UnpostDebitNoteAsync(Guid debitNoteId, Guid userId, CancellationToken ct = default)
    {
        var note = await context.BackOfficeSalesDebitNotes
            .FirstOrDefaultAsync(d => d.Id == debitNoteId, ct);
        if (note is null)
            return Result.Failure("DebitNoteNotFound", "Debit note not found.");
        if (note.Status != BackOfficeSalesDocumentStatus.Posted)
            return Result.Failure(ErrorCodes.SalesInvalidStatusTransition,
                "Only posted debit notes can be reversed.");

        Guid? reversalId = null;
        if (note.JournalEntryId.HasValue)
        {
            var rev = await journalPosting.ReverseAsync(note.JournalEntryId.Value, userId, ct);
            if (!rev.IsSuccess)
                return Result.Failure(rev.ErrorCode!, rev.ErrorMessage!);
            reversalId = rev.Data!.Id;
        }

        note.MarkReversed(reversalId);
        context.BackOfficeSalesDebitNotes.Update(note);
        await context.SaveChangesAsync(ct);
        logger.LogInformation("Back-office sales debit note reversed: {DebitNoteId}", note.Id);
        return Result.Success();
    }
}
