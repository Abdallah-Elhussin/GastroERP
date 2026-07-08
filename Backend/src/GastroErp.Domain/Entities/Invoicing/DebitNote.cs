using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Invoicing;

namespace GastroErp.Domain.Entities.Invoicing;

/// <summary>DebitNote — إشعار مدين (Aggregate Root)</summary>
public sealed class DebitNote : AuditableBaseEntity, ITenantEntity, IBranchEntity
{
    public Guid TenantId { get; private set; }
    public Guid BranchId { get; private set; }
    public string DebitNoteNumber { get; private set; }
    public Guid OriginalInvoiceId { get; private set; }
    public DebitNoteStatus Status { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal TaxTotal { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Currency { get; private set; }
    public string Reason { get; private set; }
    public DateTimeOffset? IssuedAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }

    private readonly List<DebitNoteLine> _lines = [];
    public IReadOnlyCollection<DebitNoteLine> Lines => _lines.AsReadOnly();

    private DebitNote()
    {
        DebitNoteNumber = string.Empty;
        Currency = "SAR";
        Reason = string.Empty;
    }

    public static DebitNote CreateDraft(
        Guid tenantId, Guid branchId, string debitNoteNumber,
        Guid originalInvoiceId, string reason, string currency = "SAR")
    {
        if (string.IsNullOrWhiteSpace(reason)) throw new BusinessException(ErrorCodes.RequiredField);
        if (originalInvoiceId == Guid.Empty) throw new BusinessException(ErrorCodes.InvoiceReferenceRequired);

        return new DebitNote
        {
            TenantId = tenantId,
            BranchId = branchId,
            DebitNoteNumber = debitNoteNumber,
            OriginalInvoiceId = originalInvoiceId,
            Status = DebitNoteStatus.Draft,
            Reason = reason,
            Currency = currency.ToUpperInvariant()
        };
    }

    public DebitNoteLine AddLine(
        int lineNumber, string descriptionAr, string? descriptionEn,
        decimal quantity, decimal unitPrice, decimal taxAmount)
    {
        if (Status != DebitNoteStatus.Draft)
            throw new BusinessException(ErrorCodes.DebitNoteNotEditable);

        var netAmount = Math.Round(quantity * unitPrice, 4);
        var totalAmount = Math.Round(netAmount + taxAmount, 4);
        if (taxAmount < 0) throw new BusinessException(ErrorCodes.InvalidTaxAmount);

        var line = new DebitNoteLine(
            Id, lineNumber, descriptionAr, descriptionEn,
            quantity, unitPrice, taxAmount, netAmount, totalAmount, Currency);

        _lines.Add(line);
        RecalculateTotals();
        return line;
    }

    public void Issue()
    {
        if (Status != DebitNoteStatus.Draft)
            throw new BusinessException(ErrorCodes.DebitNoteAlreadyIssued);
        if (!_lines.Any())
            throw new BusinessException(ErrorCodes.DebitNoteHasNoLines);

        Status = DebitNoteStatus.Issued;
        IssuedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new DebitNoteIssuedEvent(Id, OriginalInvoiceId, TotalAmount));
    }

    public void Cancel(string reason)
    {
        if (Status == DebitNoteStatus.Cancelled)
            throw new BusinessException(ErrorCodes.DebitNoteAlreadyCancelled);
        if (string.IsNullOrWhiteSpace(reason))
            throw new BusinessException(ErrorCodes.CancellationReasonRequired);

        Status = DebitNoteStatus.Cancelled;
        CancellationReason = reason;
        CancelledAt = DateTimeOffset.UtcNow;
    }

    private void RecalculateTotals()
    {
        SubTotal = _lines.Sum(l => l.NetAmount);
        TaxTotal = _lines.Sum(l => l.TaxAmount);
        TotalAmount = _lines.Sum(l => l.TotalAmount);
    }
}

public sealed class DebitNoteLine : AuditableBaseEntity
{
    public Guid DebitNoteId { get; private set; }
    public int LineNumber { get; private set; }
    public string DescriptionAr { get; private set; }
    public string? DescriptionEn { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal NetAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Currency { get; private set; }

    private DebitNoteLine() { DescriptionAr = string.Empty; Currency = "SAR"; }

    internal DebitNoteLine(
        Guid debitNoteId, int lineNumber, string descriptionAr, string? descriptionEn,
        decimal quantity, decimal unitPrice, decimal taxAmount,
        decimal netAmount, decimal totalAmount, string currency)
    {
        DebitNoteId = debitNoteId;
        LineNumber = lineNumber;
        DescriptionAr = descriptionAr;
        DescriptionEn = descriptionEn;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TaxAmount = taxAmount;
        NetAmount = netAmount;
        TotalAmount = totalAmount;
        Currency = currency.ToUpperInvariant();
    }
}
