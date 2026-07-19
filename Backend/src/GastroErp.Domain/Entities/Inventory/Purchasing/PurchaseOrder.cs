using GastroErp.Domain.Common;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Inventory.Purchasing;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;

/// <summary>
/// أمر الشراء (Aggregate Root)
/// </summary>
public sealed class PurchaseOrder : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid SupplierId { get; private set; }
    public Guid DestinationWarehouseId { get; private set; }
    public string PoNumber { get; private set; }
    public DateTimeOffset OrderDate { get; private set; }
    public DateTimeOffset? ExpectedDeliveryDate { get; private set; }
    public PurchaseOrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Currency { get; private set; }
    public string? Notes { get; private set; }
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

    private readonly List<PurchaseOrderLine> _lines = [];
    public IReadOnlyCollection<PurchaseOrderLine> Lines => _lines.AsReadOnly();

    private PurchaseOrder()
    {
        PoNumber = string.Empty;
        Currency = "SAR";
    }

    public PurchaseOrder(Guid tenantId, Guid supplierId, Guid destinationWarehouseId, string poNumber,
                         DateTimeOffset expectedDeliveryDate, string currency = "SAR", string? notes = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (supplierId == Guid.Empty) throw new ArgumentException("SupplierId cannot be empty.", nameof(supplierId));
        if (destinationWarehouseId == Guid.Empty) throw new ArgumentException("DestinationWarehouseId cannot be empty.", nameof(destinationWarehouseId));

        TenantId = tenantId;
        SupplierId = supplierId;
        DestinationWarehouseId = destinationWarehouseId;
        PoNumber = poNumber;
        OrderDate = DateTimeOffset.UtcNow;
        ExpectedDeliveryDate = expectedDeliveryDate;
        Currency = currency.ToUpperInvariant();
        Notes = notes;
        Status = PurchaseOrderStatus.Draft;
        TotalAmount = 0;
    }

    public void AddLine(Guid inventoryItemId, Guid unitId, decimal quantity, decimal unitPrice, decimal taxAmount = 0)
    {
        if (Status != PurchaseOrderStatus.Draft)
            throw new BusinessException(ErrorCodes.CannotModifyApprovedDocument);

        _lines.Add(new PurchaseOrderLine(TenantId, Id, inventoryItemId, unitId, quantity, unitPrice, taxAmount));
        RecalculateTotal();
    }

    public void RemoveLine(Guid lineId)
    {
        if (Status != PurchaseOrderStatus.Draft)
            throw new BusinessException(ErrorCodes.CannotModifyApprovedDocument);

        var line = _lines.FirstOrDefault(l => l.Id == lineId)
            ?? throw new BusinessException(ErrorCodes.ItemNotFound);
        _lines.Remove(line);
        RecalculateTotal();
    }

    private void RecalculateTotal()
    {
        TotalAmount = _lines.Sum(l => l.LineTotal);
    }

    public void Approve()
    {
        if (Status != PurchaseOrderStatus.Draft && Status != PurchaseOrderStatus.PendingApproval)
            throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        Status = PurchaseOrderStatus.Approved;
    }

    public void SubmitForApproval()
    {
        if (Status != PurchaseOrderStatus.Draft) throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        if (_lines.Count == 0) throw new BusinessException(ErrorCodes.RequiredField);
        Status = PurchaseOrderStatus.PendingApproval;
        RaiseDomainEvent(new Domain.Events.Inventory.PurchaseOrderSubmittedEvent(Id, TenantId, TotalAmount));
    }

    public void MarkAsSent()
    {
        if (Status != PurchaseOrderStatus.Approved) throw new BusinessException(ErrorCodes.InvalidStatusTransition);
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

    public void Cancel() => Status = PurchaseOrderStatus.Cancelled;
    public void Reject()
    {
        if (Status is not PurchaseOrderStatus.Draft and not PurchaseOrderStatus.PendingApproval)
            throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        Status = PurchaseOrderStatus.Rejected;
    }
    public void Close() => Status = PurchaseOrderStatus.Closed;
}

// ─────────────────────────────────────────────────────────────────────────────

public sealed class PurchaseOrderLine : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid PurchaseOrderId { get; private set; }
    public Guid InventoryItemId { get; private set; }
    public Guid UnitId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TaxAmount { get; private set; }
    
    /// <summary>الكمية التي تم استلامها فعلياً حتى الآن</summary>
    public decimal ReceivedQuantity { get; private set; }

    public decimal LineTotal => (Quantity * UnitPrice) + TaxAmount;

    private PurchaseOrderLine() { }

    internal PurchaseOrderLine(Guid tenantId, Guid purchaseOrderId, Guid inventoryItemId, Guid unitId, decimal quantity, decimal unitPrice, decimal taxAmount)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        if (unitPrice < 0) throw new ArgumentException("UnitPrice cannot be negative.", nameof(unitPrice));

        TenantId = tenantId;
        PurchaseOrderId = purchaseOrderId;
        InventoryItemId = inventoryItemId;
        UnitId = unitId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TaxAmount = taxAmount;
        ReceivedQuantity = 0;
    }

    public void AddReceivedQuantity(decimal qty)
    {
        var next = ReceivedQuantity + qty;
        if (next < 0)
            throw new ArgumentException("Received quantity cannot go below zero.");
        ReceivedQuantity = next;
    }
}
