using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Inventory.Purchasing;

/// <summary>
/// أمر الشراء — التزام تشغيلي فقط (لا مخزون ولا قيود محاسبية).
/// </summary>
public sealed class PurchaseOrder : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid SupplierId { get; private set; }
    public Guid DestinationWarehouseId { get; private set; }
    public Guid? BranchId { get; private set; }
    public Guid? CostCenterId { get; private set; }
    public Guid? ResponsibleEmployeeId { get; private set; }

    public string PoNumber { get; private set; }
    public byte OrderType { get; private set; } = 1;
    public DateTimeOffset OrderDate { get; private set; }
    public DateTimeOffset? ExpectedDeliveryDate { get; private set; }
    public PurchaseOrderStatus Status { get; private set; }

    public string Currency { get; private set; }
    public decimal ExchangeRate { get; private set; } = 1m;
    public string? PaymentMethod { get; private set; }
    public string? PaymentTerms { get; private set; }
    public string? ExternalReference { get; private set; }
    public string? Notes { get; private set; }

    public decimal TotalAmount { get; private set; }
    public DateTimeOffset? LastReceiptDate { get; private set; }

    public decimal CompletionPercent
    {
        get
        {
            var ordered = _lines.Sum(l => l.Quantity);
            if (ordered <= 0) return 0;
            var received = _lines.Sum(l => Math.Min(l.ReceivedQuantity, l.Quantity));
            return Math.Round(received / ordered * 100m, 2);
        }
    }

    public decimal RemainingQuantity =>
        _lines.Sum(l => Math.Max(0, l.Quantity - l.ReceivedQuantity));

    public bool HasAnyReceipt => _lines.Any(l => l.ReceivedQuantity > 0);

    private readonly List<PurchaseOrderLine> _lines = [];
    public IReadOnlyCollection<PurchaseOrderLine> Lines => _lines.AsReadOnly();

    private PurchaseOrder()
    {
        PoNumber = string.Empty;
        Currency = "SAR";
    }

    public PurchaseOrder(
        Guid tenantId,
        Guid supplierId,
        Guid destinationWarehouseId,
        string poNumber,
        DateTimeOffset? orderDate = null,
        DateTimeOffset? expectedDeliveryDate = null,
        string currency = "SAR",
        decimal exchangeRate = 1m,
        byte orderType = 1,
        Guid? branchId = null,
        Guid? costCenterId = null,
        Guid? responsibleEmployeeId = null,
        string? paymentMethod = null,
        string? paymentTerms = null,
        string? externalReference = null,
        string? notes = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException(nameof(tenantId));
        if (supplierId == Guid.Empty) throw new ArgumentException(nameof(supplierId));
        if (destinationWarehouseId == Guid.Empty) throw new ArgumentException(nameof(destinationWarehouseId));
        if (string.IsNullOrWhiteSpace(poNumber)) throw new ArgumentException(nameof(poNumber));
        if (exchangeRate <= 0) throw new ArgumentException(nameof(exchangeRate));

        TenantId = tenantId;
        SupplierId = supplierId;
        DestinationWarehouseId = destinationWarehouseId;
        BranchId = branchId;
        CostCenterId = costCenterId;
        ResponsibleEmployeeId = responsibleEmployeeId;
        PoNumber = poNumber.Trim();
        OrderType = orderType == 0 ? (byte)1 : orderType;
        OrderDate = orderDate ?? DateTimeOffset.UtcNow;
        ExpectedDeliveryDate = expectedDeliveryDate;
        Currency = currency.Trim().ToUpperInvariant();
        ExchangeRate = exchangeRate;
        PaymentMethod = paymentMethod;
        PaymentTerms = paymentTerms;
        ExternalReference = externalReference;
        Notes = notes;
        Status = PurchaseOrderStatus.Draft;
        TotalAmount = 0;
    }

    public void UpdateHeader(
        Guid supplierId,
        Guid destinationWarehouseId,
        DateTimeOffset orderDate,
        DateTimeOffset? expectedDeliveryDate,
        string currency,
        decimal exchangeRate,
        byte orderType,
        Guid? branchId,
        Guid? costCenterId,
        Guid? responsibleEmployeeId,
        string? paymentMethod,
        string? paymentTerms,
        string? externalReference,
        string? notes)
    {
        EnsureDraft();
        if (supplierId == Guid.Empty) throw new ArgumentException(nameof(supplierId));
        if (destinationWarehouseId == Guid.Empty) throw new ArgumentException(nameof(destinationWarehouseId));
        if (exchangeRate <= 0) throw new ArgumentException(nameof(exchangeRate));

        SupplierId = supplierId;
        DestinationWarehouseId = destinationWarehouseId;
        OrderDate = orderDate;
        ExpectedDeliveryDate = expectedDeliveryDate;
        Currency = currency.Trim().ToUpperInvariant();
        ExchangeRate = exchangeRate;
        OrderType = orderType == 0 ? (byte)1 : orderType;
        BranchId = branchId;
        CostCenterId = costCenterId;
        ResponsibleEmployeeId = responsibleEmployeeId;
        PaymentMethod = paymentMethod;
        PaymentTerms = paymentTerms;
        ExternalReference = externalReference;
        Notes = notes;
    }

    public void AddLine(
        Guid inventoryItemId,
        Guid unitId,
        decimal quantity,
        decimal unitPrice,
        decimal discountAmount = 0,
        decimal taxAmount = 0,
        string? description = null,
        Guid? warehouseId = null,
        string? lineNotes = null)
    {
        EnsureDraft();
        _lines.Add(new PurchaseOrderLine(
            TenantId, Id, inventoryItemId, unitId, quantity, unitPrice,
            discountAmount, taxAmount, description, warehouseId, lineNotes));
        RecalculateTotal();
    }

    public void ReplaceLines(IEnumerable<(
        Guid InventoryItemId,
        Guid UnitId,
        decimal Quantity,
        decimal UnitPrice,
        decimal DiscountAmount,
        decimal TaxAmount,
        string? Description,
        Guid? WarehouseId,
        string? LineNotes)> lines)
    {
        EnsureDraft();
        _lines.Clear();
        foreach (var l in lines)
        {
            _lines.Add(new PurchaseOrderLine(
                TenantId, Id, l.InventoryItemId, l.UnitId, l.Quantity, l.UnitPrice,
                l.DiscountAmount, l.TaxAmount, l.Description, l.WarehouseId, l.LineNotes));
        }
        RecalculateTotal();
    }

    public void RemoveLine(Guid lineId)
    {
        EnsureDraft();
        var line = _lines.FirstOrDefault(l => l.Id == lineId)
            ?? throw new BusinessException(ErrorCodes.ItemNotFound);
        _lines.Remove(line);
        RecalculateTotal();
    }

    public void Approve()
    {
        if (Status is not (PurchaseOrderStatus.Draft or PurchaseOrderStatus.PendingApproval))
            throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        if (SupplierId == Guid.Empty)
            throw new BusinessException(ErrorCodes.RequiredField);
        if (_lines.Count == 0)
            throw new BusinessException(ErrorCodes.RequiredField);

        Status = PurchaseOrderStatus.Approved;
    }

    public void SubmitForApproval()
    {
        if (Status != PurchaseOrderStatus.Draft)
            throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        if (_lines.Count == 0)
            throw new BusinessException(ErrorCodes.RequiredField);
        Status = PurchaseOrderStatus.PendingApproval;
        RaiseDomainEvent(new Domain.Events.Inventory.PurchaseOrderSubmittedEvent(Id, TenantId, TotalAmount));
    }

    public void MarkAsSent()
    {
        if (Status != PurchaseOrderStatus.Approved)
            throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        Status = PurchaseOrderStatus.SentToSupplier;
    }

    public void MarkAsPartiallyReceived() => Status = PurchaseOrderStatus.PartiallyReceived;

    public void MarkAsFullyReceived()
    {
        Status = PurchaseOrderStatus.FullyReceived;
        Close();
    }

    public void RecordReceiptDate(DateTimeOffset receiptDate)
    {
        if (!LastReceiptDate.HasValue || receiptDate > LastReceiptDate.Value)
            LastReceiptDate = receiptDate;
    }

    public void Cancel()
    {
        if (Status is PurchaseOrderStatus.Closed or PurchaseOrderStatus.Cancelled or PurchaseOrderStatus.FullyReceived)
            throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        if (HasAnyReceipt)
            throw new BusinessException(ErrorCodes.InvalidStatusTransition);

        Status = PurchaseOrderStatus.Cancelled;
    }

    public void Reject()
    {
        if (Status is not PurchaseOrderStatus.Draft and not PurchaseOrderStatus.PendingApproval)
            throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        Status = PurchaseOrderStatus.Rejected;
    }

    public void Close()
    {
        if (Status is PurchaseOrderStatus.Cancelled or PurchaseOrderStatus.Rejected)
            throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        Status = PurchaseOrderStatus.Closed;
    }

    public void MarkDeleted(string? deletedBy = null)
    {
        EnsureDraft();
        if (HasAnyReceipt)
            throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        SoftDelete(deletedBy);
    }

    public PurchaseOrder CloneAsDraft(string newPoNumber)
    {
        var clone = new PurchaseOrder(
            TenantId, SupplierId, DestinationWarehouseId, newPoNumber,
            DateTimeOffset.UtcNow, ExpectedDeliveryDate, Currency, ExchangeRate, OrderType,
            BranchId, CostCenterId, ResponsibleEmployeeId, PaymentMethod, PaymentTerms,
            ExternalReference, Notes);

        clone.ReplaceLines(_lines.Select(line => (
            line.InventoryItemId, line.UnitId, line.Quantity, line.UnitPrice,
            line.DiscountAmount, line.TaxAmount, line.Description,
            line.WarehouseId, line.LineNotes)));

        return clone;
    }

    private void EnsureDraft()
    {
        if (Status != PurchaseOrderStatus.Draft)
            throw new BusinessException(ErrorCodes.CannotModifyApprovedDocument);
    }

    private void RecalculateTotal()
    {
        TotalAmount = _lines.Sum(l => l.LineTotal);
    }
}

public sealed class PurchaseOrderLine : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid PurchaseOrderId { get; private set; }
    public Guid InventoryItemId { get; private set; }
    public Guid UnitId { get; private set; }
    public Guid? WarehouseId { get; private set; }

    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal ReceivedQuantity { get; private set; }
    public decimal InvoicedQuantity { get; private set; }

    public string? Description { get; private set; }
    public string? LineNotes { get; private set; }

    public decimal RemainingQuantity => Math.Max(0, Quantity - ReceivedQuantity);
    public decimal LineSubTotal => Math.Max(0, (Quantity * UnitPrice) - DiscountAmount);
    public decimal LineTotal => LineSubTotal + TaxAmount;

    private PurchaseOrderLine()
    {
    }

    internal PurchaseOrderLine(
        Guid tenantId,
        Guid purchaseOrderId,
        Guid inventoryItemId,
        Guid unitId,
        decimal quantity,
        decimal unitPrice,
        decimal discountAmount,
        decimal taxAmount,
        string? description,
        Guid? warehouseId,
        string? lineNotes)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        if (unitPrice < 0) throw new ArgumentException("UnitPrice cannot be negative.", nameof(unitPrice));
        if (discountAmount < 0) throw new ArgumentException(nameof(discountAmount));
        if (taxAmount < 0) throw new ArgumentException(nameof(taxAmount));

        TenantId = tenantId;
        PurchaseOrderId = purchaseOrderId;
        InventoryItemId = inventoryItemId;
        UnitId = unitId;
        WarehouseId = warehouseId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        DiscountAmount = discountAmount;
        TaxAmount = taxAmount;
        Description = description;
        LineNotes = lineNotes;
        ReceivedQuantity = 0;
        InvoicedQuantity = 0;
    }

    public void AddReceivedQuantity(decimal qty)
    {
        var next = ReceivedQuantity + qty;
        if (next < 0)
            throw new ArgumentException("Received quantity cannot go below zero.");
        ReceivedQuantity = next;
    }

    public void AddInvoicedQuantity(decimal qty)
    {
        var next = InvoicedQuantity + qty;
        if (next < 0)
            throw new ArgumentException("Invoiced quantity cannot go below zero.");
        InvoicedQuantity = next;
    }
}
