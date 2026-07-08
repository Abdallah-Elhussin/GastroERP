using GastroErp.Domain.Common;

namespace GastroErp.Domain.Events.Inventory;

/// <summary>حدث: تم إنشاء مادة خام جديدة</summary>
public sealed record InventoryItemCreatedEvent(Guid ItemId, Guid TenantId, string Name) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

/// <summary>حدث: تم استلام بضاعة بنجاح</summary>
public sealed record GoodsReceivedEvent(Guid GoodsReceiptId, Guid PurchaseOrderId, Guid WarehouseId, Guid TenantId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

/// <summary>حدث: تم تسجيل حركة مخزنية جديدة</summary>
public sealed record StockMovementRecordedEvent(Guid MovementId, Guid TransactionId, Guid ItemId, Guid WarehouseId, decimal QuantityChange, Guid TenantId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

/// <summary>حدث: تم حجز كمية من المخزون</summary>
public sealed record StockReservedEvent(Guid ReservationId, Guid ItemId, Guid WarehouseId, decimal Quantity, Guid TenantId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

/// <summary>حدث: انخفاض المخزون عن حد إعادة الطلب</summary>
public sealed record ReorderLevelReachedEvent(Guid ItemId, Guid WarehouseId, decimal CurrentStock, Guid TenantId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

/// <summary>حدث: انتهاء صلاحية تشغيلة (دفعة)</summary>
public sealed record BatchExpiredEvent(Guid BatchId, Guid ItemId, Guid WarehouseId, Guid TenantId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

/// <summary>حدث: إكمال جرد المستودع</summary>
public sealed record StockCountCompletedEvent(Guid StockCountId, Guid WarehouseId, Guid TenantId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
