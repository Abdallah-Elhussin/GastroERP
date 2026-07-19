using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Inventory.Purchasing;

/// <summary>
/// فاتورة مشتريات (AP Invoice) — من استلام أو مباشرة.
/// FromReceipt: يصفّي GRNI دون إعادة إدخال المخزون.
/// Direct: شراء كامل في مستند واحد (بضاعة / خدمات / أصول).
/// </summary>
public sealed class PurchaseInvoice : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid? CompanyId { get; private set; }
    public Guid? BranchId { get; private set; }
    public string InvoiceNumber { get; private set; }
    public PurchaseInvoiceKind Kind { get; private set; }
    public PurchaseInvoicePaymentMode PaymentMode { get; private set; }
    public DirectPurchaseNature Nature { get; private set; }
    public PurchasingDocumentStatus Status { get; private set; }
    public Guid SupplierId { get; private set; }
    public Guid? PurchaseOrderId { get; private set; }
    public Guid? GoodsReceiptId { get; private set; }
    public Guid? WarehouseId { get; private set; }
    public Guid? CostCenterId { get; private set; }
    public DateOnly InvoiceDate { get; private set; }
    public DateOnly? DueDate { get; private set; }
    public string Currency { get; private set; }
    public decimal ExchangeRate { get; private set; }
    public string? SupplierInvoiceNumber { get; private set; }
    public string? ExternalReference { get; private set; }
    public string? Notes { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal PaidAmount { get; private set; }
    public PurchaseInvoicePaymentStatus PaymentStatus { get; private set; }
    public Guid? JournalEntryId { get; private set; }
    public Guid? ReversalJournalEntryId { get; private set; }
    public DateTimeOffset? PostedAt { get; private set; }
    public Guid? PostedBy { get; private set; }

    public decimal RemainingAmount => Math.Max(0, TotalAmount - PaidAmount);
    public bool AffectsInventory => Kind == PurchaseInvoiceKind.Direct
        ? Nature == DirectPurchaseNature.Inventory
        : false;

    private readonly List<PurchaseInvoiceLine> _lines = [];
    public IReadOnlyCollection<PurchaseInvoiceLine> Lines => _lines.AsReadOnly();

    private PurchaseInvoice()
    {
        InvoiceNumber = string.Empty;
        Currency = "SAR";
        ExchangeRate = 1m;
        Nature = DirectPurchaseNature.Inventory;
    }

    public static PurchaseInvoice CreateDraft(
        Guid tenantId,
        string invoiceNumber,
        PurchaseInvoiceKind kind,
        PurchaseInvoicePaymentMode paymentMode,
        Guid supplierId,
        DateOnly invoiceDate,
        string currency = "SAR",
        Guid? companyId = null,
        Guid? branchId = null,
        Guid? purchaseOrderId = null,
        Guid? goodsReceiptId = null,
        Guid? warehouseId = null,
        DateOnly? dueDate = null,
        string? supplierInvoiceNumber = null,
        string? notes = null,
        DirectPurchaseNature nature = DirectPurchaseNature.Inventory,
        decimal exchangeRate = 1m,
        string? externalReference = null,
        Guid? costCenterId = null,
        decimal discountAmount = 0)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException(nameof(tenantId));
        if (string.IsNullOrWhiteSpace(invoiceNumber)) throw new BusinessException(ErrorCodes.RequiredField);
        if (supplierId == Guid.Empty) throw new BusinessException(ErrorCodes.RequiredField);
        if (kind == PurchaseInvoiceKind.FromReceipt && goodsReceiptId is null)
            throw new BusinessException(ErrorCodes.RequiredField, "Goods receipt is required for receipt-based invoices.");
        if (kind == PurchaseInvoiceKind.Direct
            && nature == DirectPurchaseNature.Inventory
            && warehouseId is null)
            throw new BusinessException(ErrorCodes.RequiredField, "Warehouse is required for direct inventory purchases.");
        if (exchangeRate <= 0) throw new BusinessException(ErrorCodes.InvalidAmount);

        return new PurchaseInvoice
        {
            TenantId = tenantId,
            CompanyId = companyId,
            BranchId = branchId,
            InvoiceNumber = invoiceNumber.Trim(),
            Kind = kind,
            PaymentMode = paymentMode,
            Nature = kind == PurchaseInvoiceKind.Direct ? nature : DirectPurchaseNature.Inventory,
            Status = PurchasingDocumentStatus.Draft,
            SupplierId = supplierId,
            PurchaseOrderId = purchaseOrderId,
            GoodsReceiptId = goodsReceiptId,
            WarehouseId = warehouseId,
            CostCenterId = costCenterId,
            InvoiceDate = invoiceDate,
            DueDate = dueDate,
            Currency = currency.ToUpperInvariant(),
            ExchangeRate = exchangeRate,
            SupplierInvoiceNumber = supplierInvoiceNumber,
            ExternalReference = string.IsNullOrWhiteSpace(externalReference) ? null : externalReference.Trim(),
            Notes = notes,
            DiscountAmount = Math.Max(0, discountAmount),
            PaymentStatus = PurchaseInvoicePaymentStatus.Unpaid
        };
    }

    public void AddLine(
        Guid inventoryItemId,
        Guid unitId,
        decimal quantity,
        decimal unitPrice,
        decimal taxAmount = 0,
        Guid? goodsReceiptLineId = null,
        Guid? purchaseOrderLineId = null,
        string? description = null,
        decimal discountPercent = 0,
        decimal discountAmount = 0,
        decimal taxPercent = 0,
        string? batchNumber = null,
        string? serialNumber = null,
        DateTimeOffset? productionDate = null,
        DateTimeOffset? expiryDate = null,
        Guid? lineWarehouseId = null,
        Guid? costCenterId = null)
    {
        EnsureDraft();
        if (quantity <= 0) throw new BusinessException(ErrorCodes.InvalidQuantity);
        if (unitPrice <= 0) throw new BusinessException(ErrorCodes.InvalidAmount, "Unit price must be greater than zero.");
        if (Kind == PurchaseInvoiceKind.FromReceipt && goodsReceiptLineId is null)
            throw new BusinessException(ErrorCodes.RequiredField, "Receipt line is required.");

        _lines.Add(new PurchaseInvoiceLine(
            Id, inventoryItemId, unitId, quantity, unitPrice, taxAmount,
            goodsReceiptLineId, purchaseOrderLineId, description,
            discountPercent, discountAmount, taxPercent,
            batchNumber, serialNumber, productionDate, expiryDate,
            lineWarehouseId, costCenterId));
        RecalculateTotals();
    }

    public void ClearLines()
    {
        EnsureDraft();
        _lines.Clear();
        RecalculateTotals();
    }

    public void UpdateHeader(
        DateOnly invoiceDate,
        PurchaseInvoicePaymentMode paymentMode,
        DateOnly? dueDate,
        string? supplierInvoiceNumber,
        string? notes,
        Guid? warehouseId = null,
        DirectPurchaseNature? nature = null,
        decimal? exchangeRate = null,
        string? externalReference = null,
        Guid? costCenterId = null,
        decimal? discountAmount = null,
        Guid? branchId = null)
    {
        EnsureDraft();
        InvoiceDate = invoiceDate;
        PaymentMode = paymentMode;
        DueDate = dueDate;
        SupplierInvoiceNumber = supplierInvoiceNumber;
        Notes = notes;
        if (warehouseId.HasValue) WarehouseId = warehouseId;
        if (nature.HasValue && Kind == PurchaseInvoiceKind.Direct) Nature = nature.Value;
        if (exchangeRate.HasValue)
        {
            if (exchangeRate.Value <= 0) throw new BusinessException(ErrorCodes.InvalidAmount);
            ExchangeRate = exchangeRate.Value;
        }
        if (externalReference is not null)
            ExternalReference = string.IsNullOrWhiteSpace(externalReference) ? null : externalReference.Trim();
        if (costCenterId.HasValue) CostCenterId = costCenterId;
        if (discountAmount.HasValue) DiscountAmount = Math.Max(0, discountAmount.Value);
        if (branchId.HasValue) BranchId = branchId;
        RecalculateTotals();
    }

    public void Approve()
    {
        if (Status != PurchasingDocumentStatus.Draft)
            throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        if (_lines.Count == 0)
            throw new BusinessException(ErrorCodes.RequiredField);
        if (SupplierId == Guid.Empty)
            throw new BusinessException(ErrorCodes.RequiredField);
        if (AffectsInventory && WarehouseId is null)
            throw new BusinessException(ErrorCodes.RequiredField, "Warehouse is required.");
        Status = PurchasingDocumentStatus.Approved;
    }

    public void MarkPosted(Guid journalEntryId, Guid postedBy)
    {
        // Spec: posting requires approval for direct invoices.
        if (Kind == PurchaseInvoiceKind.Direct)
        {
            if (Status != PurchasingDocumentStatus.Approved)
                throw new BusinessException(ErrorCodes.InvalidStatusTransition, "Direct invoice must be approved before posting.");
        }
        else if (Status is not (PurchasingDocumentStatus.Draft or PurchasingDocumentStatus.Approved))
        {
            throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        }

        if (_lines.Count == 0)
            throw new BusinessException(ErrorCodes.RequiredField);

        Status = PurchasingDocumentStatus.Posted;
        JournalEntryId = journalEntryId;
        PostedAt = DateTimeOffset.UtcNow;
        PostedBy = postedBy;
        RefreshPaymentStatus();
    }

    public void MarkReversed(Guid? reversalJournalId = null)
    {
        if (Status != PurchasingDocumentStatus.Posted)
            throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        Status = PurchasingDocumentStatus.Reversed;
        ReversalJournalEntryId = reversalJournalId;
    }

    public void Cancel()
    {
        if (Status is PurchasingDocumentStatus.Posted or PurchasingDocumentStatus.Reversed)
            throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        Status = PurchasingDocumentStatus.Cancelled;
    }

    public void ApplyPayment(decimal amount)
    {
        if (Status != PurchasingDocumentStatus.Posted)
            throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        if (amount <= 0)
            throw new BusinessException(ErrorCodes.InvalidAmount);
        if (PaidAmount + amount > TotalAmount + 0.0001m)
            throw new BusinessException(ErrorCodes.InvalidAmount, "Payment exceeds remaining invoice amount.");

        PaidAmount += amount;
        RefreshPaymentStatus();
    }

    private void RefreshPaymentStatus()
    {
        if (_lines.Count > 0 && _lines.All(l => l.ReturnedQuantity >= l.Quantity))
        {
            PaymentStatus = PurchaseInvoicePaymentStatus.FullyReturned;
            return;
        }

        if (PaidAmount <= 0)
            PaymentStatus = PurchaseInvoicePaymentStatus.Unpaid;
        else if (PaidAmount + 0.0001m >= TotalAmount)
            PaymentStatus = PurchaseInvoicePaymentStatus.FullyPaid;
        else
            PaymentStatus = PurchaseInvoicePaymentStatus.PartiallyPaid;
    }

    public void RefreshReturnSettlement() => RefreshPaymentStatus();

    private void RecalculateTotals()
    {
        SubTotal = _lines.Sum(l => l.LineNet);
        var lineTax = _lines.Sum(l => l.TaxAmount);
        TaxAmount = lineTax;
        TotalAmount = Math.Max(0, SubTotal - DiscountAmount + TaxAmount);
    }

    private void EnsureDraft()
    {
        if (Status != PurchasingDocumentStatus.Draft)
            throw new BusinessException(ErrorCodes.CannotModifyApprovedDocument);
    }
}

public sealed class PurchaseInvoiceLine : AuditableBaseEntity
{
    public Guid PurchaseInvoiceId { get; private set; }
    public Guid InventoryItemId { get; private set; }
    public Guid UnitId { get; private set; }
    public Guid? GoodsReceiptLineId { get; private set; }
    public Guid? PurchaseOrderLineId { get; private set; }
    public Guid? LineWarehouseId { get; private set; }
    public Guid? CostCenterId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal DiscountPercent { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TaxPercent { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal ReturnedQuantity { get; private set; }
    public string? BatchNumber { get; private set; }
    public string? SerialNumber { get; private set; }
    public DateTimeOffset? ProductionDate { get; private set; }
    public DateTimeOffset? ExpiryDate { get; private set; }
    public string? Description { get; private set; }

    public decimal LineGross => Quantity * UnitPrice;
    public decimal LineNet => Math.Max(0, LineGross - DiscountAmount);
    public decimal LineTotal => LineNet + TaxAmount;
    public decimal RemainingToReturn => Math.Max(0, Quantity - ReturnedQuantity);

    private PurchaseInvoiceLine() { }

    internal PurchaseInvoiceLine(
        Guid purchaseInvoiceId,
        Guid inventoryItemId,
        Guid unitId,
        decimal quantity,
        decimal unitPrice,
        decimal taxAmount,
        Guid? goodsReceiptLineId,
        Guid? purchaseOrderLineId,
        string? description,
        decimal discountPercent = 0,
        decimal discountAmount = 0,
        decimal taxPercent = 0,
        string? batchNumber = null,
        string? serialNumber = null,
        DateTimeOffset? productionDate = null,
        DateTimeOffset? expiryDate = null,
        Guid? lineWarehouseId = null,
        Guid? costCenterId = null)
    {
        PurchaseInvoiceId = purchaseInvoiceId;
        InventoryItemId = inventoryItemId;
        UnitId = unitId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        DiscountPercent = Math.Max(0, discountPercent);
        DiscountAmount = discountAmount > 0
            ? Math.Max(0, discountAmount)
            : Math.Round(Quantity * UnitPrice * DiscountPercent / 100m, 4);
        TaxPercent = Math.Max(0, taxPercent);
        TaxAmount = taxAmount > 0
            ? Math.Max(0, taxAmount)
            : Math.Round(Math.Max(0, Quantity * UnitPrice - DiscountAmount) * TaxPercent / 100m, 4);
        GoodsReceiptLineId = goodsReceiptLineId;
        PurchaseOrderLineId = purchaseOrderLineId;
        Description = description;
        BatchNumber = string.IsNullOrWhiteSpace(batchNumber) ? null : batchNumber.Trim();
        SerialNumber = string.IsNullOrWhiteSpace(serialNumber) ? null : serialNumber.Trim();
        ProductionDate = productionDate;
        ExpiryDate = expiryDate;
        LineWarehouseId = lineWarehouseId;
        CostCenterId = costCenterId;
        ReturnedQuantity = 0;
    }

    public void AddReturnedQuantity(decimal qty)
    {
        if (qty == 0) return;
        if (ReturnedQuantity + qty < -0.0001m)
            throw new BusinessException(ErrorCodes.InvalidQuantity);
        if (ReturnedQuantity + qty > Quantity + 0.0001m)
            throw new BusinessException(ErrorCodes.InvalidQuantity, "Cannot return more than invoiced quantity.");
        ReturnedQuantity = Math.Max(0, ReturnedQuantity + qty);
    }
}
