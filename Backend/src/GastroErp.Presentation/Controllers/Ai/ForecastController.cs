using Asp.Versioning;
using GastroErp.Application.Features.Ai.Commands;
using GastroErp.Application.Features.Ai.DTOs;
using GastroErp.Application.Features.Ai.Queries;
using GastroErp.Domain.Enums;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Ai;

[ApiVersion("1.0")]
public class ForecastController : BaseApiController
{
    [HttpGet($"{ApiRoutes.Ai.Forecast}/demand")]
    [HasPermission(Permissions.Ai.ForecastView)]
    public async Task<IActionResult> GetDemandForecast([FromQuery] ForecastFilterDto filter)
        => HandleResult(await Mediator.Send(new GetDemandForecastQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Ai.Forecast}/sales")]
    [HasPermission(Permissions.Ai.ForecastView)]
    public async Task<IActionResult> GetSalesForecast([FromQuery] ForecastFilterDto filter)
        => HandleResult(await Mediator.Send(new GetSalesForecastQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Ai.Forecast}/inventory")]
    [HasPermission(Permissions.Ai.ForecastView)]
    public async Task<IActionResult> GetInventoryForecast([FromQuery] ForecastFilterDto filter)
        => HandleResult(await Mediator.Send(new GetInventoryForecastQuery(TenantId, filter)));

    [HttpPost($"{ApiRoutes.Ai.Forecast}/refresh")]
    [HasPermission(Permissions.Ai.DataManage)]
    public async Task<IActionResult> RefreshForecasts([FromBody] RefreshForecastsDto dto)
        => HandleResult(await Mediator.Send(new RefreshForecastsCommand(TenantId, dto)));

    [HttpGet(ApiRoutes.Ai.Predictions)]
    [HasPermission(Permissions.Ai.ForecastView)]
    public async Task<IActionResult> GetPredictionRuns([FromQuery] ForecastType? type, [FromQuery] int take = 50)
        => HandleResult(await Mediator.Send(new GetPredictionRunsQuery(TenantId, type, take)));
}
