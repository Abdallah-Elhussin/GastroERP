using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Workflow;

namespace GastroErp.Domain.Entities.Workflow;

public sealed class WorkflowDefinition : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; }
    public string Code { get; private set; }
    public string? Description { get; private set; }
    public WorkflowModule Module { get; private set; }
    public WorkflowTrigger Trigger { get; private set; }
    public WorkflowPriority Priority { get; private set; }
    public int Version { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsPublished { get; private set; }

    private WorkflowDefinition()
    {
        Name = string.Empty;
        Code = string.Empty;
    }

    public static WorkflowDefinition Create(Guid tenantId, string name, string code, WorkflowModule module,
        string? description = null, WorkflowTrigger trigger = WorkflowTrigger.Manual,
        WorkflowPriority priority = WorkflowPriority.Normal)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new BusinessException(ErrorCodes.RequiredField, "Name required.");
        if (string.IsNullOrWhiteSpace(code)) throw new BusinessException(ErrorCodes.RequiredField, "Code required.");
        return new WorkflowDefinition
        {
            TenantId = tenantId,
            Name = name.Trim(),
            Code = code.Trim().ToUpperInvariant(),
            Description = description,
            Module = module,
            Trigger = trigger,
            Priority = priority,
            Version = 1,
            IsActive = true,
            IsPublished = false
        };
    }

    public void Update(string name, string? description, WorkflowTrigger trigger, WorkflowPriority priority)
    {
        if (IsPublished) throw new InvalidOperationException("Published workflows cannot be edited.");
        Name = name.Trim();
        Description = description;
        Trigger = trigger;
        Priority = priority;
    }

    public void Publish()
    {
        IsPublished = true;
        Version++;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}

public sealed class WorkflowStep : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid WorkflowDefinitionId { get; private set; }
    public int StepOrder { get; private set; }
    public string Name { get; private set; }
    public ApprovalType ApprovalType { get; private set; }
    public Guid? ApproverRoleId { get; private set; }
    public bool IsFinalStep { get; private set; }

    private WorkflowStep() => Name = string.Empty;

    public static WorkflowStep Create(Guid tenantId, Guid definitionId, int stepOrder, string name,
        ApprovalType approvalType, bool isFinalStep, Guid? approverRoleId = null)
        => new()
        {
            TenantId = tenantId,
            WorkflowDefinitionId = definitionId,
            StepOrder = stepOrder,
            Name = name.Trim(),
            ApprovalType = approvalType,
            IsFinalStep = isFinalStep,
            ApproverRoleId = approverRoleId
        };
}

public sealed class WorkflowCondition : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid WorkflowDefinitionId { get; private set; }
    public string FieldName { get; private set; }
    public WorkflowConditionOperator Operator { get; private set; }
    public string Value { get; private set; }
    public string? LogicalOperator { get; private set; }

    private WorkflowCondition() { FieldName = string.Empty; Value = string.Empty; }

    public static WorkflowCondition Create(Guid tenantId, Guid definitionId, string fieldName,
        WorkflowConditionOperator op, string value, string? logicalOperator = null)
        => new()
        {
            TenantId = tenantId,
            WorkflowDefinitionId = definitionId,
            FieldName = fieldName.Trim(),
            Operator = op,
            Value = value.Trim(),
            LogicalOperator = logicalOperator
        };
}

public sealed class WorkflowInstance : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid WorkflowDefinitionId { get; private set; }
    public string ReferenceType { get; private set; }
    public Guid ReferenceId { get; private set; }
    public int CurrentStepOrder { get; private set; }
    public Guid? CurrentStepId { get; private set; }
    public WorkflowStatus Status { get; private set; }
    public WorkflowPriority Priority { get; private set; }
    public Guid RequestedBy { get; private set; }
    public string? ContextJson { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    private WorkflowInstance()
    {
        ReferenceType = string.Empty;
    }

    public static WorkflowInstance Start(Guid tenantId, Guid definitionId, string referenceType, Guid referenceId,
        Guid requestedBy, WorkflowPriority priority, Guid? firstStepId, int firstStepOrder, string? contextJson = null)
    {
        var instance = new WorkflowInstance
        {
            TenantId = tenantId,
            WorkflowDefinitionId = definitionId,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            CurrentStepOrder = firstStepOrder,
            CurrentStepId = firstStepId,
            Status = WorkflowStatus.InProgress,
            Priority = priority,
            RequestedBy = requestedBy,
            ContextJson = contextJson
        };
        instance.RaiseDomainEvent(new WorkflowStartedEvent(instance.Id, tenantId, referenceType, referenceId));
        if (firstStepId.HasValue)
            instance.RaiseDomainEvent(new ApprovalRequestedEvent(instance.Id, tenantId, firstStepId.Value, firstStepOrder));
        return instance;
    }

    public void AdvanceToStep(Guid stepId, int stepOrder, string stepName)
    {
        CurrentStepId = stepId;
        CurrentStepOrder = stepOrder;
        RaiseDomainEvent(new WorkflowStepCompletedEvent(Id, TenantId, stepOrder, stepName));
        RaiseDomainEvent(new ApprovalRequestedEvent(Id, TenantId, stepId, stepOrder));
    }

    public void Complete()
    {
        Status = WorkflowStatus.Completed;
        CompletedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new WorkflowCompletedEvent(Id, TenantId, ReferenceType, ReferenceId));
    }

    public void Approve(Guid approverId)
    {
        Status = WorkflowStatus.Approved;
        CompletedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new WorkflowApprovedEvent(Id, TenantId, approverId));
        RaiseDomainEvent(new WorkflowCompletedEvent(Id, TenantId, ReferenceType, ReferenceId));
    }

    public void Reject(Guid userId, string reason)
    {
        Status = WorkflowStatus.Rejected;
        CompletedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new WorkflowRejectedEvent(Id, TenantId, userId, reason));
    }

    public void Cancel(Guid userId, string? reason)
    {
        Status = WorkflowStatus.Cancelled;
        CompletedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new WorkflowCancelledEvent(Id, TenantId, userId, reason));
    }

    public void ReturnToPreviousStep(Guid stepId, int stepOrder, string stepName)
    {
        if (Status != WorkflowStatus.InProgress)
            throw new InvalidOperationException("Workflow is not in progress.");
        CurrentStepId = stepId;
        CurrentStepOrder = stepOrder;
        RaiseDomainEvent(new WorkflowReturnedEvent(Id, TenantId, stepOrder));
        RaiseDomainEvent(new ApprovalRequestedEvent(Id, TenantId, stepId, stepOrder));
    }

    public void MarkEscalated(Guid stepId, string role)
        => RaiseDomainEvent(new WorkflowEscalatedEvent(Id, TenantId, stepId, role));

    public void MarkRestarted(Guid newInstanceId)
        => RaiseDomainEvent(new WorkflowRestartedEvent(Id, newInstanceId, TenantId, ReferenceType, ReferenceId));
}

public sealed class WorkflowApproval : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid WorkflowInstanceId { get; private set; }
    public Guid WorkflowStepId { get; private set; }
    public Guid ApproverId { get; private set; }
    public ApprovalDecision Decision { get; private set; }
    public ApprovalStatus Status { get; private set; }
    public DateTimeOffset DecisionDate { get; private set; }
    public string? Comments { get; private set; }

    private WorkflowApproval() { }

    public static WorkflowApproval Record(Guid tenantId, Guid instanceId, Guid stepId, Guid approverId,
        ApprovalDecision decision, string? comments = null)
        => new()
        {
            TenantId = tenantId,
            WorkflowInstanceId = instanceId,
            WorkflowStepId = stepId,
            ApproverId = approverId,
            Decision = decision,
            Status = decision == ApprovalDecision.Approve ? ApprovalStatus.Approved
                : decision == ApprovalDecision.Reject ? ApprovalStatus.Rejected
                : decision == ApprovalDecision.Delegate ? ApprovalStatus.Delegated
                : ApprovalStatus.Skipped,
            DecisionDate = DateTimeOffset.UtcNow,
            Comments = comments
        };
}

public sealed class WorkflowHistory : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid WorkflowInstanceId { get; private set; }
    public string Action { get; private set; }
    public string? OldStatus { get; private set; }
    public string? NewStatus { get; private set; }
    public Guid? UserId { get; private set; }
    public string? Details { get; private set; }

    private WorkflowHistory() => Action = string.Empty;

    public static WorkflowHistory Record(Guid tenantId, Guid instanceId, string action,
        string? oldStatus, string? newStatus, Guid? userId, string? details = null)
        => new()
        {
            TenantId = tenantId,
            WorkflowInstanceId = instanceId,
            Action = action,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            UserId = userId,
            Details = details
        };
}

public sealed class ApprovalDelegate : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid DelegateUserId { get; private set; }
    public DateOnly FromDate { get; private set; }
    public DateOnly ToDate { get; private set; }
    public bool IsActive { get; private set; }
    public string? Reason { get; private set; }

    private ApprovalDelegate() { }

    public static ApprovalDelegate Create(Guid tenantId, Guid userId, Guid delegateUserId,
        DateOnly from, DateOnly to, string? reason = null)
    {
        if (to < from) throw new BusinessException(ErrorCodes.RequiredField, "ToDate must be >= FromDate.");
        var d = new ApprovalDelegate
        {
            TenantId = tenantId,
            UserId = userId,
            DelegateUserId = delegateUserId,
            FromDate = from,
            ToDate = to,
            IsActive = true,
            Reason = reason
        };
        d.RaiseDomainEvent(new DelegationAssignedEvent(d.Id, tenantId, userId, delegateUserId));
        return d;
    }

    public void Update(Guid delegateUserId, DateOnly from, DateOnly to, string? reason)
    {
        DelegateUserId = delegateUserId;
        FromDate = from;
        ToDate = to;
        Reason = reason;
    }

    public void Deactivate() => IsActive = false;
    public bool IsEffectiveOn(DateOnly date) => IsActive && date >= FromDate && date <= ToDate;
}

public sealed class ApprovalEscalation : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid WorkflowStepId { get; private set; }
    public int EscalateAfterHours { get; private set; }
    public string EscalateToRole { get; private set; }
    public bool IsActive { get; private set; }

    private ApprovalEscalation() => EscalateToRole = string.Empty;

    public static ApprovalEscalation Create(Guid tenantId, Guid stepId, int hours, string escalateToRole)
        => new()
        {
            TenantId = tenantId,
            WorkflowStepId = stepId,
            EscalateAfterHours = hours,
            EscalateToRole = escalateToRole.Trim(),
            IsActive = true
        };
}
