using Asp.Versioning;
using GastroErp.Application.Features.Menu.Commands;
using GastroErp.Application.Features.Menu.DTOs;
using GastroErp.Application.Features.Menu.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Menu;

/// <summary>
/// Branch Menu assignment and availability management
/// </summary>
[ApiVersion("1.0")]
public class BranchMenuController : BaseApiController
{
    [HttpGet($"{ApiRoutes.Menu.BranchMenus}/branch/{{branchId:guid}}")]
    [HasPermission(Permissions.Menu.View)]
    public async Task<IActionResult> GetByBranchId(Guid branchId, [FromQuery] bool? isActive)
    {
        return HandleResult(await Mediator.Send(new GetBranchMenusByBranchIdQuery(branchId, isActive)));
    }

    [HttpGet($"{ApiRoutes.Menu.BranchMenus}/menu/{{menuId:guid}}")]
    [HasPermission(Permissions.Menu.View)]
    public async Task<IActionResult> GetByMenuId(Guid menuId)
    {
        return HandleResult(await Mediator.Send(new GetBranchMenusByMenuIdQuery(menuId)));
    }

    [HttpPost(ApiRoutes.Menu.BranchMenus)]
    [HasPermission(Permissions.Menu.Create)]
    public async Task<IActionResult> Create([FromBody] CreateBranchMenuDto dto)
    {
        var finalDto = dto with { TenantId = TenantId };
        return HandleResult(await Mediator.Send(new CreateBranchMenuCommand(finalDto)));
    }

    [HttpPut($"{ApiRoutes.Menu.BranchMenus}/{{id:guid}}/price-level")]
    [HasPermission(Permissions.Menu.Update)]
    public async Task<IActionResult> SetPriceLevel(Guid id, [FromQuery] Guid? priceLevelId)
    {
        return HandleResult(await Mediator.Send(new SetBranchMenuPriceLevelCommand(id, priceLevelId)));
    }

    [HttpPut($"{ApiRoutes.Menu.BranchMenus}/{{id:guid}}/activate")]
    [HasPermission(Permissions.Menu.Update)]
    public async Task<IActionResult> Activate(Guid id)
    {
        return HandleResult(await Mediator.Send(new ActivateBranchMenuCommand(id)));
    }

    [HttpPut($"{ApiRoutes.Menu.BranchMenus}/{{id:guid}}/deactivate")]
    [HasPermission(Permissions.Menu.Update)]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeactivateBranchMenuCommand(id)));
    }

    [HttpPut($"{ApiRoutes.Menu.BranchMenus}/{{id:guid}}/availability")]
    [HasPermission(Permissions.Menu.Update)]
    public async Task<IActionResult> SetAvailability(Guid id, [FromBody] SetMenuAvailabilityDto dto)
    {
        return HandleResult(await Mediator.Send(new SetMenuAvailabilityCommand(id, dto)));
    }
}
