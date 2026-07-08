using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Workflow.Commands;
using GastroErp.Application.Features.Workflow.DTOs;
using GastroErp.Application.Features.Workflow.Queries;
using GastroErp.Application.Features.Workflow.Services;
using MediatR;

namespace GastroErp.Application.Features.Workflow.Commands;

public sealed class CreateWorkflowDefinitionCommandHandler : IRequestHandler<CreateWorkflowDefinitionCommand, Result<WorkflowDefinitionDto>>
{
    private readonly IWorkflowDefinitionService _service;
    public CreateWorkflowDefinitionCommandHandler(IWorkflowDefinitionService service) => _service = service;
    public async Task<Result<WorkflowDefinitionDto>> Handle(CreateWorkflowDefinitionCommand request, CancellationToken ct)
        => Result<WorkflowDefinitionDto>.Success(await _service.CreateAsync(request.TenantId, request.Dto, ct));
}

public sealed class UpdateWorkflowDefinitionCommandHandler : IRequestHandler<UpdateWorkflowDefinitionCommand, Result<WorkflowDefinitionDto>>
{
    private readonly IWorkflowDefinitionService _service;
    public UpdateWorkflowDefinitionCommandHandler(IWorkflowDefinitionService service) => _service = service;
    public async Task<Result<WorkflowDefinitionDto>> Handle(UpdateWorkflowDefinitionCommand request, CancellationToken ct)
        => Result<WorkflowDefinitionDto>.Success(await _service.UpdateAsync(request.TenantId, request.Id, request.Dto, ct));
}

public sealed class PublishWorkflowDefinitionCommandHandler : IRequestHandler<PublishWorkflowDefinitionCommand, Result<WorkflowDefinitionDto>>
{
    private readonly IWorkflowDefinitionService _service;
    public PublishWorkflowDefinitionCommandHandler(IWorkflowDefinitionService service) => _service = service;
    public async Task<Result<WorkflowDefinitionDto>> Handle(PublishWorkflowDefinitionCommand request, CancellationToken ct)
        => Result<WorkflowDefinitionDto>.Success(await _service.PublishAsync(request.TenantId, request.Id, ct));
}

public sealed class ActivateWorkflowDefinitionCommandHandler : IRequestHandler<ActivateWorkflowDefinitionCommand, Result>
{
    private readonly IWorkflowDefinitionService _service;
    public ActivateWorkflowDefinitionCommandHandler(IWorkflowDefinitionService service) => _service = service;
    public async Task<Result> Handle(ActivateWorkflowDefinitionCommand request, CancellationToken ct)
    {
        await _service.ActivateAsync(request.TenantId, request.Id, ct);
        return Result.Success();
    }
}

public sealed class DeactivateWorkflowDefinitionCommandHandler : IRequestHandler<DeactivateWorkflowDefinitionCommand, Result>
{
    private readonly IWorkflowDefinitionService _service;
    public DeactivateWorkflowDefinitionCommandHandler(IWorkflowDefinitionService service) => _service = service;
    public async Task<Result> Handle(DeactivateWorkflowDefinitionCommand request, CancellationToken ct)
    {
        await _service.DeactivateAsync(request.TenantId, request.Id, ct);
        return Result.Success();
    }
}

public sealed class StartWorkflowCommandHandler : IRequestHandler<StartWorkflowCommand, Result<WorkflowInstanceDto>>
{
    private readonly IWorkflowEngine _engine;
    public StartWorkflowCommandHandler(IWorkflowEngine engine) => _engine = engine;
    public async Task<Result<WorkflowInstanceDto>> Handle(StartWorkflowCommand request, CancellationToken ct)
        => Result<WorkflowInstanceDto>.Success(await _engine.StartAsync(request.TenantId, request.UserId, request.Dto, ct));
}

public sealed class ApproveWorkflowCommandHandler : IRequestHandler<ApproveWorkflowCommand, Result<WorkflowInstanceDto>>
{
    private readonly IWorkflowEngine _engine;
    public ApproveWorkflowCommandHandler(IWorkflowEngine engine) => _engine = engine;
    public async Task<Result<WorkflowInstanceDto>> Handle(ApproveWorkflowCommand request, CancellationToken ct)
        => Result<WorkflowInstanceDto>.Success(await _engine.ApproveAsync(request.TenantId, request.UserId, request.Dto, ct));
}

public sealed class RejectWorkflowCommandHandler : IRequestHandler<RejectWorkflowCommand, Result<WorkflowInstanceDto>>
{
    private readonly IWorkflowEngine _engine;
    public RejectWorkflowCommandHandler(IWorkflowEngine engine) => _engine = engine;
    public async Task<Result<WorkflowInstanceDto>> Handle(RejectWorkflowCommand request, CancellationToken ct)
        => Result<WorkflowInstanceDto>.Success(await _engine.RejectAsync(request.TenantId, request.UserId, request.Dto, ct));
}

public sealed class CancelWorkflowCommandHandler : IRequestHandler<CancelWorkflowCommand, Result<WorkflowInstanceDto>>
{
    private readonly IWorkflowEngine _engine;
    public CancelWorkflowCommandHandler(IWorkflowEngine engine) => _engine = engine;
    public async Task<Result<WorkflowInstanceDto>> Handle(CancelWorkflowCommand request, CancellationToken ct)
        => Result<WorkflowInstanceDto>.Success(await _engine.CancelAsync(request.TenantId, request.UserId, request.Dto, ct));
}

public sealed class CreateApprovalDelegateCommandHandler : IRequestHandler<CreateApprovalDelegateCommand, Result<ApprovalDelegateDto>>
{
    private readonly IDelegateService _service;
    public CreateApprovalDelegateCommandHandler(IDelegateService service) => _service = service;
    public async Task<Result<ApprovalDelegateDto>> Handle(CreateApprovalDelegateCommand request, CancellationToken ct)
        => Result<ApprovalDelegateDto>.Success(await _service.CreateAsync(request.TenantId, request.UserId, request.Dto, ct));
}

public sealed class UpdateApprovalDelegateCommandHandler : IRequestHandler<UpdateApprovalDelegateCommand, Result<ApprovalDelegateDto>>
{
    private readonly IDelegateService _service;
    public UpdateApprovalDelegateCommandHandler(IDelegateService service) => _service = service;
    public async Task<Result<ApprovalDelegateDto>> Handle(UpdateApprovalDelegateCommand request, CancellationToken ct)
        => Result<ApprovalDelegateDto>.Success(await _service.UpdateAsync(request.TenantId, request.Id, request.Dto, ct));
}

public sealed class DeleteApprovalDelegateCommandHandler : IRequestHandler<DeleteApprovalDelegateCommand, Result>
{
    private readonly IDelegateService _service;
    public DeleteApprovalDelegateCommandHandler(IDelegateService service) => _service = service;
    public async Task<Result> Handle(DeleteApprovalDelegateCommand request, CancellationToken ct)
    {
        await _service.DeleteAsync(request.TenantId, request.Id, ct);
        return Result.Success();
    }
}

public sealed class RestartWorkflowCommandHandler : IRequestHandler<RestartWorkflowCommand, Result<WorkflowInstanceDto>>
{
    private readonly IWorkflowEngine _engine;
    public RestartWorkflowCommandHandler(IWorkflowEngine engine) => _engine = engine;
    public async Task<Result<WorkflowInstanceDto>> Handle(RestartWorkflowCommand request, CancellationToken ct)
        => Result<WorkflowInstanceDto>.Success(await _engine.RestartAsync(request.TenantId, request.UserId, request.Dto.InstanceId, ct));
}

public sealed class ReturnWorkflowCommandHandler : IRequestHandler<ReturnWorkflowCommand, Result<WorkflowInstanceDto>>
{
    private readonly IWorkflowEngine _engine;
    public ReturnWorkflowCommandHandler(IWorkflowEngine engine) => _engine = engine;
    public async Task<Result<WorkflowInstanceDto>> Handle(ReturnWorkflowCommand request, CancellationToken ct)
        => Result<WorkflowInstanceDto>.Success(await _engine.ReturnToPreviousStepAsync(request.TenantId, request.UserId, request.Dto.InstanceId, ct));
}
