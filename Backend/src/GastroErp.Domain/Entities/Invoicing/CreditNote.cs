using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Invoicing;

namespace GastroErp.Domain.Entities.Invoicing;

/// <summary>CreditNote — إشعار دائن (Aggregate Root)</summary>
public sealed class CreditNote : AuditableBaseEntity, ITenantEntity, IBranchEntity
{
    public Guid TenantId { get; private set; }
    public Guid BranchId { get; private set; }
    public string CreditNoteNumber { get; private set; }
    public Guid OriginalInvoiceId { get; private set; }
    public CreditNoteType CreditType { get; private set; }
    public CreditNoteStatus Status { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal TaxTotal { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Currency { get; private set; }
    public string Reason { get; private set; }
    public DateTimeOffset? IssuedAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }

    private readonly List<CreditNoteLine> _lines = [];
    public IReadOnlyCollection<CreditNoteLine> Lines => _lines.AsReadOnly();

    private CreditNote()
    {
        CreditNoteNumber = string.Empty;
        Currency = "SAR";
        Reason = string.Empty;
    }

    public static CreditNote CreateDraft(
        Guid tenantId, Guid branchId, string creditNoteNumber,
        Guid originalInvoiceId, CreditNoteType creditType, string reason, string currency = "SAR")
    {
        if (string.IsNullOrWhiteSpace(reason)) throw new BusinessException(ErrorCodes.RequiredField);
        if (originalInvoiceId == Guid.Empty) throw new BusinessException(ErrorCodes.InvoiceNotFound);

        return new CreditNote
        {
            TenantId = tenantId,
            BranchId = branchId,
            CreditNoteNumber = creditNoteNumber,
            OriginalInvoiceId = originalInvoiceId,
            CreditType = creditType,
            Status = CreditNoteStatus.Draft,
            Reason = reason,
            Currency = currency.ToUpperInvariant()
        };
    }

    public CreditNoteLine AddLine(
        Guid? invoiceLineId, int lineNumber, Guid productId,
        string productNameAr, string? productNameEn, decimal quantity,
        decimal unitPrice, decimal taxAmount)
    {
        if (Status != CreditNoteStatus.Draft)
            throw new BusinessException(ErrorCodes.CreditNoteNotEditable);

        var netAmount = Math.Round(quantity * unitPrice, 4);
        var totalAmount = Math.Round(netAmount + taxAmount, 4);
        if (taxAmount < 0) throw new BusinessException(ErrorCodes.InvalidTaxAmount);

        var line = new CreditNoteLine(
            Id, invoiceLineId, lineNumber, productId, productNameAr, productNameEn,
            quantity, unitPrice, taxAmount, netAmount, totalAmount, Currency);

        _lines.Add(line);
        RecalculateTotals();
        return line;
    }

    public void Issue(decimal maxInvoiceAmount, decimal alreadyCredited)
    {
        if (Status != CreditNoteStatus.Draft)
            throw new BusinessException(ErrorCodes.CreditNoteAlreadyIssued);
        if (!_lines.Any())
            throw new BusinessException(ErrorCodes.CreditNoteHasNoLines);
        if (TotalAmount + alreadyCredited > maxInvoiceAmount)
            throw new BusinessException(ErrorCodes.CreditNoteExceedsInvoice);

        Status = CreditNoteStatus.Issued;
        IssuedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new CreditNoteIssuedEvent(Id, OriginalInvoiceId, TotalAmount, CreditType));
    }

    public void Cancel(string reason)
    {
        if (Status == CreditNoteStatus.Cancelled)
            throw new BusinessException(ErrorCodes.CreditNoteAlreadyCancelled);
        if (string.IsNullOrWhiteSpace(reason))
            throw new BusinessException(ErrorCodes.CancellationReasonRequired);

        Status = CreditNoteStatus.Cancelled;
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

public sealed class CreditNoteLine : AuditableBaseEntity
{
    public Guid CreditNoteId { get; private set; }
    public Guid? InvoiceLineId { get; private set; }
    public int LineNumber { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductNameAr { get; private set; }
    public string? ProductNameEn { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal NetAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Currency { get; private set; }

    private CreditNoteLine() { ProductNameAr = string.Empty; Currency = "SAR"; }

    internal CreditNoteLine(
        Guid creditNoteId, Guid? invoiceLineId, int lineNumber, Guid productId,
        string productNameAr, string? productNameEn, decimal quantity, decimal unitPrice,
        decimal taxAmount, decimal netAmount, decimal totalAmount, string currency)
    {
        CreditNoteId = creditNoteId;
        InvoiceLineId = invoiceLineId;
        LineNumber = lineNumber;
        ProductId = productId;
        ProductNameAr = productNameAr;
        ProductNameEn = productNameEn;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TaxAmount = taxAmount;
        NetAmount = netAmount;
        TotalAmount = totalAmount;
        Currency = currency.ToUpperInvariant();
    }
}
