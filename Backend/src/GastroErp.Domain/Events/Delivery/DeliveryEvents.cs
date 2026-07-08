using GastroErp.Domain.Common;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Events.Delivery;

public sealed record DeliveryOrderCreatedEvent(
    Guid DeliveryOrderId, Guid SalesOrderId, Guid TenantId, Guid BranchId,
    DeliveryProviderType ProviderType) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record DeliveryAssignedEvent(
    Guid DeliveryOrderId, Guid SalesOrderId, Guid DriverId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record DeliveryPickedUpEvent(
    Guid DeliveryOrderId, Guid SalesOrderId, Guid DriverId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record DeliveryCompletedEvent(
    Guid DeliveryOrderId, Guid SalesOrderId, decimal DeliveryFee) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record DeliveryFailedEvent(
    Guid DeliveryOrderId, Guid SalesOrderId, string Reason) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record DriverStatusChangedEvent(
    Guid DriverId, DriverStatus From, DriverStatus To) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record DeliveryReadyForPickupEvent(
    Guid DeliveryOrderId, Guid SalesOrderId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
