using GastroErp.Domain.Common;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Events.Invoicing;

public sealed record InvoiceCreatedEvent(
    Guid InvoiceId, Guid TenantId, Guid BranchId, InvoiceType InvoiceType, string InvoiceNumber) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record InvoiceFinalizedEvent(
    Guid InvoiceId, Guid? SalesOrderId, decimal GrandTotal, string Currency) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record InvoiceCancelledEvent(
    Guid InvoiceId, string Reason, Guid CancelledBy) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record InvoicePrintedEvent(
    Guid InvoiceId, int PrintCount) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record CreditNoteIssuedEvent(
    Guid CreditNoteId, Guid InvoiceId, decimal Amount, CreditNoteType CreditType) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record DebitNoteIssuedEvent(
    Guid DebitNoteId, Guid InvoiceId, decimal Amount) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record TaxCalculatedEvent(
    Guid EntityId, string EntityType, decimal TaxAmount, string Currency) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
