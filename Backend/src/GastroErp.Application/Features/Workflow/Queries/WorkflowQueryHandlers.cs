using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Workflow.DTOs;
using GastroErp.Application.Features.Workflow.Queries;
using GastroErp.Application.Features.Workflow.Services;
using MediatR;

namespace GastroErp.Application.Features.Workflow.Queries;

public sealed class GetWorkflowDefinitionsQueryHandler : IRequestHandler<GetWorkflowDefinitionsQuery, Result<IReadOnlyList<WorkflowDefinitionDto>>>
{
    private readonly IWorkflowDefinitionService _service;
    public GetWorkflowDefinitionsQueryHandler(IWorkflowDefinitionService service) => _service = service;
    public async Task<Result<IReadOnlyList<WorkflowDefinitionDto>>> Handle(GetWorkflowDefinitionsQuery request, CancellationToken ct)
        => Result<IReadOnlyList<WorkflowDefinitionDto>>.Success(await _service.ListAsync(request.TenantId, request.Filter, ct));
}

public sealed class GetWorkflowDefinitionByIdQueryHandler : IRequestHandler<GetWorkflowDefinitionByIdQuery, Result<WorkflowDefinitionDto>>
{
    private readonly IWorkflowDefinitionService _service;
    public GetWorkflowDefinitionByIdQueryHandler(IWorkflowDefinitionService service) => _service = service;
    public async Task<Result<WorkflowDefinitionDto>> Handle(GetWorkflowDefinitionByIdQuery request, CancellationToken ct)
    {
        var def = await _service.GetByIdAsync(request.TenantId, request.Id, ct);
        return def is null
            ? Result<WorkflowDefinitionDto>.Failure("NotFound", "Workflow definition not found.")
            : Result<WorkflowDefinitionDto>.Success(def);
    }
}

public sealed class GetWorkflowInstanceQueryHandler : IRequestHandler<GetWorkflowInstanceQuery, Result<WorkflowInstanceDto>>
{
    private readonly IWorkflowEngine _engine;
    public GetWorkflowInstanceQueryHandler(IWorkflowEngine engine) => _engine = engine;
    public async Task<Result<WorkflowInstanceDto>> Handle(GetWorkflowInstanceQuery request, CancellationToken ct)
    {
        var inst = await _engine.GetInstanceAsync(request.TenantId, request.InstanceId, ct);
        return inst is null
            ? Result<WorkflowInstanceDto>.Failure("NotFound", "Workflow instance not found.")
            : Result<WorkflowInstanceDto>.Success(inst);
    }
}

public sealed class GetWorkflowHistoryQueryHandler : IRequestHandler<GetWorkflowHistoryQuery, Result<IReadOnlyList<WorkflowHistoryDto>>>
{
    private readonly IWorkflowHistoryService _service;
    public GetWorkflowHistoryQueryHandler(IWorkflowHistoryService service) => _service = service;
    public async Task<Result<IReadOnlyList<WorkflowHistoryDto>>> Handle(GetWorkflowHistoryQuery request, CancellationToken ct)
        => Result<IReadOnlyList<WorkflowHistoryDto>>.Success(await _service.GetHistoryAsync(request.TenantId, request.InstanceId, ct));
}

public sealed class GetWorkflowApprovalsQueryHandler : IRequestHandler<GetWorkflowApprovalsQuery, Result<IReadOnlyList<WorkflowApprovalDto>>>
{
    private readonly IApprovalService _service;
    public GetWorkflowApprovalsQueryHandler(IApprovalService service) => _service = service;
    public async Task<Result<IReadOnlyList<WorkflowApprovalDto>>> Handle(GetWorkflowApprovalsQuery request, CancellationToken ct)
        => Result<IReadOnlyList<WorkflowApprovalDto>>.Success(await _service.GetApprovalsAsync(request.TenantId, request.InstanceId, ct));
}

public sealed class GetPendingApprovalsQueryHandler : IRequestHandler<GetPendingApprovalsQuery, Result<IReadOnlyList<WorkflowInstanceDto>>>
{
    private readonly IApprovalService _service;
    public GetPendingApprovalsQueryHandler(IApprovalService service) => _service = service;
    public async Task<Result<IReadOnlyList<WorkflowInstanceDto>>> Handle(GetPendingApprovalsQuery request, CancellationToken ct)
        => Result<IReadOnlyList<WorkflowInstanceDto>>.Success(await _service.GetPendingApprovalsAsync(request.TenantId, ct));
}

public sealed class GetUserTasksQueryHandler : IRequestHandler<GetUserTasksQuery, Result<IReadOnlyList<UserTaskDto>>>
{
    private readonly IApprovalService _service;
    public GetUserTasksQueryHandler(IApprovalService service) => _service = service;
    public async Task<Result<IReadOnlyList<UserTaskDto>>> Handle(GetUserTasksQuery request, CancellationToken ct)
        => Result<IReadOnlyList<UserTaskDto>>.Success(await _service.GetUserTasksAsync(request.TenantId, request.UserId, ct));
}

public sealed class GetActiveDelegationsQueryHandler : IRequestHandler<GetActiveDelegationsQuery, Result<IReadOnlyList<ApprovalDelegateDto>>>
{
    private readonly IDelegateService _service;
    public GetActiveDelegationsQueryHandler(IDelegateService service) => _service = service;
    public async Task<Result<IReadOnlyList<ApprovalDelegateDto>>> Handle(GetActiveDelegationsQuery request, CancellationToken ct)
        => Result<IReadOnlyList<ApprovalDelegateDto>>.Success(await _service.GetActiveDelegationsAsync(request.TenantId, request.UserId, ct));
}

public sealed class GetWorkflowStatusByReferenceQueryHandler : IRequestHandler<GetWorkflowStatusByReferenceQuery, Result<WorkflowInstanceDto>>
{
    private readonly IWorkflowIntegrationService _integration;
    public GetWorkflowStatusByReferenceQueryHandler(IWorkflowIntegrationService integration) => _integration = integration;
    public async Task<Result<WorkflowInstanceDto>> Handle(GetWorkflowStatusByReferenceQuery request, CancellationToken ct)
    {
        var inst = await _integration.GetStatusByReferenceAsync(request.TenantId, request.ReferenceType, request.ReferenceId, ct);
        return inst is null
            ? Result<WorkflowInstanceDto>.Failure("NotFound", "No workflow found for reference.")
            : Result<WorkflowInstanceDto>.Success(inst);
    }
}

public sealed class GetWorkflowTimelineQueryHandler : IRequestHandler<GetWorkflowTimelineQuery, Result<WorkflowTimelineDto>>
{
    private readonly IWorkflowIntegrationService _integration;
    public GetWorkflowTimelineQueryHandler(IWorkflowIntegrationService integration) => _integration = integration;
    public async Task<Result<WorkflowTimelineDto>> Handle(GetWorkflowTimelineQuery request, CancellationToken ct)
    {
        var timeline = await _integration.GetTimelineAsync(request.TenantId, request.InstanceId, ct);
        return timeline is null
            ? Result<WorkflowTimelineDto>.Failure("NotFound", "Workflow instance not found.")
            : Result<WorkflowTimelineDto>.Success(timeline);
    }
}
