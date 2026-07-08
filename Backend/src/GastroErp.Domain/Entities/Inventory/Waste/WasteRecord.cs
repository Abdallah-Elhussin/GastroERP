using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;

namespace GastroErp.Domain.Entities.Inventory.Waste;

/// <summary>
/// سجل الهدر (Aggregate Root)
/// </summary>
public sealed class WasteRecord : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public string RecordNumber { get; private set; }
    public DateTimeOffset WasteDate { get; private set; }
    public string? Notes { get; private set; }
    public bool IsCompleted { get; private set; }

    private readonly List<WasteItem> _items = [];
    public IReadOnlyCollection<WasteItem> Items => _items.AsReadOnly();

    private WasteRecord() { RecordNumber = string.Empty; }

    public WasteRecord(Guid tenantId, Guid warehouseId, string recordNumber, string? notes = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (warehouseId == Guid.Empty) throw new ArgumentException("WarehouseId cannot be empty.", nameof(warehouseId));

        TenantId = tenantId;
        WarehouseId = warehouseId;
        RecordNumber = recordNumber;
        WasteDate = DateTimeOffset.UtcNow;
        Notes = notes;
        IsCompleted = false;
    }

    public void AddItem(Guid inventoryItemId, Guid unitId, Guid wasteReasonId, decimal quantity, decimal unitCost, string? batchNumber = null)
    {
        if (IsCompleted) throw new BusinessException(ErrorCodes.CannotModifyApprovedDocument);
        _items.Add(new WasteItem(TenantId, Id, inventoryItemId, unitId, wasteReasonId, quantity, unitCost, batchNumber));
    }

    public void Complete()
    {
        if (IsCompleted) throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        IsCompleted = true;
    }
}

// ─────────────────────────────────────────────────────────────────────────────

public sealed class WasteItem : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid WasteRecordId { get; private set; }
    public Guid InventoryItemId { get; private set; }
    public Guid UnitId { get; private set; }
    public Guid WasteReasonId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public string? BatchNumber { get; private set; }

    private WasteItem() { }

    internal WasteItem(Guid tenantId, Guid wasteRecordId, Guid inventoryItemId, Guid unitId, Guid wasteReasonId, decimal quantity, decimal unitCost, string? batchNumber)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        TenantId = tenantId;
        WasteRecordId = wasteRecordId;
        InventoryItemId = inventoryItemId;
        UnitId = unitId;
        WasteReasonId = wasteReasonId;
        Quantity = quantity;
        UnitCost = unitCost;
        BatchNumber = batchNumber;
    }
}
