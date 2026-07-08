using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;

namespace GastroErp.Domain.Entities.Inventory.Counting;

/// <summary>
/// تسوية المخزون (Aggregate Root)
/// يستخدم لتصحيح كمية أو تكلفة المخزون يدوياً بعد الجرد أو في حالات الهدر غير المسجلة.
/// </summary>
public sealed class StockAdjustment : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid WarehouseId { get; private set; }
    
    /// <summary>معرف الجرد في حال كانت التسوية ناتجة عن جرد (اختياري)</summary>
    public Guid? StockCountId { get; private set; }

    public string AdjustmentNumber { get; private set; }
    public DateTimeOffset AdjustmentDate { get; private set; }
    public string? Notes { get; private set; }
    public bool IsCompleted { get; private set; }

    private readonly List<StockAdjustmentLine> _lines = [];
    public IReadOnlyCollection<StockAdjustmentLine> Lines => _lines.AsReadOnly();

    private StockAdjustment() { AdjustmentNumber = string.Empty; }

    public StockAdjustment(Guid tenantId, Guid warehouseId, string adjustmentNumber, Guid? stockCountId = null, string? notes = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (warehouseId == Guid.Empty) throw new ArgumentException("WarehouseId cannot be empty.", nameof(warehouseId));

        TenantId = tenantId;
        WarehouseId = warehouseId;
        AdjustmentNumber = adjustmentNumber;
        StockCountId = stockCountId;
        AdjustmentDate = DateTimeOffset.UtcNow;
        Notes = notes;
        IsCompleted = false;
    }

    /// <summary>
    /// إضافة خط تسوية.
    /// إذا كانت AdjustmentQuantity سالبة، تعني نقصاً في المخزون.
    /// إذا كانت موجبة، تعني زيادة.
    /// </summary>
    public void AddLine(Guid inventoryItemId, Guid unitId, Guid reasonId, decimal adjustmentQuantity, decimal? unitCost = null, string? batchNumber = null)
    {
        if (IsCompleted) throw new BusinessException(ErrorCodes.CannotModifyApprovedDocument);
        if (adjustmentQuantity == 0) throw new ArgumentException("Adjustment quantity cannot be zero.", nameof(adjustmentQuantity));

        _lines.Add(new StockAdjustmentLine(TenantId, Id, inventoryItemId, unitId, reasonId, adjustmentQuantity, unitCost, batchNumber));
    }

    public void SubmitForApproval()
    {
        if (IsCompleted) throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        if (_lines.Count == 0) throw new BusinessException(ErrorCodes.RequiredField);
        RaiseDomainEvent(new Domain.Events.Inventory.StockAdjustmentSubmittedEvent(Id, TenantId));
    }

    public void Complete()
    {
        if (IsCompleted) throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        IsCompleted = true;
    }
}

// ─────────────────────────────────────────────────────────────────────────────

public sealed class StockAdjustmentLine : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid StockAdjustmentId { get; private set; }
    public Guid InventoryItemId { get; private set; }
    public Guid UnitId { get; private set; }
    public Guid AdjustmentReasonId { get; private set; }
    
    /// <summary>الكمية المعدلة (يمكن أن تكون سالبة أو موجبة)</summary>
    public decimal AdjustmentQuantity { get; private set; }
    
    /// <summary>التكلفة المرجعية للتعديل إن وجدت</summary>
    public decimal? UnitCost { get; private set; }
    
    public string? BatchNumber { get; private set; }

    private StockAdjustmentLine() { }

    internal StockAdjustmentLine(Guid tenantId, Guid adjustmentId, Guid inventoryItemId, Guid unitId, Guid reasonId, decimal quantity, decimal? unitCost, string? batchNumber)
    {
        if (quantity == 0) throw new ArgumentException("Adjustment quantity cannot be zero.", nameof(quantity));

        TenantId = tenantId;
        StockAdjustmentId = adjustmentId;
        InventoryItemId = inventoryItemId;
        UnitId = unitId;
        AdjustmentReasonId = reasonId;
        AdjustmentQuantity = quantity;
        UnitCost = unitCost;
        BatchNumber = batchNumber;
    }
}
