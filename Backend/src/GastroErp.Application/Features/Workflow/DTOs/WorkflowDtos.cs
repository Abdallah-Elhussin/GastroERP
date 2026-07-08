using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Workflow.DTOs;

public record WorkflowFilterDto(WorkflowModule? Module = null, bool? IsActive = null, int Page = 1, int PageSize = 50);

public record CreateWorkflowStepDto(int StepOrder, string Name, ApprovalType ApprovalType, bool IsFinalStep, Guid? ApproverRoleId = null);
public record CreateWorkflowConditionDto(string FieldName, WorkflowConditionOperator Operator, string Value, string? LogicalOperator = null);

public record CreateWorkflowDefinitionDto(
    string Name, string Code, WorkflowModule Module, string? Description = null,
    WorkflowTrigger Trigger = WorkflowTrigger.Manual, WorkflowPriority Priority = WorkflowPriority.Normal,
    IReadOnlyList<CreateWorkflowStepDto>? Steps = null,
    IReadOnlyList<CreateWorkflowConditionDto>? Conditions = null);

public record UpdateWorkflowDefinitionDto(
    string Name, string? Description, WorkflowTrigger Trigger, WorkflowPriority Priority);

public record WorkflowStepDto(Guid Id, int StepOrder, string Name, ApprovalType ApprovalType, bool IsFinalStep, Guid? ApproverRoleId);
public record WorkflowConditionDto(Guid Id, string FieldName, WorkflowConditionOperator Operator, string Value);

public record WorkflowDefinitionDto(
    Guid Id, string Name, string Code, string? Description, WorkflowModule Module,
    WorkflowTrigger Trigger, WorkflowPriority Priority, int Version, bool IsActive, bool IsPublished,
    IReadOnlyList<WorkflowStepDto> Steps, IReadOnlyList<WorkflowConditionDto> Conditions);

public record StartWorkflowDto(
    string WorkflowCode, string ReferenceType, Guid ReferenceId,
    string? ContextJson = null, WorkflowPriority? Priority = null);

public record WorkflowInstanceDto(
    Guid Id, Guid WorkflowDefinitionId, string WorkflowName, string ReferenceType, Guid ReferenceId,
    WorkflowStatus Status, WorkflowPriority Priority, int CurrentStepOrder, string? CurrentStepName,
    Guid RequestedBy, DateTimeOffset CreatedAt, DateTimeOffset? CompletedAt);

public record ApproveWorkflowDto(Guid InstanceId, string? Comments = null);
public record RejectWorkflowDto(Guid InstanceId, string Reason);
public record CancelWorkflowDto(Guid InstanceId, string? Reason = null);

public record WorkflowApprovalDto(
    Guid Id, Guid InstanceId, Guid StepId, Guid ApproverId, ApprovalDecision Decision,
    ApprovalStatus Status, DateTimeOffset DecisionDate, string? Comments);

public record WorkflowHistoryDto(
    Guid Id, Guid InstanceId, string Action, string? OldStatus, string? NewStatus,
    Guid? UserId, string? Details, DateTimeOffset CreatedAt);

public record CreateApprovalDelegateDto(Guid DelegateUserId, DateOnly FromDate, DateOnly ToDate, string? Reason = null);
public record UpdateApprovalDelegateDto(Guid DelegateUserId, DateOnly FromDate, DateOnly ToDate, string? Reason = null);
public record ApprovalDelegateDto(
    Guid Id, Guid UserId, Guid DelegateUserId, DateOnly FromDate, DateOnly ToDate, bool IsActive, string? Reason);

public record UserTaskDto(
    Guid InstanceId, string WorkflowName, string ReferenceType, Guid ReferenceId,
    string StepName, WorkflowPriority Priority, DateTimeOffset CreatedAt);

public record WorkflowTimelineDto(
    WorkflowInstanceDto Instance,
    IReadOnlyList<WorkflowHistoryDto> History,
    IReadOnlyList<WorkflowApprovalDto> Approvals);

public record WorkflowStatusQueryDto(string ReferenceType, Guid ReferenceId);

public record RestartWorkflowDto(Guid InstanceId);
public record ReturnWorkflowDto(Guid InstanceId);
