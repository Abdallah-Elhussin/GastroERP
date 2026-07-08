using Asp.Versioning;
using GastroErp.Application.Features.Ai.Commands;
using GastroErp.Application.Features.Ai.DTOs;
using GastroErp.Application.Features.Ai.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Ai;

[ApiVersion("1.0")]
public class IntelligenceController : BaseApiController
{
    [HttpGet($"{ApiRoutes.Ai.Intelligence}/fraud")]
    [HasPermission(Permissions.Ai.FraudView)]
    public async Task<IActionResult> GetFraudAlerts([FromQuery] IntelligenceFilterDto filter)
        => HandleResult(await Mediator.Send(new GetFraudAlertsQuery(TenantId, filter)));

    [HttpPost($"{ApiRoutes.Ai.Intelligence}/fraud/refresh")]
    [HasPermission(Permissions.Ai.IntelligenceManage)]
    public async Task<IActionResult> RefreshFraud([FromBody] RefreshIntelligenceDto dto)
        => HandleResult(await Mediator.Send(new RefreshFraudAnalysisCommand(TenantId, dto)));

    [HttpGet($"{ApiRoutes.Ai.Intelligence}/segments")]
    [HasPermission(Permissions.Ai.SegmentsView)]
    public async Task<IActionResult> GetSegments([FromQuery] IntelligenceFilterDto filter)
        => HandleResult(await Mediator.Send(new GetCustomerSegmentsQuery(TenantId, filter)));

    [HttpPost($"{ApiRoutes.Ai.Intelligence}/segments/refresh")]
    [HasPermission(Permissions.Ai.IntelligenceManage)]
    public async Task<IActionResult> RefreshSegments([FromBody] RefreshIntelligenceDto dto)
        => HandleResult(await Mediator.Send(new RefreshSegmentsCommand(TenantId, dto)));

    [HttpGet($"{ApiRoutes.Ai.Intelligence}/churn")]
    [HasPermission(Permissions.Ai.ChurnView)]
    public async Task<IActionResult> GetChurn([FromQuery] IntelligenceFilterDto filter)
        => HandleResult(await Mediator.Send(new GetChurnPredictionsQuery(TenantId, filter)));

    [HttpPost($"{ApiRoutes.Ai.Intelligence}/churn/refresh")]
    [HasPermission(Permissions.Ai.IntelligenceManage)]
    public async Task<IActionResult> RefreshChurn([FromBody] RefreshIntelligenceDto dto)
        => HandleResult(await Mediator.Send(new RefreshChurnCommand(TenantId, dto)));

    [HttpGet($"{ApiRoutes.Ai.Intelligence}/recommendations")]
    [HasPermission(Permissions.Ai.ProductRecommendationView)]
    public async Task<IActionResult> GetRecommendations([FromQuery] IntelligenceFilterDto filter)
        => HandleResult(await Mediator.Send(new GetProductRecommendationsQuery(TenantId, filter)));

    [HttpPost($"{ApiRoutes.Ai.Intelligence}/recommendations/refresh")]
    [HasPermission(Permissions.Ai.IntelligenceManage)]
    public async Task<IActionResult> RefreshRecommendations([FromBody] RefreshIntelligenceDto dto)
        => HandleResult(await Mediator.Send(new RefreshIntelligenceRecommendationsCommand(TenantId, dto)));

    [HttpGet($"{ApiRoutes.Ai.Intelligence}/dashboard")]
    [HasPermission(Permissions.Ai.IntelligenceView)]
    public async Task<IActionResult> GetDashboard([FromQuery] Guid? branchId)
        => HandleResult(await Mediator.Send(new GetIntelligenceDashboardQuery(TenantId, branchId)));

    [HttpGet($"{ApiRoutes.Ai.Intelligence}/monitoring")]
    [HasPermission(Permissions.Ai.IntelligenceView)]
    public async Task<IActionResult> GetMonitoring()
        => HandleResult(await Mediator.Send(new GetIntelligenceMonitoringQuery()));
}
