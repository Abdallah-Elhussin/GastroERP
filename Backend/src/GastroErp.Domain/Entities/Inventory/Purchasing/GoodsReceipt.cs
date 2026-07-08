using GastroErp.Domain.Common;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Inventory;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;

namespace GastroErp.Domain.Entities.Inventory.Purchasing;

/// <summary>
/// استلام البضاعة (Aggregate Root)
/// </summary>
public sealed class GoodsReceipt : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid? PurchaseOrderId { get; private set; }
    public Guid SupplierId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public string ReceiptNumber { get; private set; }
    public string? SupplierInvoiceNumber { get; private set; }
    public DateTimeOffset ReceiptDate { get; private set; }
    public GoodsReceiptStatus Status { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<GoodsReceiptLine> _lines = [];
    public IReadOnlyCollection<GoodsReceiptLine> Lines => _lines.AsReadOnly();

    private GoodsReceipt() { ReceiptNumber = string.Empty; }

    public GoodsReceipt(Guid tenantId, Guid supplierId, Guid warehouseId, string receiptNumber,
                        Guid? purchaseOrderId = null, string? supplierInvoiceNumber = null, string? notes = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (supplierId == Guid.Empty) throw new ArgumentException("SupplierId cannot be empty.", nameof(supplierId));
        if (warehouseId == Guid.Empty) throw new ArgumentException("WarehouseId cannot be empty.", nameof(warehouseId));

        TenantId = tenantId;
        SupplierId = supplierId;
        WarehouseId = warehouseId;
        ReceiptNumber = receiptNumber;
        PurchaseOrderId = purchaseOrderId;
        SupplierInvoiceNumber = supplierInvoiceNumber;
        ReceiptDate = DateTimeOffset.UtcNow;
        Notes = notes;
        Status = GoodsReceiptStatus.Draft;
    }

    public void AddLine(Guid inventoryItemId, Guid unitId, decimal receivedQuantity, decimal unitCost,
                        string? batchNumber = null, DateTimeOffset? productionDate = null, DateTimeOffset? expiryDate = null)
    {
        if (Status != GoodsReceiptStatus.Draft)
            throw new BusinessException(ErrorCodes.CannotModifyApprovedDocument);

        _lines.Add(new GoodsReceiptLine(TenantId, Id, inventoryItemId, unitId, receivedQuantity, unitCost, batchNumber, productionDate, expiryDate));
    }

    public void Complete()
    {
        if (Status != GoodsReceiptStatus.Draft) throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        Status = GoodsReceiptStatus.Completed;

        if (PurchaseOrderId.HasValue)
        {
            RaiseDomainEvent(new GoodsReceivedEvent(Id, PurchaseOrderId.Value, WarehouseId, TenantId));
        }
    }

    public void Cancel() => Status = GoodsReceiptStatus.Cancelled;
}

// ─────────────────────────────────────────────────────────────────────────────

public sealed class GoodsReceiptLine : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid GoodsReceiptId { get; private set; }
    public Guid InventoryItemId { get; private set; }
    public Guid UnitId { get; private set; }
    public decimal ReceivedQuantity { get; private set; }
    public decimal UnitCost { get; private set; }
    
    // Batch & Expiry Info directly attached to the receipt line for tracking
    public string? BatchNumber { get; private set; }
    public DateTimeOffset? ProductionDate { get; private set; }
    public DateTimeOffset? ExpiryDate { get; private set; }

    private GoodsReceiptLine() { }

    internal GoodsReceiptLine(Guid tenantId, Guid goodsReceiptId, Guid inventoryItemId, Guid unitId,
                              decimal receivedQuantity, decimal unitCost, string? batchNumber,
                              DateTimeOffset? productionDate, DateTimeOffset? expiryDate)
    {
        if (receivedQuantity <= 0) throw new ArgumentException("Received quantity must be greater than zero.", nameof(receivedQuantity));
        if (unitCost < 0) throw new ArgumentException("UnitCost cannot be negative.", nameof(unitCost));

        TenantId = tenantId;
        GoodsReceiptId = goodsReceiptId;
        InventoryItemId = inventoryItemId;
        UnitId = unitId;
        ReceivedQuantity = receivedQuantity;
        UnitCost = unitCost;
        BatchNumber = batchNumber;
        ProductionDate = productionDate;
        ExpiryDate = expiryDate;
    }
}
