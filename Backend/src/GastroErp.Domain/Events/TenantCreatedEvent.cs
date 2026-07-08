using GastroErp.Domain.Common;

namespace GastroErp.Domain.Events;

public record TenantCreatedEvent(
    Guid TenantId,
    string Name,
    string Domain,
    string AdminEmail,
    DateTimeOffset OccurredAt
) : IDomainEvent;
