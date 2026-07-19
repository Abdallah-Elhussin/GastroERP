using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Inventory.Transactions;

/// <summary>
/// رصيد المخزون المادي لكل صنف/مستودع — يُحدَّث حصراً عبر InventoryMovementPipeline.
/// </summary>
public sealed class InventoryBalance : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid InventoryItemId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public decimal QtyOnHand { get; private set; }
    public decimal ReservedQty { get; private set; }
    public decimal AvgCost { get; private set; }

    public decimal AvailableQty => QtyOnHand - ReservedQty;

    private InventoryBalance() { }

    public InventoryBalance(Guid tenantId, Guid inventoryItemId, Guid warehouseId)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (inventoryItemId == Guid.Empty) throw new ArgumentException("InventoryItemId cannot be empty.", nameof(inventoryItemId));
        if (warehouseId == Guid.Empty) throw new ArgumentException("WarehouseId cannot be empty.", nameof(warehouseId));

        TenantId = tenantId;
        InventoryItemId = inventoryItemId;
        WarehouseId = warehouseId;
    }

    public void ApplyInbound(decimal quantity, decimal unitCost)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");
        if (unitCost < 0) throw new ArgumentOutOfRangeException(nameof(unitCost));

        var oldQty = QtyOnHand;
        var newQty = oldQty + quantity;
        AvgCost = newQty == 0
            ? 0
            : ((oldQty * AvgCost) + (quantity * unitCost)) / newQty;
        QtyOnHand = newQty;
    }

    public void ApplyOutbound(decimal quantity, bool allowNegative)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");

        if (!allowNegative && AvailableQty < quantity)
            throw new BusinessException(ErrorCodes.InsufficientStock);

        QtyOnHand -= quantity;
        // Weighted average unchanged on outbound
    }

    public void Reserve(decimal quantity, bool allowNegative)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");
        if (!allowNegative && AvailableQty < quantity)
            throw new BusinessException(ErrorCodes.InsufficientStock);

        ReservedQty += quantity;
    }

    public void ReleaseReservation(decimal quantity)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");
        ReservedQty = Math.Max(0, ReservedQty - quantity);
    }

    public void FulfillReservation(decimal quantity, bool allowNegative)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");
        if (ReservedQty < quantity)
            throw new BusinessException(ErrorCodes.InsufficientStock);

        ReservedQty -= quantity;
        ApplyOutbound(quantity, allowNegative);
    }
}
