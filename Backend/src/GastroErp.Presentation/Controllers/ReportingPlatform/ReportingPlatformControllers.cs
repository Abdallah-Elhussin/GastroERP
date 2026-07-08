using Asp.Versioning;
using GastroErp.Application.Features.ReportingPlatform.Commands;
using GastroErp.Application.Features.ReportingPlatform.DTOs;
using GastroErp.Application.Features.ReportingPlatform.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.ReportingPlatform;

[ApiVersion("1.0")]
[Tags("Reporting Platform — Dashboards")]
public class ReportingDashboardsController : BaseApiController
{
    private Guid CurrentUserId => HttpContext.RequestServices
        .GetRequiredService<GastroErp.Application.Common.Interfaces.ICurrentUser>().Id ?? Guid.Empty;

    [HttpGet(ApiRoutes.ReportingPlatform.Dashboards)]
    [HasPermission(Permissions.Reporting.View)]
    public async Task<IActionResult> GetDashboards([FromQuery] ReportingPlatformFilterDto filter)
        => HandleResult(await Mediator.Send(new GetDashboardsQuery(TenantId, CurrentUserId, filter)));

    [HttpGet($"{ApiRoutes.ReportingPlatform.Dashboards}/{{id:guid}}")]
    [HasPermission(Permissions.Reporting.View)]
    public async Task<IActionResult> GetById(Guid id)
        => HandleResult(await Mediator.Send(new GetDashboardQuery(TenantId, id)));

    [HttpPost(ApiRoutes.ReportingPlatform.Dashboards)]
    [HasPermission(Permissions.Reporting.Create)]
    public async Task<IActionResult> Create([FromBody] CreateDashboardDto dto)
        => HandleResult(await Mediator.Send(new CreateDashboardCommand(TenantId, CurrentUserId, dto)));

    [HttpPut($"{ApiRoutes.ReportingPlatform.Dashboards}/{{id:guid}}")]
    [HasPermission(Permissions.Reporting.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDashboardDto dto)
        => HandleResult(await Mediator.Send(new UpdateDashboardCommand(TenantId, id, dto)));

    [HttpDelete($"{ApiRoutes.ReportingPlatform.Dashboards}/{{id:guid}}")]
    [HasPermission(Permissions.Reporting.Delete)]
    public async Task<IActionResult> Delete(Guid id)
        => HandleResult(await Mediator.Send(new DeleteDashboardCommand(TenantId, id, CurrentUserId.ToString())));

    [HttpPost($"{ApiRoutes.ReportingPlatform.Dashboards}/{{id:guid}}/share")]
    [HasPermission(Permissions.Reporting.Edit)]
    public async Task<IActionResult> Share(Guid id, [FromBody] ShareDashboardDto dto)
        => HandleResult(await Mediator.Send(new ShareDashboardCommand(TenantId, id, dto)));

    [HttpPost($"{ApiRoutes.ReportingPlatform.Dashboards}/{{id:guid}}/favorite")]
    [HasPermission(Permissions.Reporting.View)]
    public async Task<IActionResult> SetFavorite(Guid id, [FromQuery] bool favorite = true)
        => HandleResult(await Mediator.Send(new SetDashboardFavoriteCommand(TenantId, id, favorite)));
}

[ApiVersion("1.0")]
[Tags("Reporting Platform — Reports")]
public class ReportingDefinitionsController : BaseApiController
{
    private Guid CurrentUserId => HttpContext.RequestServices
        .GetRequiredService<GastroErp.Application.Common.Interfaces.ICurrentUser>().Id ?? Guid.Empty;

    [HttpGet(ApiRoutes.ReportingPlatform.Reports)]
    [HasPermission(Permissions.Reporting.View)]
    public async Task<IActionResult> GetReports([FromQuery] ReportingPlatformFilterDto filter)
        => HandleResult(await Mediator.Send(new GetReportDefinitionsQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.ReportingPlatform.Reports}/{{id:guid}}")]
    [HasPermission(Permissions.Reporting.View)]
    public async Task<IActionResult> GetById(Guid id)
        => HandleResult(await Mediator.Send(new GetReportDefinitionQuery(TenantId, id)));

    [HttpPost(ApiRoutes.ReportingPlatform.Reports)]
    [HasPermission(Permissions.Reporting.Create)]
    public async Task<IActionResult> Create([FromBody] CreateReportDefinitionDto dto)
        => HandleResult(await Mediator.Send(new CreateReportDefinitionCommand(TenantId, dto)));

    [HttpPut($"{ApiRoutes.ReportingPlatform.Reports}/{{id:guid}}")]
    [HasPermission(Permissions.Reporting.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateReportDefinitionDto dto)
        => HandleResult(await Mediator.Send(new UpdateReportDefinitionCommand(TenantId, id, dto)));

    [HttpPost($"{ApiRoutes.ReportingPlatform.Reports}/{{id:guid}}/publish")]
    [HasPermission(Permissions.Reporting.Publish)]
    public async Task<IActionResult> Publish(Guid id)
        => HandleResult(await Mediator.Send(new PublishReportCommand(TenantId, id)));

    [HttpPost($"{ApiRoutes.ReportingPlatform.Reports}/execute")]
    [HasPermission(Permissions.Reporting.Execute)]
    public async Task<IActionResult> Execute([FromBody] ExecuteReportDto dto)
        => HandleResult(await Mediator.Send(new ExecuteReportCommand(TenantId, CurrentUserId, dto)));

    [HttpPost($"{ApiRoutes.ReportingPlatform.Reports}/preview")]
    [HasPermission(Permissions.Reporting.Execute)]
    public async Task<IActionResult> Preview([FromBody] ExecuteReportDto dto)
        => HandleResult(await Mediator.Send(new PreviewReportCommand(TenantId, CurrentUserId, dto)));

    [HttpPost($"{ApiRoutes.ReportingPlatform.Reports}/export")]
    [HasPermission(Permissions.Reporting.Export)]
    public async Task<IActionResult> Export([FromBody] PlatformExportRequestDto dto)
    {
        var result = await Mediator.Send(new ExportReportPlatformCommand(TenantId, dto));
        if (!result.IsSuccess) return HandleResult(result);
        return File(result.Data!.Content, result.Data.ContentType, result.Data.FileName);
    }

    [HttpGet($"{ApiRoutes.ReportingPlatform.Reports}/history")]
    [HasPermission(Permissions.Reporting.View)]
    public async Task<IActionResult> GetHistory([FromQuery] Guid? reportDefinitionId, [FromQuery] int take = 50)
        => HandleResult(await Mediator.Send(new GetReportHistoryQuery(TenantId, reportDefinitionId, take)));
}

[ApiVersion("1.0")]
[Tags("Reporting Platform — KPIs")]
public class ReportingKpisController : BaseApiController
{
    [HttpGet(ApiRoutes.ReportingPlatform.Kpis)]
    [HasPermission(Permissions.Reporting.Kpi)]
    public async Task<IActionResult> GetKpis([FromQuery] ReportingPlatformFilterDto filter)
        => HandleResult(await Mediator.Send(new GetKpiDefinitionsQuery(TenantId, filter)));

    [HttpPost(ApiRoutes.ReportingPlatform.Kpis)]
    [HasPermission(Permissions.Reporting.Create)]
    public async Task<IActionResult> Create([FromBody] CreateKpiDefinitionDto dto)
        => HandleResult(await Mediator.Send(new CreateKpiDefinitionCommand(TenantId, dto)));

    [HttpPut($"{ApiRoutes.ReportingPlatform.Kpis}/{{id:guid}}")]
    [HasPermission(Permissions.Reporting.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateKpiDefinitionDto dto)
        => HandleResult(await Mediator.Send(new UpdateKpiDefinitionCommand(TenantId, id, dto)));

    [HttpPost($"{ApiRoutes.ReportingPlatform.Kpis}/{{id:guid}}/calculate")]
    [HasPermission(Permissions.Reporting.Kpi)]
    public async Task<IActionResult> Calculate(Guid id, [FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate, [FromQuery] Guid? branchId)
        => HandleResult(await Mediator.Send(new CalculateKpiCommand(TenantId, id, fromDate, toDate, branchId)));

    [HttpGet($"{ApiRoutes.ReportingPlatform.Kpis}/{{id:guid}}/history")]
    [HasPermission(Permissions.Reporting.Kpi)]
    public async Task<IActionResult> GetHistory(Guid id, [FromQuery] int take = 90)
        => HandleResult(await Mediator.Send(new GetKpiHistoryQuery(TenantId, id, take)));
}

[ApiVersion("1.0")]
[Tags("Reporting Platform — Scheduled Reports")]
public class ReportingScheduledController : BaseApiController
{
    private Guid CurrentUserId => HttpContext.RequestServices
        .GetRequiredService<GastroErp.Application.Common.Interfaces.ICurrentUser>().Id ?? Guid.Empty;

    [HttpGet(ApiRoutes.ReportingPlatform.Scheduled)]
    [HasPermission(Permissions.Reporting.Schedule)]
    public async Task<IActionResult> GetScheduled()
        => HandleResult(await Mediator.Send(new GetScheduledReportsQuery(TenantId)));

    [HttpGet($"{ApiRoutes.ReportingPlatform.Scheduled}/{{id:guid}}")]
    [HasPermission(Permissions.Reporting.Schedule)]
    public async Task<IActionResult> GetById(Guid id)
        => HandleResult(await Mediator.Send(new GetScheduledReportQuery(TenantId, id)));

    [HttpPost(ApiRoutes.ReportingPlatform.Scheduled)]
    [HasPermission(Permissions.Reporting.Schedule)]
    public async Task<IActionResult> Create([FromBody] CreateScheduledReportDto dto)
        => HandleResult(await Mediator.Send(new CreateScheduledReportCommand(TenantId, dto)));

    [HttpPut($"{ApiRoutes.ReportingPlatform.Scheduled}/{{id:guid}}")]
    [HasPermission(Permissions.Reporting.Schedule)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateScheduledReportDto dto)
        => HandleResult(await Mediator.Send(new UpdateScheduledReportCommand(TenantId, id, dto)));

    [HttpDelete($"{ApiRoutes.ReportingPlatform.Scheduled}/{{id:guid}}")]
    [HasPermission(Permissions.Reporting.Delete)]
    public async Task<IActionResult> Delete(Guid id)
        => HandleResult(await Mediator.Send(new DeleteScheduledReportCommand(TenantId, id, CurrentUserId.ToString())));

    [HttpPost($"{ApiRoutes.ReportingPlatform.Scheduled}/{{id:guid}}/enable")]
    [HasPermission(Permissions.Reporting.Schedule)]
    public async Task<IActionResult> Enable(Guid id)
        => HandleResult(await Mediator.Send(new EnableScheduledReportCommand(TenantId, id)));

    [HttpPost($"{ApiRoutes.ReportingPlatform.Scheduled}/{{id:guid}}/disable")]
    [HasPermission(Permissions.Reporting.Schedule)]
    public async Task<IActionResult> Disable(Guid id)
        => HandleResult(await Mediator.Send(new DisableScheduledReportCommand(TenantId, id)));

    [HttpPost($"{ApiRoutes.ReportingPlatform.Scheduled}/{{id:guid}}/execute")]
    [HasPermission(Permissions.Reporting.Execute)]
    public async Task<IActionResult> ExecuteNow(Guid id)
        => HandleResult(await Mediator.Send(new ExecuteScheduledReportNowCommand(TenantId, id, CurrentUserId)));
}

[ApiVersion("1.0")]
[Tags("Reporting Platform — Charts & Power BI")]
public class ReportingAnalyticsController : BaseApiController
{
    [HttpPost(ApiRoutes.ReportingPlatform.Charts)]
    [HasPermission(Permissions.Reporting.View)]
    public async Task<IActionResult> BuildChart([FromBody] ChartRequestDto request)
        => HandleResult(await Mediator.Send(new BuildChartQuery(TenantId, request)));

    [HttpGet(ApiRoutes.ReportingPlatform.PowerBi)]
    [HasPermission(Permissions.Reporting.Admin)]
    public async Task<IActionResult> GetPowerBiConfig()
        => HandleResult(await Mediator.Send(new GetPowerBiConfigQuery(TenantId)));

    [HttpPost($"{ApiRoutes.ReportingPlatform.PowerBi}/refresh")]
    [HasPermission(Permissions.Reporting.Admin)]
    public async Task<IActionResult> RefreshDataset()
    {
        var service = HttpContext.RequestServices.GetRequiredService<GastroErp.Application.Features.ReportingPlatform.Services.IPowerBiIntegrationService>();
        await service.RefreshDatasetAsync(TenantId);
        return Ok();
    }

    [HttpGet($"{ApiRoutes.ReportingPlatform.PowerBi}/embed/{{reportId}}")]
    [HasPermission(Permissions.Reporting.View)]
    public async Task<IActionResult> GetEmbedToken(string reportId)
        => HandleResult(await Mediator.Send(new GetPowerBiEmbedTokenQuery(TenantId, reportId)));
}
