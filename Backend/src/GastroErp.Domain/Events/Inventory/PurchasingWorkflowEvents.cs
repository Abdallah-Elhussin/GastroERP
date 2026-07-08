using GastroErp.Domain.Common;

namespace GastroErp.Domain.Events.Inventory;

public sealed record PurchaseOrderSubmittedEvent(Guid PurchaseOrderId, Guid TenantId, decimal TotalAmount) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record StockCountSubmittedEvent(Guid StockCountId, Guid TenantId, Guid WarehouseId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record StockAdjustmentSubmittedEvent(Guid AdjustmentId, Guid TenantId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record StockTransferSubmittedEvent(Guid TransferId, Guid TenantId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record RefundRequestedEvent(Guid RefundId, Guid TenantId, Guid PaymentId, decimal Amount) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
