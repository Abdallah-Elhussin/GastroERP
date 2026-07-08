using GastroErp.Domain.Common;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Inventory;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;

namespace GastroErp.Domain.Entities.Inventory.Counting;

/// <summary>
/// جرد المخزون (Aggregate Root)
/// </summary>
public sealed class StockCount : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public string CountNumber { get; private set; }
    public DateTimeOffset CountDate { get; private set; }
    public StockCountStatus Status { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<StockCountLine> _lines = [];
    public IReadOnlyCollection<StockCountLine> Lines => _lines.AsReadOnly();

    private StockCount() { CountNumber = string.Empty; }

    public StockCount(Guid tenantId, Guid warehouseId, string countNumber, string? notes = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (warehouseId == Guid.Empty) throw new ArgumentException("WarehouseId cannot be empty.", nameof(warehouseId));

        TenantId = tenantId;
        WarehouseId = warehouseId;
        CountNumber = countNumber;
        CountDate = DateTimeOffset.UtcNow;
        Notes = notes;
        Status = StockCountStatus.Draft;
    }

    public void AddLine(Guid inventoryItemId, Guid unitId, decimal expectedQuantity, decimal actualQuantity, string? batchNumber = null)
    {
        if (Status == StockCountStatus.Completed || Status == StockCountStatus.Cancelled)
            throw new BusinessException(ErrorCodes.CannotModifyApprovedDocument);

        _lines.Add(new StockCountLine(TenantId, Id, inventoryItemId, unitId, expectedQuantity, actualQuantity, batchNumber));
    }

    public void MarkAsInProgress() => Status = StockCountStatus.InProgress;
    public void MarkForReview()
    {
        Status = StockCountStatus.Review;
        RaiseDomainEvent(new Domain.Events.Inventory.StockCountSubmittedEvent(Id, TenantId, WarehouseId));
    }
    
    public void Complete()
    {
        if (Status == StockCountStatus.Completed || Status == StockCountStatus.Cancelled) throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        Status = StockCountStatus.Completed;

        RaiseDomainEvent(new StockCountCompletedEvent(Id, WarehouseId, TenantId));
    }

    public void Cancel() => Status = StockCountStatus.Cancelled;
}

// ─────────────────────────────────────────────────────────────────────────────

public sealed class StockCountLine : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid StockCountId { get; private set; }
    public Guid InventoryItemId { get; private set; }
    public Guid UnitId { get; private set; }
    
    /// <summary>الكمية المتوقعة بناءً على سجل المخزون (System Quantity)</summary>
    public decimal ExpectedQuantity { get; private set; }
    
    /// <summary>الكمية الفعلية التي تم جردها يدوياً (Physical Quantity)</summary>
    public decimal ActualQuantity { get; private set; }
    
    public string? BatchNumber { get; private set; }

    /// <summary>الفرق بين الفعلي والمتوقع (إذا كان سالباً يعني نقص، وإذا كان موجباً يعني زيادة)</summary>
    public decimal Difference => ActualQuantity - ExpectedQuantity;

    private StockCountLine() { }

    internal StockCountLine(Guid tenantId, Guid countId, Guid inventoryItemId, Guid unitId, decimal expectedQuantity, decimal actualQuantity, string? batchNumber)
    {
        if (expectedQuantity < 0) throw new ArgumentException("Expected quantity cannot be negative.");
        if (actualQuantity < 0) throw new ArgumentException("Actual quantity cannot be negative.");

        TenantId = tenantId;
        StockCountId = countId;
        InventoryItemId = inventoryItemId;
        UnitId = unitId;
        ExpectedQuantity = expectedQuantity;
        ActualQuantity = actualQuantity;
        BatchNumber = batchNumber;
    }

    public void UpdateActualQuantity(decimal actualQty)
    {
        if (actualQty < 0) throw new ArgumentException("Actual quantity cannot be negative.");
        ActualQuantity = actualQty;
    }
}
