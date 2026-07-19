using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Sales.BackOffice;

/// <summary>
/// عرض سعر مبيعات إداري (Back Office) — يسبق أمر البيع.
/// الدورة: مسودة → اعتماد → تحويل لأمر بيع (يُعامل كترحيل) / إلغاء.
/// </summary>
public sealed class BackOfficeSalesQuotation : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid? CompanyId { get; private set; }
    public Guid? BranchId { get; private set; }
    public string QuotationNumber { get; private set; }
    public BackOfficeSalesDocumentStatus Status { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid? WarehouseId { get; private set; }
    public Guid? SalesPersonId { get; private set; }
    public DateOnly QuotationDate { get; private set; }
    public DateOnly? ValidUntil { get; private set; }
    public string Currency { get; private set; }
    public decimal ExchangeRate { get; private set; }
    public string? Notes { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public Guid? ConvertedOrderId { get; private set; }
    public DateTimeOffset? ApprovedAt { get; private set; }
    public Guid? ApprovedBy { get; private set; }

    public bool IsExpired => ValidUntil.HasValue && ValidUntil.Value < DateOnly.FromDateTime(DateTimeOffset.UtcNow.Date);

    private readonly List<BackOfficeSalesQuotationLine> _lines = [];
    public IReadOnlyCollection<BackOfficeSalesQuotationLine> Lines => _lines.AsReadOnly();

    private BackOfficeSalesQuotation()
    {
        QuotationNumber = string.Empty;
        Currency = "SAR";
        ExchangeRate = 1m;
        Status = BackOfficeSalesDocumentStatus.Draft;
    }

    public static BackOfficeSalesQuotation CreateDraft(
        Guid tenantId,
        string quotationNumber,
        Guid customerId,
        DateOnly quotationDate,
        string currency = "SAR",
        Guid? companyId = null,
        Guid? branchId = null,
        Guid? warehouseId = null,
        Guid? salesPersonId = null,
        DateOnly? validUntil = null,
        decimal exchangeRate = 1m,
        string? notes = null,
        decimal discountAmount = 0)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException(nameof(tenantId));
        if (string.IsNullOrWhiteSpace(quotationNumber)) throw new BusinessException(ErrorCodes.RequiredField);
        if (customerId == Guid.Empty) throw new BusinessException(ErrorCodes.RequiredField);
        if (exchangeRate <= 0) throw new BusinessException(ErrorCodes.InvalidAmount);
        if (validUntil.HasValue && validUntil.Value < quotationDate)
            throw new BusinessException(ErrorCodes.RequiredField, "Valid until date cannot precede the quotation date.");

        return new BackOfficeSalesQuotation
        {
            TenantId = tenantId,
            CompanyId = companyId,
            BranchId = branchId,
            QuotationNumber = quotationNumber.Trim(),
            Status = BackOfficeSalesDocumentStatus.Draft,
            CustomerId = customerId,
            WarehouseId = warehouseId,
            SalesPersonId = salesPersonId,
            QuotationDate = quotationDate,
            ValidUntil = validUntil,
            Currency = currency.ToUpperInvariant(),
            ExchangeRate = exchangeRate,
            Notes = notes,
            DiscountAmount = Math.Max(0, discountAmount)
        };
    }

    public void AddLine(
        string description,
        decimal quantity,
        decimal unitPrice,
        Guid? inventoryItemId = null,
        Guid? unitId = null,
        BackOfficeSalesLineNature lineNature = BackOfficeSalesLineNature.Inventory,
        decimal discountAmount = 0,
        decimal taxPercent = 0,
        decimal taxAmount = 0,
        decimal unitCost = 0)
    {
        EnsureDraft();
        if (quantity <= 0) throw new BusinessException(ErrorCodes.InvalidQuantity);
        if (unitPrice < 0) throw new BusinessException(ErrorCodes.InvalidAmount);

        _lines.Add(new BackOfficeSalesQuotationLine(
            Id, inventoryItemId, unitId, lineNature, description, quantity, unitPrice,
            discountAmount, taxPercent, taxAmount, unitCost));
        RecalculateTotals();
    }

    public void ClearLines()
    {
        EnsureDraft();
        _lines.Clear();
        RecalculateTotals();
    }

    public void UpdateHeader(
        DateOnly quotationDate,
        Guid? warehouseId = null,
        Guid? salesPersonId = null,
        Guid? branchId = null,
        DateOnly? validUntil = null,
        string? notes = null,
        decimal? discountAmount = null)
    {
        EnsureDraft();
        if (validUntil.HasValue && validUntil.Value < quotationDate)
            throw new BusinessException(ErrorCodes.RequiredField, "Valid until date cannot precede the quotation date.");

        QuotationDate = quotationDate;
        ValidUntil = validUntil;
        if (warehouseId.HasValue) WarehouseId = warehouseId;
        if (salesPersonId.HasValue) SalesPersonId = salesPersonId;
        if (branchId.HasValue) BranchId = branchId;
        if (notes is not null) Notes = notes;
        if (discountAmount.HasValue) DiscountAmount = Math.Max(0, discountAmount.Value);

        RecalculateTotals();
    }

    public void Approve(Guid approvedBy)
    {
        if (Status != BackOfficeSalesDocumentStatus.Draft)
            throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition);
        if (_lines.Count == 0)
            throw new BusinessException(ErrorCodes.RequiredField, "Quotation must have lines.");

        Status = BackOfficeSalesDocumentStatus.Approved;
        ApprovedAt = DateTimeOffset.UtcNow;
        ApprovedBy = approvedBy;
    }

    public void Unapprove()
    {
        if (Status != BackOfficeSalesDocumentStatus.Approved)
            throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition);
        Status = BackOfficeSalesDocumentStatus.Draft;
        ApprovedAt = null;
        ApprovedBy = null;
    }

    public void Cancel()
    {
        if (Status is BackOfficeSalesDocumentStatus.Posted or BackOfficeSalesDocumentStatus.Reversed)
            throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition);
        Status = BackOfficeSalesDocumentStatus.Cancelled;
    }

    /// <summary>يُعامل كترحيل العرض — لا يوجد قيد محاسبي مباشر للعرض نفسه.</summary>
    public void MarkConverted(Guid orderId)
    {
        if (Status != BackOfficeSalesDocumentStatus.Approved)
            throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition, "Quotation must be approved before conversion.");
        if (orderId == Guid.Empty) throw new BusinessException(ErrorCodes.RequiredField, "Order is required.");

        Status = BackOfficeSalesDocumentStatus.Posted;
        ConvertedOrderId = orderId;
    }

    private void RecalculateTotals()
    {
        SubTotal = _lines.Sum(l => l.LineNet);
        TaxAmount = _lines.Sum(l => l.TaxAmount);
        TotalAmount = Math.Max(0, SubTotal - DiscountAmount + TaxAmount);
    }

    private void EnsureDraft()
    {
        if (Status != BackOfficeSalesDocumentStatus.Draft)
            throw new BusinessException(ErrorCodes.CannotModifyApprovedDocument);
    }
}

public sealed class BackOfficeSalesQuotationLine : AuditableBaseEntity
{
    public Guid QuotationId { get; private set; }
    public Guid? InventoryItemId { get; private set; }
    public Guid? UnitId { get; private set; }
    public BackOfficeSalesLineNature LineNature { get; private set; }
    public string Description { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TaxPercent { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal UnitCost { get; private set; }

    public decimal LineGross => Quantity * UnitPrice;
    public decimal LineNet => Math.Max(0, LineGross - DiscountAmount);
    public decimal LineTotal => LineNet + TaxAmount;

    private BackOfficeSalesQuotationLine() => Description = string.Empty;

    internal BackOfficeSalesQuotationLine(
        Guid quotationId,
        Guid? inventoryItemId,
        Guid? unitId,
        BackOfficeSalesLineNature lineNature,
        string description,
        decimal quantity,
        decimal unitPrice,
        decimal discountAmount,
        decimal taxPercent,
        decimal taxAmount,
        decimal unitCost)
    {
        QuotationId = quotationId;
        InventoryItemId = inventoryItemId;
        UnitId = unitId;
        LineNature = lineNature;
        Description = string.IsNullOrWhiteSpace(description) ? "—" : description.Trim();
        Quantity = quantity;
        UnitPrice = unitPrice;
        UnitCost = Math.Max(0, unitCost);
        DiscountAmount = Math.Max(0, discountAmount);
        TaxPercent = Math.Max(0, taxPercent);
        TaxAmount = taxAmount > 0
            ? Math.Max(0, taxAmount)
            : Math.Round(Math.Max(0, Quantity * UnitPrice - DiscountAmount) * TaxPercent / 100m, 4);
    }
}
