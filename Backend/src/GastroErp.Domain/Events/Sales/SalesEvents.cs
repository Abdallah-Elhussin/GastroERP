using GastroErp.Domain.Common;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Events.Sales;

public sealed record OrderCreatedEvent(
    Guid OrderId, Guid BranchId, Guid TenantId,
    OrderType OrderType, SalesChannel Channel) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record OrderSubmittedEvent(
    Guid OrderId, int ItemCount, decimal GrandTotal) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record OrderConfirmedEvent(
    Guid OrderId, Guid BranchId, Guid TenantId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record OrderStatusChangedEvent(
    Guid OrderId, OrderStatus From, OrderStatus To, Guid ChangedBy) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record OrderCancelledEvent(
    Guid OrderId, string Reason, Guid CancelledBy) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record OrderCompletedEvent(
    Guid OrderId, decimal GrandTotal, string Currency, DateTimeOffset CompletedAt) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record OrderReopenedEvent(
    Guid OrderId, Guid ReopenedBy, string Reason) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record OrderItemVoidedEvent(
    Guid OrderId, Guid OrderItemId, string Reason) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record PaymentCompletedEvent(
    Guid PaymentId, Guid OrderId, decimal Amount, string Currency, Guid ShiftId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record PaymentFailedEvent(
    Guid PaymentId, string Reason) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record PaymentVoidedEvent(
    Guid PaymentId, Guid VoidedBy, string Reason) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record PaymentRefundedEvent(
    Guid PaymentId, Guid RefundId, decimal Amount, Guid OrderId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record RegisterOpenedEvent(
    Guid RegisterId, Guid BranchId, Guid OpenedBy) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record RegisterClosedEvent(
    Guid RegisterId, decimal ActualBalance, decimal Difference) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record ShiftOpenedEvent(
    Guid ShiftId, Guid CashierId, Guid DeviceId, Guid RegisterId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record ShiftClosedEvent(
    Guid ShiftId, decimal ActualCash, decimal Variance) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record CashMovementCreatedEvent(
    Guid MovementId, Guid ShiftId, CashMovementType Type, decimal Amount) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record ReconciliationCompletedEvent(
    Guid ShiftId, Guid ReconciledBy, decimal Variance) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record KitchenTicketCreatedEvent(
    Guid TicketId, Guid OrderId, Guid StationId, Guid BranchId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record KitchenTicketStartedEvent(
    Guid TicketId, DateTimeOffset StartedAt) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record KitchenTicketCompletedEvent(
    Guid TicketId, DateTimeOffset CompletedAt, int? PrepMinutes) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record KitchenItemReadyEvent(
    Guid TicketId, Guid ItemId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record TableStatusChangedEvent(
    Guid TableId, TableStatus From, TableStatus To, Guid? OrderId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record TableReservationConfirmedEvent(
    Guid ReservationId, Guid BranchId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record TableReservationCancelledEvent(
    Guid ReservationId, string Reason) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
