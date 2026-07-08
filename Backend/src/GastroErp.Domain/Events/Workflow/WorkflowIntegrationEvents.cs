using GastroErp.Domain.Common;

namespace GastroErp.Domain.Events.Workflow;

public sealed record LeaveWorkflowCompletedEvent(Guid LeaveId, Guid TenantId, Guid ApprovedBy) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record PurchaseWorkflowCompletedEvent(Guid PurchaseOrderId, Guid TenantId, Guid ApprovedBy) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record JournalWorkflowCompletedEvent(Guid JournalEntryId, Guid TenantId, Guid ApprovedBy) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record RefundWorkflowCompletedEvent(Guid RefundId, Guid TenantId, Guid ApprovedBy) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record WorkflowReturnedEvent(Guid InstanceId, Guid TenantId, int ReturnedToStepOrder) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record WorkflowRestartedEvent(Guid OldInstanceId, Guid NewInstanceId, Guid TenantId, string ReferenceType, Guid ReferenceId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
