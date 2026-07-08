using GastroErp.Domain.Common;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Events.Finance;

public sealed record AccountCreatedEvent(
    Guid AccountId, Guid TenantId, string AccountNumber, string NameAr) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record CostCenterCreatedEvent(
    Guid CostCenterId, Guid TenantId, string Code) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record JournalPostedEvent(
    Guid JournalEntryId, Guid TenantId, string EntryNumber, PostingSource Source) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record JournalReversedEvent(
    Guid OriginalJournalId, Guid ReversalJournalId, Guid TenantId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record FiscalPeriodClosedEvent(
    Guid FiscalPeriodId, Guid TenantId, int FiscalYear) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
