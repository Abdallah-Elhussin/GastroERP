using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Sales.BackOffice;

/// <summary>
/// فاتورة مبيعات إدارية (Back Office) — مستقلة عن طلبات POS.
/// الدورة: مسودة → اعتماد → ترحيل → عكس / إلغاء.
/// </summary>
public sealed class BackOfficeSalesInvoice : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid? CompanyId { get; private set; }
    public Guid? BranchId { get; private set; }
    public string InvoiceNumber { get; private set; }
    public BackOfficeSalesInvoiceNature Nature { get; private set; }
    public BackOfficeSalesPaymentMode PaymentMode { get; private set; }
    public BackOfficeSalesDocumentStatus Status { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid? WarehouseId { get; private set; }
    public Guid? CostCenterId { get; private set; }
    public Guid? SalesPersonId { get; private set; }
    public Guid? BackOfficeSalesOrderId { get; private set; }
    public DateOnly InvoiceDate { get; private set; }
    public DateOnly? DueDate { get; private set; }
    public string Currency { get; private set; }
    public decimal ExchangeRate { get; private set; }
    public string? ExternalReference { get; private set; }
    public string? Notes { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal PaidAmount { get; private set; }
    public BackOfficeSalesPaymentStatus PaymentStatus { get; private set; }
    public Guid? JournalEntryId { get; private set; }
    public Guid? CogsJournalEntryId { get; private set; }
    public Guid? ReversalJournalEntryId { get; private set; }
    public DateTimeOffset? ApprovedAt { get; private set; }
    public Guid? ApprovedBy { get; private set; }
    public DateTimeOffset? PostedAt { get; private set; }
    public Guid? PostedBy { get; private set; }

    public decimal RemainingAmount => Math.Max(0, TotalAmount - PaidAmount);

    public bool AffectsInventory => Nature is BackOfficeSalesInvoiceNature.Inventory
        or BackOfficeSalesInvoiceNature.Mixed;

    private readonly List<BackOfficeSalesInvoiceLine> _lines = [];
    public IReadOnlyCollection<BackOfficeSalesInvoiceLine> Lines => _lines.AsReadOnly();

    private BackOfficeSalesInvoice()
    {
        InvoiceNumber = string.Empty;
        Currency = "SAR";
        ExchangeRate = 1m;
        Nature = BackOfficeSalesInvoiceNature.Inventory;
        PaymentMode = BackOfficeSalesPaymentMode.Credit;
        Status = BackOfficeSalesDocumentStatus.Draft;
        PaymentStatus = BackOfficeSalesPaymentStatus.Unpaid;
    }

    public static BackOfficeSalesInvoice CreateDraft(
        Guid tenantId,
        string invoiceNumber,
        Guid customerId,
        DateOnly invoiceDate,
        BackOfficeSalesPaymentMode paymentMode,
        BackOfficeSalesInvoiceNature nature = BackOfficeSalesInvoiceNature.Inventory,
        string currency = "SAR",
        Guid? companyId = null,
        Guid? branchId = null,
        Guid? warehouseId = null,
        Guid? costCenterId = null,
        Guid? salesPersonId = null,
        Guid? backOfficeSalesOrderId = null,
        DateOnly? dueDate = null,
        decimal exchangeRate = 1m,
        string? externalReference = null,
        string? notes = null,
        decimal discountAmount = 0)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException(nameof(tenantId));
        if (string.IsNullOrWhiteSpace(invoiceNumber)) throw new BusinessException(ErrorCodes.RequiredField);
        if (customerId == Guid.Empty) throw new BusinessException(ErrorCodes.RequiredField);
        if (exchangeRate <= 0) throw new BusinessException(ErrorCodes.InvalidAmount);
        if (nature is BackOfficeSalesInvoiceNature.Inventory && warehouseId is null)
            throw new BusinessException(ErrorCodes.RequiredField, "Warehouse is required for inventory sales.");

        return new BackOfficeSalesInvoice
        {
            TenantId = tenantId,
            CompanyId = companyId,
            BranchId = branchId,
            InvoiceNumber = invoiceNumber.Trim(),
            Nature = nature,
            PaymentMode = paymentMode,
            Status = BackOfficeSalesDocumentStatus.Draft,
            CustomerId = customerId,
            WarehouseId = warehouseId,
            CostCenterId = costCenterId,
            SalesPersonId = salesPersonId,
            BackOfficeSalesOrderId = backOfficeSalesOrderId,
            InvoiceDate = invoiceDate,
            DueDate = dueDate,
            Currency = currency.ToUpperInvariant(),
            ExchangeRate = exchangeRate,
            ExternalReference = string.IsNullOrWhiteSpace(externalReference) ? null : externalReference.Trim(),
            Notes = notes,
            DiscountAmount = Math.Max(0, discountAmount),
            PaymentStatus = BackOfficeSalesPaymentStatus.Unpaid
        };
    }

    public void AddLine(
        Guid? inventoryItemId,
        Guid? productId,
        string description,
        decimal quantity,
        decimal unitPrice,
        BackOfficeSalesLineNature lineNature = BackOfficeSalesLineNature.Inventory,
        Guid? unitId = null,
        Guid? lineWarehouseId = null,
        decimal discountPercent = 0,
        decimal discountAmount = 0,
        decimal taxPercent = 0,
        decimal taxAmount = 0,
        Guid? costCenterId = null,
        decimal? unitCost = null,
        Guid? salesOrderLineId = null)
    {
        EnsureDraft();
        if (quantity <= 0) throw new BusinessException(ErrorCodes.InvalidQuantity);
        if (unitPrice < 0) throw new BusinessException(ErrorCodes.InvalidAmount);
        if (lineNature == BackOfficeSalesLineNature.Inventory && inventoryItemId is null && productId is null)
            throw new BusinessException(ErrorCodes.RequiredField, "Item or product is required for inventory lines.");

        _lines.Add(new BackOfficeSalesInvoiceLine(
            Id, inventoryItemId, productId, description, quantity, unitPrice, lineNature,
            unitId, lineWarehouseId, discountPercent, discountAmount, taxPercent, taxAmount,
            costCenterId, unitCost, salesOrderLineId));
        RecalculateTotals();
    }

    /// <summary>يربط الفاتورة (المسودة) بأمر بيع إداري.</summary>
    public void LinkToSalesOrder(Guid salesOrderId)
    {
        EnsureDraft();
        if (salesOrderId == Guid.Empty)
            throw new BusinessException(ErrorCodes.RequiredField, "Sales order is required.");
        BackOfficeSalesOrderId = salesOrderId;
    }

    public void ClearLines()
    {
        EnsureDraft();
        _lines.Clear();
        RecalculateTotals();
    }

    public void UpdateHeader(
        DateOnly invoiceDate,
        BackOfficeSalesPaymentMode paymentMode,
        BackOfficeSalesInvoiceNature? nature = null,
        DateOnly? dueDate = null,
        Guid? warehouseId = null,
        Guid? costCenterId = null,
        Guid? salesPersonId = null,
        Guid? branchId = null,
        decimal? exchangeRate = null,
        string? externalReference = null,
        string? notes = null,
        decimal? discountAmount = null)
    {
        EnsureDraft();
        InvoiceDate = invoiceDate;
        PaymentMode = paymentMode;
        DueDate = dueDate;
        if (nature.HasValue) Nature = nature.Value;
        if (warehouseId.HasValue) WarehouseId = warehouseId;
        if (costCenterId.HasValue) CostCenterId = costCenterId;
        if (salesPersonId.HasValue) SalesPersonId = salesPersonId;
        if (branchId.HasValue) BranchId = branchId;
        if (exchangeRate.HasValue)
        {
            if (exchangeRate.Value <= 0) throw new BusinessException(ErrorCodes.InvalidAmount);
            ExchangeRate = exchangeRate.Value;
        }
        if (externalReference is not null)
            ExternalReference = string.IsNullOrWhiteSpace(externalReference) ? null : externalReference.Trim();
        if (notes is not null) Notes = notes;
        if (discountAmount.HasValue) DiscountAmount = Math.Max(0, discountAmount.Value);

        if (Nature is BackOfficeSalesInvoiceNature.Inventory && WarehouseId is null)
            throw new BusinessException(ErrorCodes.RequiredField, "Warehouse is required for inventory sales.");

        RecalculateTotals();
    }

    public void Approve(Guid approvedBy)
    {
        if (Status != BackOfficeSalesDocumentStatus.Draft)
            throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition);
        if (_lines.Count == 0)
            throw new BusinessException(ErrorCodes.RequiredField, "Invoice must have lines.");
        if (CustomerId == Guid.Empty)
            throw new BusinessException(ErrorCodes.RequiredField);
        if (AffectsInventory && WarehouseId is null
            && _lines.Any(l => l.LineNature == BackOfficeSalesLineNature.Inventory && l.LineWarehouseId is null))
            throw new BusinessException(ErrorCodes.RequiredField, "Warehouse is required.");

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

    public void MarkPosted(Guid journalEntryId, Guid postedBy, Guid? cogsJournalEntryId = null)
    {
        if (Status != BackOfficeSalesDocumentStatus.Approved)
            throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition, "Invoice must be approved before posting.");
        if (_lines.Count == 0)
            throw new BusinessException(ErrorCodes.RequiredField);

        Status = BackOfficeSalesDocumentStatus.Posted;
        JournalEntryId = journalEntryId;
        CogsJournalEntryId = cogsJournalEntryId;
        PostedAt = DateTimeOffset.UtcNow;
        PostedBy = postedBy;
        RefreshPaymentStatus();
    }

    public void MarkReversed(Guid? reversalJournalId = null)
    {
        if (Status != BackOfficeSalesDocumentStatus.Posted)
            throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition);
        Status = BackOfficeSalesDocumentStatus.Reversed;
        ReversalJournalEntryId = reversalJournalId;
    }

    public void Cancel()
    {
        if (Status is BackOfficeSalesDocumentStatus.Posted or BackOfficeSalesDocumentStatus.Reversed)
            throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition);
        Status = BackOfficeSalesDocumentStatus.Cancelled;
    }

    public void ApplyPayment(decimal amount)
    {
        if (Status != BackOfficeSalesDocumentStatus.Posted)
            throw new BusinessException(ErrorCodes.SalesInvalidStatusTransition);
        if (amount <= 0) throw new BusinessException(ErrorCodes.InvalidAmount);
        if (PaidAmount + amount > TotalAmount + 0.0001m)
            throw new BusinessException(ErrorCodes.InvalidAmount, "Payment exceeds remaining invoice amount.");
        PaidAmount += amount;
        RefreshPaymentStatus();
    }

    private void RefreshPaymentStatus()
    {
        if (PaidAmount <= 0)
            PaymentStatus = BackOfficeSalesPaymentStatus.Unpaid;
        else if (PaidAmount + 0.0001m >= TotalAmount)
            PaymentStatus = BackOfficeSalesPaymentStatus.FullyPaid;
        else
            PaymentStatus = BackOfficeSalesPaymentStatus.PartiallyPaid;
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

public sealed class BackOfficeSalesInvoiceLine : AuditableBaseEntity
{
    public Guid BackOfficeSalesInvoiceId { get; private set; }
    public Guid? InventoryItemId { get; private set; }
    public Guid? ProductId { get; private set; }
    public Guid? UnitId { get; private set; }
    public Guid? LineWarehouseId { get; private set; }
    public Guid? CostCenterId { get; private set; }
    public Guid? SalesOrderLineId { get; private set; }
    public BackOfficeSalesLineNature LineNature { get; private set; }
    public string Description { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal UnitCost { get; private set; }
    public decimal DiscountPercent { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TaxPercent { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal ReturnedQuantity { get; private set; }

    public decimal LineGross => Quantity * UnitPrice;
    public decimal LineNet => Math.Max(0, LineGross - DiscountAmount);
    public decimal LineTotal => LineNet + TaxAmount;
    public decimal RemainingToReturn => Math.Max(0, Quantity - ReturnedQuantity);
    public bool AffectsInventory => LineNature == BackOfficeSalesLineNature.Inventory;

    private BackOfficeSalesInvoiceLine()
    {
        Description = string.Empty;
    }

    internal BackOfficeSalesInvoiceLine(
        Guid invoiceId,
        Guid? inventoryItemId,
        Guid? productId,
        string description,
        decimal quantity,
        decimal unitPrice,
        BackOfficeSalesLineNature lineNature,
        Guid? unitId,
        Guid? lineWarehouseId,
        decimal discountPercent,
        decimal discountAmount,
        decimal taxPercent,
        decimal taxAmount,
        Guid? costCenterId,
        decimal? unitCost,
        Guid? salesOrderLineId = null)
    {
        BackOfficeSalesInvoiceId = invoiceId;
        InventoryItemId = inventoryItemId;
        ProductId = productId;
        Description = string.IsNullOrWhiteSpace(description) ? "—" : description.Trim();
        Quantity = quantity;
        UnitPrice = unitPrice;
        UnitCost = Math.Max(0, unitCost ?? 0);
        LineNature = lineNature;
        UnitId = unitId;
        LineWarehouseId = lineWarehouseId;
        CostCenterId = costCenterId;
        SalesOrderLineId = salesOrderLineId;
        DiscountPercent = Math.Max(0, discountPercent);
        DiscountAmount = discountAmount > 0
            ? Math.Max(0, discountAmount)
            : Math.Round(Quantity * UnitPrice * DiscountPercent / 100m, 4);
        TaxPercent = Math.Max(0, taxPercent);
        TaxAmount = taxAmount > 0
            ? Math.Max(0, taxAmount)
            : Math.Round(Math.Max(0, Quantity * UnitPrice - DiscountAmount) * TaxPercent / 100m, 4);
        ReturnedQuantity = 0;
    }

    public void AddReturnedQuantity(decimal qty)
    {
        if (qty == 0) return;
        if (ReturnedQuantity + qty > Quantity + 0.0001m)
            throw new BusinessException(ErrorCodes.InvalidQuantity, "Cannot return more than invoiced quantity.");
        ReturnedQuantity = Math.Max(0, ReturnedQuantity + qty);
    }
}
