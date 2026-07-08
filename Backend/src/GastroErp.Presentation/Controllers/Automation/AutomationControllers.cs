using Asp.Versioning;
using GastroErp.Application.Features.Automation.Commands;
using GastroErp.Application.Features.Automation.DTOs;
using GastroErp.Application.Features.Automation.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Automation;

[ApiVersion("1.0")]
public class JobsController : BaseApiController
{
    [HttpGet(ApiRoutes.Jobs.Base)]
    [HasPermission(Permissions.Jobs.View)]
    public async Task<IActionResult> GetJobs([FromQuery] int take = 50)
        => HandleResult(await Mediator.Send(new GetJobsQuery(TenantId, take)));

    [HttpGet($"{ApiRoutes.Jobs.Base}/monitoring")]
    [HasPermission(Permissions.System.Monitor)]
    public async Task<IActionResult> GetMonitoring()
        => HandleResult(await Mediator.Send(new GetJobMonitoringQuery(TenantId)));

    [HttpGet($"{ApiRoutes.Jobs.Base}/history")]
    [HasPermission(Permissions.Jobs.View)]
    public async Task<IActionResult> GetHistory([FromQuery] int take = 50)
        => HandleResult(await Mediator.Send(new GetJobHistoryQuery(TenantId, take)));

    [HttpPost($"{ApiRoutes.Jobs.Base}/execute")]
    [HasPermission(Permissions.Jobs.Execute)]
    public async Task<IActionResult> ExecuteJob([FromBody] ExecuteJobDto dto)
        => HandleResult(await Mediator.Send(new ExecuteJobCommand(TenantId, dto)));

    [HttpPost($"{ApiRoutes.Jobs.Base}/{{id:guid}}/retry")]
    [HasPermission(Permissions.Jobs.Manage)]
    public async Task<IActionResult> RetryJob(Guid id)
        => HandleResult(await Mediator.Send(new RetryJobCommand(TenantId, id)));

    [HttpPost($"{ApiRoutes.Jobs.Base}/{{id:guid}}/cancel")]
    [HasPermission(Permissions.Jobs.Manage)]
    public async Task<IActionResult> CancelJob(Guid id)
        => HandleResult(await Mediator.Send(new CancelJobCommand(TenantId, id)));
}

[ApiVersion("1.0")]
public class NotificationsController : BaseApiController
{
    private Guid CurrentUserId => HttpContext.RequestServices
        .GetRequiredService<GastroErp.Application.Common.Interfaces.ICurrentUser>().Id ?? Guid.Empty;

    [HttpGet(ApiRoutes.Notifications.Base)]
    [HasPermission(Permissions.Notifications.View)]
    public async Task<IActionResult> GetUserNotifications([FromQuery] NotificationFilterDto filter)
        => HandleResult(await Mediator.Send(new GetUserNotificationsQuery(TenantId, CurrentUserId, filter)));

    [HttpPost(ApiRoutes.Notifications.Base)]
    [HasPermission(Permissions.Notifications.Send)]
    public async Task<IActionResult> Send([FromBody] SendNotificationDto dto)
        => HandleResult(await Mediator.Send(new SendNotificationCommand(TenantId, dto)));

    [HttpPost($"{ApiRoutes.Notifications.Base}/{{id:guid}}/resend")]
    [HasPermission(Permissions.Notifications.Send)]
    public async Task<IActionResult> Resend(Guid id)
        => HandleResult(await Mediator.Send(new ResendNotificationCommand(TenantId, id)));

    [HttpPost($"{ApiRoutes.Notifications.Base}/{{id:guid}}/read")]
    [HasPermission(Permissions.Notifications.View)]
    public async Task<IActionResult> MarkRead(Guid id)
        => HandleResult(await Mediator.Send(new MarkNotificationReadCommand(id)));

    [HttpPost($"{ApiRoutes.Notifications.Base}/{{id:guid}}/archive")]
    [HasPermission(Permissions.Notifications.View)]
    public async Task<IActionResult> Archive(Guid id)
        => HandleResult(await Mediator.Send(new ArchiveNotificationCommand(id)));
}

[ApiVersion("1.0")]
public class IntegrationsController : BaseApiController
{
    [HttpGet(ApiRoutes.Integrations.Base)]
    [HasPermission(Permissions.Integrations.View)]
    public async Task<IActionResult> GetIntegrations()
        => HandleResult(await Mediator.Send(new GetIntegrationsQuery(TenantId)));

    [HttpPut(ApiRoutes.Integrations.Base)]
    [HasPermission(Permissions.Integrations.Manage)]
    public async Task<IActionResult> Upsert([FromBody] UpsertIntegrationDto dto)
        => HandleResult(await Mediator.Send(new UpsertIntegrationCommand(TenantId, dto)));

    [HttpPost($"{ApiRoutes.Integrations.Base}/test")]
    [HasPermission(Permissions.Integrations.Manage)]
    public async Task<IActionResult> TestConnection([FromBody] TestIntegrationDto dto)
        => HandleResult(await Mediator.Send(new TestIntegrationCommand(TenantId, dto)));

    [HttpGet($"{ApiRoutes.Integrations.Base}/status")]
    [HasPermission(Permissions.Integrations.View)]
    public async Task<IActionResult> GetStatus([FromQuery] TestIntegrationDto dto)
        => HandleResult(await Mediator.Send(new TestIntegrationCommand(TenantId, dto)));

    [HttpPost($"{ApiRoutes.Integrations.Base}/webhooks/inbound")]
    [HasPermission(Permissions.Integrations.Manage)]
    public async Task<IActionResult> ReceiveInbound([FromBody] InboundWebhookDto dto)
        => HandleResult(await Mediator.Send(new ProcessInboundWebhookCommand(TenantId, dto)));
}
