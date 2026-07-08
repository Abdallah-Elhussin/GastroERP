using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Invoicing;

namespace GastroErp.Domain.Entities.Invoicing;

/// <summary>Invoice — فاتورة ضريبية (Aggregate Root)</summary>
public sealed class Invoice : AuditableBaseEntity, ITenantEntity, ICompanyEntity, IBranchEntity
{
    public Guid TenantId { get; private set; }
    public Guid CompanyId { get; private set; }
    public Guid BranchId { get; private set; }
    public string InvoiceNumber { get; private set; }
    public InvoiceType InvoiceType { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public Guid? SalesOrderId { get; private set; }
    public Guid? CustomerId { get; private set; }
    public string? CustomerName { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal DiscountTotal { get; private set; }
    public decimal TaxTotal { get; private set; }
    public decimal GrandTotal { get; private set; }
    public decimal PaidAmount { get; private set; }
    public decimal CreditedAmount { get; private set; }
    public InvoicePaymentStatus PaymentStatus { get; private set; }
    public string Currency { get; private set; }
    public DateTimeOffset IssuedAt { get; private set; }
    public DateTimeOffset? FinalizedAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }
    public Guid? CancelledBy { get; private set; }
    public Guid? AuthorizedBy { get; private set; }
    public int PrintCount { get; private set; }
    public DateTimeOffset? LastPrintedAt { get; private set; }
    public string? Notes { get; private set; }

    public decimal RemainingBalance => Math.Max(0, GrandTotal - PaidAmount - CreditedAmount);

    private readonly List<InvoiceLine> _lines = [];
    public IReadOnlyCollection<InvoiceLine> Lines => _lines.AsReadOnly();

    private readonly List<InvoiceTaxLine> _taxLines = [];
    public IReadOnlyCollection<InvoiceTaxLine> TaxLines => _taxLines.AsReadOnly();

    private Invoice()
    {
        InvoiceNumber = string.Empty;
        Currency = "SAR";
    }

    public static Invoice CreateDraft(
        Guid tenantId, Guid companyId, Guid branchId, string invoiceNumber,
        InvoiceType invoiceType, string currency = "SAR",
        Guid? salesOrderId = null, Guid? customerId = null, string? customerName = null, string? notes = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(invoiceNumber)) throw new ArgumentException("InvoiceNumber cannot be empty.", nameof(invoiceNumber));

        var invoice = new Invoice
        {
            TenantId = tenantId,
            CompanyId = companyId,
            BranchId = branchId,
            InvoiceNumber = invoiceNumber,
            InvoiceType = invoiceType,
            Status = InvoiceStatus.Draft,
            SalesOrderId = salesOrderId,
            CustomerId = customerId,
            CustomerName = customerName,
            Currency = currency.ToUpperInvariant(),
            IssuedAt = DateTimeOffset.UtcNow,
            PaymentStatus = InvoicePaymentStatus.Unpaid,
            Notes = notes
        };

        invoice.RaiseDomainEvent(new InvoiceCreatedEvent(invoice.Id, tenantId, branchId, invoiceType, invoiceNumber));
        return invoice;
    }

    public InvoiceLine AddLine(
        Guid? orderItemId, int lineNumber, Guid productId,
        string productNameAr, string? productNameEn, string? sku,
        decimal quantity, decimal unitPrice, decimal discountAmount,
        decimal taxAmount, Guid? taxRateId = null)
    {
        EnsureModifiable();
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive.", nameof(quantity));
        if (unitPrice < 0) throw new BusinessException(ErrorCodes.InvalidInvoiceAmount);
        if (discountAmount < 0 || taxAmount < 0) throw new BusinessException(ErrorCodes.InvalidTaxAmount);

        var netAmount = Math.Round((quantity * unitPrice) - discountAmount, 4);
        var totalAmount = Math.Round(netAmount + taxAmount, 4);

        var line = new InvoiceLine(
            Id, orderItemId, lineNumber, productId, productNameAr, productNameEn, sku,
            quantity, unitPrice, discountAmount, taxAmount, netAmount, totalAmount, Currency, taxRateId);

        _lines.Add(line);
        RecalculateTotals();
        return line;
    }

    public void AddTaxLine(string taxNameAr, string? taxNameEn, decimal rate, decimal taxableAmount, decimal taxAmount, bool isInclusive)
    {
        EnsureModifiable();
        if (rate < 0 || taxAmount < 0) throw new BusinessException(ErrorCodes.InvalidTaxAmount);

        _taxLines.Add(new InvoiceTaxLine(Id, taxNameAr, taxNameEn, rate, taxableAmount, taxAmount, Currency, isInclusive));
        RecalculateTotals();
        RaiseDomainEvent(new TaxCalculatedEvent(Id, nameof(Invoice), taxAmount, Currency));
    }

    public void Finalize(Guid finalizedBy)
    {
        if (Status != InvoiceStatus.Draft)
            throw new BusinessException(ErrorCodes.InvoiceAlreadyFinalized);
        if (!_lines.Any())
            throw new BusinessException(ErrorCodes.InvoiceHasNoLines);

        RecalculateTotals();
        Status = InvoiceStatus.Finalized;
        FinalizedAt = DateTimeOffset.UtcNow;
        SetUpdated(finalizedBy.ToString());
        RaiseDomainEvent(new InvoiceFinalizedEvent(Id, SalesOrderId, GrandTotal, Currency));
    }

    public void Cancel(string reason, Guid cancelledBy, Guid? authorizedBy)
    {
        if (Status == InvoiceStatus.Cancelled)
            throw new BusinessException(ErrorCodes.InvoiceAlreadyCancelled);
        if (Status == InvoiceStatus.Finalized && authorizedBy is null)
            throw new BusinessException(ErrorCodes.ManagerApprovalRequired);
        if (string.IsNullOrWhiteSpace(reason))
            throw new BusinessException(ErrorCodes.CancellationReasonRequired);

        Status = InvoiceStatus.Cancelled;
        CancellationReason = reason;
        CancelledBy = cancelledBy;
        AuthorizedBy = authorizedBy;
        CancelledAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new InvoiceCancelledEvent(Id, reason, cancelledBy));
    }

    public void RecordPayment(decimal amount)
    {
        if (Status != InvoiceStatus.Finalized)
            throw new BusinessException(ErrorCodes.InvoiceNotFinalized);
        if (amount <= 0) throw new BusinessException(ErrorCodes.InvalidPaymentAmount);

        PaidAmount = Math.Round(PaidAmount + amount, 4);
        PaymentStatus = PaidAmount >= GrandTotal - CreditedAmount
            ? InvoicePaymentStatus.Paid
            : InvoicePaymentStatus.PartiallyPaid;
    }

    public void RecordCredit(decimal amount)
    {
        if (amount <= 0) throw new BusinessException(ErrorCodes.InvalidInvoiceAmount);
        CreditedAmount = Math.Round(CreditedAmount + amount, 4);
        if (PaidAmount + CreditedAmount >= GrandTotal)
            PaymentStatus = InvoicePaymentStatus.Paid;
    }

    public void RecordPrint()
    {
        PrintCount++;
        LastPrintedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new InvoicePrintedEvent(Id, PrintCount));
    }

    private void EnsureModifiable()
    {
        if (Status != InvoiceStatus.Draft)
            throw new BusinessException(ErrorCodes.InvoiceNotEditable);
    }

    private void RecalculateTotals()
    {
        SubTotal = _lines.Sum(l => l.NetAmount + l.DiscountAmount);
        DiscountTotal = _lines.Sum(l => l.DiscountAmount);
        TaxTotal = _taxLines.Any() ? _taxLines.Sum(t => t.TaxAmount) : _lines.Sum(l => l.TaxAmount);
        GrandTotal = Math.Max(0, _lines.Sum(l => l.TotalAmount));
    }
}

public sealed class InvoiceLine : AuditableBaseEntity
{
    public Guid InvoiceId { get; private set; }
    public Guid? OrderItemId { get; private set; }
    public int LineNumber { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductNameAr { get; private set; }
    public string? ProductNameEn { get; private set; }
    public string? Sku { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal NetAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Currency { get; private set; }
    public Guid? TaxRateId { get; private set; }

    private InvoiceLine() { ProductNameAr = string.Empty; Currency = "SAR"; }

    internal InvoiceLine(
        Guid invoiceId, Guid? orderItemId, int lineNumber, Guid productId,
        string productNameAr, string? productNameEn, string? sku,
        decimal quantity, decimal unitPrice, decimal discountAmount, decimal taxAmount,
        decimal netAmount, decimal totalAmount, string currency, Guid? taxRateId)
    {
        InvoiceId = invoiceId;
        OrderItemId = orderItemId;
        LineNumber = lineNumber;
        ProductId = productId;
        ProductNameAr = productNameAr;
        ProductNameEn = productNameEn;
        Sku = sku;
        Quantity = quantity;
        UnitPrice = unitPrice;
        DiscountAmount = discountAmount;
        TaxAmount = taxAmount;
        NetAmount = netAmount;
        TotalAmount = totalAmount;
        Currency = currency.ToUpperInvariant();
        TaxRateId = taxRateId;
    }
}

public sealed class InvoiceTaxLine : AuditableBaseEntity
{
    public Guid InvoiceId { get; private set; }
    public string TaxNameAr { get; private set; }
    public string? TaxNameEn { get; private set; }
    public decimal TaxRate { get; private set; }
    public decimal TaxableAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public string Currency { get; private set; }
    public bool IsInclusive { get; private set; }

    private InvoiceTaxLine() { TaxNameAr = string.Empty; Currency = "SAR"; }

    internal InvoiceTaxLine(Guid invoiceId, string taxNameAr, string? taxNameEn,
        decimal rate, decimal taxableAmount, decimal taxAmount, string currency, bool isInclusive)
    {
        InvoiceId = invoiceId;
        TaxNameAr = taxNameAr;
        TaxNameEn = taxNameEn;
        TaxRate = rate;
        TaxableAmount = taxableAmount;
        TaxAmount = taxAmount;
        Currency = currency.ToUpperInvariant();
        IsInclusive = isInclusive;
    }
}
