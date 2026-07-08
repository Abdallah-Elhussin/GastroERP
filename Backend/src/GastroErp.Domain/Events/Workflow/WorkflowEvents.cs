using GastroErp.Domain.Common;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Events.Workflow;

public sealed record WorkflowStartedEvent(Guid InstanceId, Guid TenantId, string ReferenceType, Guid ReferenceId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record WorkflowStepCompletedEvent(Guid InstanceId, Guid TenantId, int StepOrder, string StepName) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record WorkflowApprovedEvent(Guid InstanceId, Guid TenantId, Guid ApproverId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record WorkflowRejectedEvent(Guid InstanceId, Guid TenantId, Guid UserId, string Reason) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record WorkflowEscalatedEvent(Guid InstanceId, Guid TenantId, Guid StepId, string EscalateToRole) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record WorkflowCancelledEvent(Guid InstanceId, Guid TenantId, Guid UserId, string? Reason) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record WorkflowCompletedEvent(Guid InstanceId, Guid TenantId, string ReferenceType, Guid ReferenceId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record ApprovalRequestedEvent(Guid InstanceId, Guid TenantId, Guid StepId, int StepOrder) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record DelegationAssignedEvent(Guid DelegateId, Guid TenantId, Guid UserId, Guid DelegateUserId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
