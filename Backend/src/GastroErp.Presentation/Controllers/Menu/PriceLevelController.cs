using Asp.Versioning;
using GastroErp.Application.Features.Menu.Commands;
using GastroErp.Application.Features.Menu.DTOs;
using GastroErp.Application.Features.Menu.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Menu;

/// <summary>
/// Price level management
/// </summary>
[ApiVersion("1.0")]
public class PriceLevelController : BaseApiController
{
    [HttpGet(ApiRoutes.Menu.PriceLevels)]
    [HasPermission(Permissions.Menu.View)]
    public async Task<IActionResult> GetPriceLevels([FromQuery] PaginationQuery query)
    {
        return HandlePagedResult(await Mediator.Send(new GetPriceLevelsQuery(TenantId, null, null, query.Page, query.PageSize)));
    }

    [HttpGet($"{ApiRoutes.Menu.PriceLevels}/{{id:guid}}")]
    [HasPermission(Permissions.Menu.View)]
    public async Task<IActionResult> GetPriceLevelById(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetPriceLevelByIdQuery(id)));
    }

    [HttpPost(ApiRoutes.Menu.PriceLevels)]
    [HasPermission(Permissions.Menu.Create)]
    public async Task<IActionResult> CreatePriceLevel([FromBody] CreatePriceLevelDto dto)
    {
        // Enforce TenantId
        var finalDto = dto with { TenantId = TenantId };
        return HandleResult(await Mediator.Send(new CreatePriceLevelCommand(finalDto)));
    }

    [HttpPut($"{ApiRoutes.Menu.PriceLevels}/{{id:guid}}")]
    [HasPermission(Permissions.Menu.Update)]
    public async Task<IActionResult> UpdatePriceLevel(Guid id, [FromBody] UpdatePriceLevelDto dto)
    {
        return HandleResult(await Mediator.Send(new UpdatePriceLevelCommand(id, dto)));
    }

    [HttpPut($"{ApiRoutes.Menu.PriceLevels}/{{id:guid}}/default")]
    [HasPermission(Permissions.Menu.Update)]
    public async Task<IActionResult> SetDefault(Guid id)
    {
        return HandleResult(await Mediator.Send(new SetPriceLevelAsDefaultCommand(id)));
    }

    [HttpPut($"{ApiRoutes.Menu.PriceLevels}/{{id:guid}}/deactivate")]
    [HasPermission(Permissions.Menu.Update)]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeactivatePriceLevelCommand(id)));
    }

    [HttpPut($"{ApiRoutes.Menu.PriceLevels}/{{id:guid}}/activate")]
    [HasPermission(Permissions.Menu.Update)]
    public async Task<IActionResult> Activate(Guid id)
    {
        return HandleResult(await Mediator.Send(new ActivatePriceLevelCommand(id)));
    }
}
