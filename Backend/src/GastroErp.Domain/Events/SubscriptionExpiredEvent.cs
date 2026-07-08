using GastroErp.Domain.Common;

namespace GastroErp.Domain.Events;

public record SubscriptionExpiredEvent(
    Guid TenantId,
    DateTimeOffset ExpiredAt,
    DateTimeOffset OccurredAt
) : IDomainEvent;
