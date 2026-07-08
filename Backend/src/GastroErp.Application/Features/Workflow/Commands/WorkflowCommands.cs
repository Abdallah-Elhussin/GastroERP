using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Workflow.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Workflow.Commands;

public record CreateWorkflowDefinitionCommand(Guid TenantId, CreateWorkflowDefinitionDto Dto) : IRequest<Result<WorkflowDefinitionDto>>;
public record UpdateWorkflowDefinitionCommand(Guid TenantId, Guid Id, UpdateWorkflowDefinitionDto Dto) : IRequest<Result<WorkflowDefinitionDto>>;
public record PublishWorkflowDefinitionCommand(Guid TenantId, Guid Id) : IRequest<Result<WorkflowDefinitionDto>>;
public record ActivateWorkflowDefinitionCommand(Guid TenantId, Guid Id) : IRequest<Result>;
public record DeactivateWorkflowDefinitionCommand(Guid TenantId, Guid Id) : IRequest<Result>;

public record StartWorkflowCommand(Guid TenantId, Guid UserId, StartWorkflowDto Dto) : IRequest<Result<WorkflowInstanceDto>>;
public record ApproveWorkflowCommand(Guid TenantId, Guid UserId, ApproveWorkflowDto Dto) : IRequest<Result<WorkflowInstanceDto>>;
public record RejectWorkflowCommand(Guid TenantId, Guid UserId, RejectWorkflowDto Dto) : IRequest<Result<WorkflowInstanceDto>>;
public record CancelWorkflowCommand(Guid TenantId, Guid UserId, CancelWorkflowDto Dto) : IRequest<Result<WorkflowInstanceDto>>;

public record CreateApprovalDelegateCommand(Guid TenantId, Guid UserId, CreateApprovalDelegateDto Dto) : IRequest<Result<ApprovalDelegateDto>>;
public record UpdateApprovalDelegateCommand(Guid TenantId, Guid Id, UpdateApprovalDelegateDto Dto) : IRequest<Result<ApprovalDelegateDto>>;
public record DeleteApprovalDelegateCommand(Guid TenantId, Guid Id) : IRequest<Result>;

public record RestartWorkflowCommand(Guid TenantId, Guid UserId, RestartWorkflowDto Dto) : IRequest<Result<WorkflowInstanceDto>>;
public record ReturnWorkflowCommand(Guid TenantId, Guid UserId, ReturnWorkflowDto Dto) : IRequest<Result<WorkflowInstanceDto>>;
