using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Workflow.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Workflow.Queries;

public record GetWorkflowDefinitionsQuery(Guid TenantId, WorkflowFilterDto Filter) : IRequest<Result<IReadOnlyList<WorkflowDefinitionDto>>>;
public record GetWorkflowDefinitionByIdQuery(Guid TenantId, Guid Id) : IRequest<Result<WorkflowDefinitionDto>>;
public record GetWorkflowInstanceQuery(Guid TenantId, Guid InstanceId) : IRequest<Result<WorkflowInstanceDto>>;
public record GetWorkflowHistoryQuery(Guid TenantId, Guid InstanceId) : IRequest<Result<IReadOnlyList<WorkflowHistoryDto>>>;
public record GetWorkflowApprovalsQuery(Guid TenantId, Guid InstanceId) : IRequest<Result<IReadOnlyList<WorkflowApprovalDto>>>;
public record GetPendingApprovalsQuery(Guid TenantId) : IRequest<Result<IReadOnlyList<WorkflowInstanceDto>>>;
public record GetUserTasksQuery(Guid TenantId, Guid UserId) : IRequest<Result<IReadOnlyList<UserTaskDto>>>;
public record GetActiveDelegationsQuery(Guid TenantId, Guid? UserId) : IRequest<Result<IReadOnlyList<ApprovalDelegateDto>>>;
public record GetWorkflowStatusByReferenceQuery(Guid TenantId, string ReferenceType, Guid ReferenceId) : IRequest<Result<WorkflowInstanceDto>>;
public record GetWorkflowTimelineQuery(Guid TenantId, Guid InstanceId) : IRequest<Result<WorkflowTimelineDto>>;
