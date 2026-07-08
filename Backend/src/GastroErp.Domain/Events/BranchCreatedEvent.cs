using GastroErp.Domain.Common;

namespace GastroErp.Domain.Events;

public record BranchCreatedEvent(
    Guid BranchId,
    Guid CompanyId,
    Guid TenantId,
    string Name,
    DateTimeOffset OccurredAt
) : IDomainEvent;
