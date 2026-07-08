using GastroErp.Domain.Common;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;

namespace GastroErp.Domain.Entities.Inventory.Purchasing;
/// <summary>
/// مرتجع المشتريات للمورد (Aggregate Root)
/// </summary>
public sealed class PurchaseReturn : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid SupplierId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public Guid? GoodsReceiptId { get; private set; }
    public string ReturnNumber { get; private set; }
    public DateTimeOffset ReturnDate { get; private set; }
    public string? Reason { get; private set; }
    public bool IsCompleted { get; private set; }

    private readonly List<PurchaseReturnLine> _lines = [];
    public IReadOnlyCollection<PurchaseReturnLine> Lines => _lines.AsReadOnly();

    private PurchaseReturn() { ReturnNumber = string.Empty; }

    public PurchaseReturn(Guid tenantId, Guid supplierId, Guid warehouseId, string returnNumber, Guid? goodsReceiptId = null, string? reason = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (supplierId == Guid.Empty) throw new ArgumentException("SupplierId cannot be empty.", nameof(supplierId));
        if (warehouseId == Guid.Empty) throw new ArgumentException("WarehouseId cannot be empty.", nameof(warehouseId));

        TenantId = tenantId;
        SupplierId = supplierId;
        WarehouseId = warehouseId;
        GoodsReceiptId = goodsReceiptId;
        ReturnNumber = returnNumber;
        ReturnDate = DateTimeOffset.UtcNow;
        Reason = reason;
        IsCompleted = false;
    }

    public void AddLine(Guid inventoryItemId, Guid unitId, decimal returnQuantity, decimal unitCost, string? notes = null)
    {
        if (IsCompleted) throw new BusinessException(ErrorCodes.CannotModifyApprovedDocument);
        _lines.Add(new PurchaseReturnLine(TenantId, Id, inventoryItemId, unitId, returnQuantity, unitCost, notes));
    }

    public void Complete()
    {
        if (IsCompleted) throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        IsCompleted = true;
    }
}

// ─────────────────────────────────────────────────────────────────────────────

public sealed class PurchaseReturnLine : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid PurchaseReturnId { get; private set; }
    public Guid InventoryItemId { get; private set; }
    public Guid UnitId { get; private set; }
    public decimal ReturnQuantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public string? Notes { get; private set; }

    private PurchaseReturnLine() { }

    internal PurchaseReturnLine(Guid tenantId, Guid purchaseReturnId, Guid inventoryItemId, Guid unitId, decimal returnQuantity, decimal unitCost, string? notes)
    {
        if (returnQuantity <= 0) throw new ArgumentException("Return quantity must be greater than zero.", nameof(returnQuantity));

        TenantId = tenantId;
        PurchaseReturnId = purchaseReturnId;
        InventoryItemId = inventoryItemId;
        UnitId = unitId;
        ReturnQuantity = returnQuantity;
        UnitCost = unitCost;
        Notes = notes;
    }
}
