using Asp.Versioning;
using GastroErp.Application.Features.Workflow.Commands;
using GastroErp.Application.Features.Workflow.DTOs;
using GastroErp.Application.Features.Workflow.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Workflow;

[ApiVersion("1.0")]
[Tags("Workflow — Definitions")]
public class WorkflowDefinitionsController : BaseApiController
{
    [HttpGet(ApiRoutes.Workflow.Definitions)]
    [HasPermission(Permissions.Workflow.View)]
    public async Task<IActionResult> GetDefinitions([FromQuery] WorkflowFilterDto filter)
        => HandleResult(await Mediator.Send(new GetWorkflowDefinitionsQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Workflow.Definitions}/{{id:guid}}")]
    [HasPermission(Permissions.Workflow.View)]
    public async Task<IActionResult> GetById(Guid id)
        => HandleResult(await Mediator.Send(new GetWorkflowDefinitionByIdQuery(TenantId, id)));

    [HttpPost(ApiRoutes.Workflow.Definitions)]
    [HasPermission(Permissions.Workflow.Create)]
    public async Task<IActionResult> Create([FromBody] CreateWorkflowDefinitionDto dto)
        => HandleResult(await Mediator.Send(new CreateWorkflowDefinitionCommand(TenantId, dto)));

    [HttpPut($"{ApiRoutes.Workflow.Definitions}/{{id:guid}}")]
    [HasPermission(Permissions.Workflow.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWorkflowDefinitionDto dto)
        => HandleResult(await Mediator.Send(new UpdateWorkflowDefinitionCommand(TenantId, id, dto)));

    [HttpPost($"{ApiRoutes.Workflow.Definitions}/{{id:guid}}/publish")]
    [HasPermission(Permissions.Workflow.Publish)]
    public async Task<IActionResult> Publish(Guid id)
        => HandleResult(await Mediator.Send(new PublishWorkflowDefinitionCommand(TenantId, id)));

    [HttpPost($"{ApiRoutes.Workflow.Definitions}/{{id:guid}}/activate")]
    [HasPermission(Permissions.Workflow.Admin)]
    public async Task<IActionResult> Activate(Guid id)
        => HandleResult(await Mediator.Send(new ActivateWorkflowDefinitionCommand(TenantId, id)));

    [HttpPost($"{ApiRoutes.Workflow.Definitions}/{{id:guid}}/deactivate")]
    [HasPermission(Permissions.Workflow.Admin)]
    public async Task<IActionResult> Deactivate(Guid id)
        => HandleResult(await Mediator.Send(new DeactivateWorkflowDefinitionCommand(TenantId, id)));
}

[ApiVersion("1.0")]
[Tags("Workflow — Instances")]
public class WorkflowController : BaseApiController
{
    private Guid CurrentUserId => HttpContext.RequestServices
        .GetRequiredService<GastroErp.Application.Common.Interfaces.ICurrentUser>().Id ?? Guid.Empty;

    [HttpPost($"{ApiRoutes.Workflow.Instances}/start")]
    [HasPermission(Permissions.Workflow.Start)]
    public async Task<IActionResult> Start([FromBody] StartWorkflowDto dto)
        => HandleResult(await Mediator.Send(new StartWorkflowCommand(TenantId, CurrentUserId, dto)));

    [HttpPost($"{ApiRoutes.Workflow.Instances}/approve")]
    [HasPermission(Permissions.Workflow.Approve)]
    public async Task<IActionResult> Approve([FromBody] ApproveWorkflowDto dto)
        => HandleResult(await Mediator.Send(new ApproveWorkflowCommand(TenantId, CurrentUserId, dto)));

    [HttpPost($"{ApiRoutes.Workflow.Instances}/reject")]
    [HasPermission(Permissions.Workflow.Reject)]
    public async Task<IActionResult> Reject([FromBody] RejectWorkflowDto dto)
        => HandleResult(await Mediator.Send(new RejectWorkflowCommand(TenantId, CurrentUserId, dto)));

    [HttpPost($"{ApiRoutes.Workflow.Instances}/cancel")]
    [HasPermission(Permissions.Workflow.Cancel)]
    public async Task<IActionResult> Cancel([FromBody] CancelWorkflowDto dto)
        => HandleResult(await Mediator.Send(new CancelWorkflowCommand(TenantId, CurrentUserId, dto)));

    [HttpGet($"{ApiRoutes.Workflow.Instances}/{{id:guid}}")]
    [HasPermission(Permissions.Workflow.View)]
    public async Task<IActionResult> GetInstance(Guid id)
        => HandleResult(await Mediator.Send(new GetWorkflowInstanceQuery(TenantId, id)));

    [HttpGet($"{ApiRoutes.Workflow.Instances}/{{id:guid}}/history")]
    [HasPermission(Permissions.Workflow.View)]
    public async Task<IActionResult> GetHistory(Guid id)
        => HandleResult(await Mediator.Send(new GetWorkflowHistoryQuery(TenantId, id)));

    [HttpGet($"{ApiRoutes.Workflow.Instances}/{{id:guid}}/approvals")]
    [HasPermission(Permissions.Workflow.View)]
    public async Task<IActionResult> GetApprovals(Guid id)
        => HandleResult(await Mediator.Send(new GetWorkflowApprovalsQuery(TenantId, id)));

    [HttpGet($"{ApiRoutes.Workflow.Instances}/pending")]
    [HasPermission(Permissions.Workflow.View)]
    public async Task<IActionResult> GetPending()
        => HandleResult(await Mediator.Send(new GetPendingApprovalsQuery(TenantId)));

    [HttpGet($"{ApiRoutes.Workflow.Instances}/tasks")]
    [HasPermission(Permissions.Workflow.View)]
    public async Task<IActionResult> GetUserTasks()
        => HandleResult(await Mediator.Send(new GetUserTasksQuery(TenantId, CurrentUserId)));

    [HttpGet($"{ApiRoutes.Workflow.Instances}/status")]
    [HasPermission(Permissions.Workflow.View)]
    public async Task<IActionResult> GetStatus([FromQuery] string referenceType, [FromQuery] Guid referenceId)
        => HandleResult(await Mediator.Send(new GetWorkflowStatusByReferenceQuery(TenantId, referenceType, referenceId)));

    [HttpGet($"{ApiRoutes.Workflow.Instances}/{{id:guid}}/timeline")]
    [HasPermission(Permissions.Workflow.ViewTimeline)]
    public async Task<IActionResult> GetTimeline(Guid id)
        => HandleResult(await Mediator.Send(new GetWorkflowTimelineQuery(TenantId, id)));

    [HttpPost($"{ApiRoutes.Workflow.Instances}/restart")]
    [HasPermission(Permissions.Workflow.Restart)]
    public async Task<IActionResult> Restart([FromBody] RestartWorkflowDto dto)
        => HandleResult(await Mediator.Send(new RestartWorkflowCommand(TenantId, CurrentUserId, dto)));

    [HttpPost($"{ApiRoutes.Workflow.Instances}/return")]
    [HasPermission(Permissions.Workflow.Return)]
    public async Task<IActionResult> Return([FromBody] ReturnWorkflowDto dto)
        => HandleResult(await Mediator.Send(new ReturnWorkflowCommand(TenantId, CurrentUserId, dto)));
}

[ApiVersion("1.0")]
[Tags("Workflow — Delegations")]
public class ApprovalDelegationController : BaseApiController
{
    private Guid CurrentUserId => HttpContext.RequestServices
        .GetRequiredService<GastroErp.Application.Common.Interfaces.ICurrentUser>().Id ?? Guid.Empty;

    [HttpGet(ApiRoutes.Workflow.Delegations)]
    [HasPermission(Permissions.Workflow.Delegate)]
    public async Task<IActionResult> GetActive([FromQuery] Guid? userId)
        => HandleResult(await Mediator.Send(new GetActiveDelegationsQuery(TenantId, userId ?? CurrentUserId)));

    [HttpPost(ApiRoutes.Workflow.Delegations)]
    [HasPermission(Permissions.Workflow.Delegate)]
    public async Task<IActionResult> Create([FromBody] CreateApprovalDelegateDto dto)
        => HandleResult(await Mediator.Send(new CreateApprovalDelegateCommand(TenantId, CurrentUserId, dto)));

    [HttpPut($"{ApiRoutes.Workflow.Delegations}/{{id:guid}}")]
    [HasPermission(Permissions.Workflow.Delegate)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateApprovalDelegateDto dto)
        => HandleResult(await Mediator.Send(new UpdateApprovalDelegateCommand(TenantId, id, dto)));

    [HttpDelete($"{ApiRoutes.Workflow.Delegations}/{{id:guid}}")]
    [HasPermission(Permissions.Workflow.Delegate)]
    public async Task<IActionResult> Delete(Guid id)
        => HandleResult(await Mediator.Send(new DeleteApprovalDelegateCommand(TenantId, id)));
}
