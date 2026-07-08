using Asp.Versioning;
using GastroErp.Application.Features.Menu.Commands;
using GastroErp.Application.Features.Menu.DTOs;
using GastroErp.Application.Features.Menu.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Menu;

/// <summary>
/// Menu management
/// </summary>
[ApiVersion("1.0")]
public class MenuController : BaseApiController
{
    [HttpGet(ApiRoutes.Menu.Menus)]
    [HasPermission(Permissions.Menu.View)]
    public async Task<IActionResult> GetMenus([FromQuery] PaginationQuery query)
    {
        return HandlePagedResult(await Mediator.Send(new GetMenusQuery(TenantId, null, null, null, query.Page, query.PageSize)));
    }

    [HttpGet($"{ApiRoutes.Menu.Menus}/{{id:guid}}")]
    [HasPermission(Permissions.Menu.View)]
    public async Task<IActionResult> GetMenuById(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetMenuByIdQuery(id)));
    }

    [HttpPost(ApiRoutes.Menu.Menus)]
    [HasPermission(Permissions.Menu.Create)]
    public async Task<IActionResult> CreateMenu([FromBody] CreateMenuDto dto)
    {
        var finalDto = dto with { TenantId = TenantId };
        return HandleResult(await Mediator.Send(new CreateMenuCommand(finalDto)));
    }

    [HttpPut($"{ApiRoutes.Menu.Menus}/{{id:guid}}")]
    [HasPermission(Permissions.Menu.Update)]
    public async Task<IActionResult> UpdateMenu(Guid id, [FromBody] UpdateMenuDto dto)
    {
        return HandleResult(await Mediator.Send(new UpdateMenuCommand(id, dto)));
    }

    [HttpPut($"{ApiRoutes.Menu.Menus}/{{id:guid}}/activate")]
    [HasPermission(Permissions.Menu.Update)]
    public async Task<IActionResult> ActivateMenu(Guid id)
    {
        return HandleResult(await Mediator.Send(new ActivateMenuCommand(id)));
    }

    [HttpPut($"{ApiRoutes.Menu.Menus}/{{id:guid}}/deactivate")]
    [HasPermission(Permissions.Menu.Update)]
    public async Task<IActionResult> DeactivateMenu(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeactivateMenuCommand(id)));
    }

    // ─── Menu Sections ────────────────────────────────────────────────────────

    [HttpPost($"{ApiRoutes.Menu.Menus}/{{menuId:guid}}/sections")]
    [HasPermission(Permissions.Menu.Update)]
    public async Task<IActionResult> AddSection(Guid menuId, [FromBody] AddMenuSectionDto dto)
    {
        return HandleResult(await Mediator.Send(new AddMenuSectionCommand(menuId, dto)));
    }

    [HttpPut($"{ApiRoutes.Menu.Menus}/{{menuId:guid}}/sections/{{sectionId:guid}}")]
    [HasPermission(Permissions.Menu.Update)]
    public async Task<IActionResult> UpdateSection(Guid menuId, Guid sectionId, [FromBody] UpdateMenuSectionDto dto)
    {
        return HandleResult(await Mediator.Send(new UpdateMenuSectionCommand(menuId, sectionId, dto)));
    }

    [HttpDelete($"{ApiRoutes.Menu.Menus}/{{menuId:guid}}/sections/{{sectionId:guid}}")]
    [HasPermission(Permissions.Menu.Update)]
    public async Task<IActionResult> RemoveSection(Guid menuId, Guid sectionId)
    {
        return HandleResult(await Mediator.Send(new RemoveMenuSectionCommand(menuId, sectionId)));
    }

    [HttpPut($"{ApiRoutes.Menu.Menus}/sections/{{sectionId:guid}}/deactivate")]
    [HasPermission(Permissions.Menu.Update)]
    public async Task<IActionResult> DeactivateSection(Guid sectionId)
    {
        return HandleResult(await Mediator.Send(new DeactivateMenuSectionCommand(sectionId)));
    }

    // ─── Menu Items ───────────────────────────────────────────────────────────

    [HttpPost($"{ApiRoutes.Menu.Menus}/sections/{{sectionId:guid}}/items")]
    [HasPermission(Permissions.Menu.Update)]
    public async Task<IActionResult> AddMenuItem(Guid sectionId, [FromBody] AddMenuItemDto dto)
    {
        return HandleResult(await Mediator.Send(new AddMenuItemCommand(sectionId, dto)));
    }

    [HttpDelete($"{ApiRoutes.Menu.Menus}/sections/{{sectionId:guid}}/items/{{productId:guid}}")]
    [HasPermission(Permissions.Menu.Update)]
    public async Task<IActionResult> RemoveMenuItem(Guid sectionId, Guid productId)
    {
        return HandleResult(await Mediator.Send(new RemoveMenuItemCommand(sectionId, productId)));
    }

    [HttpPut($"{ApiRoutes.Menu.Menus}/items/{{menuItemId:guid}}/override-price")]
    [HasPermission(Permissions.Menu.Update)]
    public async Task<IActionResult> SetMenuItemOverridePrice(Guid menuItemId, [FromQuery] decimal? price)
    {
        return HandleResult(await Mediator.Send(new SetMenuItemOverridePriceCommand(menuItemId, price)));
    }

    [HttpPut($"{ApiRoutes.Menu.Menus}/items/{{menuItemId:guid}}/out-of-stock")]
    [HasPermission(Permissions.Menu.Update)]
    public async Task<IActionResult> MarkMenuItemOutOfStock(Guid menuItemId)
    {
        return HandleResult(await Mediator.Send(new MarkMenuItemOutOfStockCommand(menuItemId)));
    }

    [HttpPut($"{ApiRoutes.Menu.Menus}/items/{{menuItemId:guid}}/in-stock")]
    [HasPermission(Permissions.Menu.Update)]
    public async Task<IActionResult> MarkMenuItemInStock(Guid menuItemId)
    {
        return HandleResult(await Mediator.Send(new MarkMenuItemInStockCommand(menuItemId)));
    }

    [HttpPut($"{ApiRoutes.Menu.Menus}/items/{{menuItemId:guid}}/hide")]
    [HasPermission(Permissions.Menu.Update)]
    public async Task<IActionResult> HideMenuItem(Guid menuItemId)
    {
        return HandleResult(await Mediator.Send(new HideMenuItemCommand(menuItemId)));
    }

    [HttpPut($"{ApiRoutes.Menu.Menus}/items/{{menuItemId:guid}}/show")]
    [HasPermission(Permissions.Menu.Update)]
    public async Task<IActionResult> ShowMenuItem(Guid menuItemId)
    {
        return HandleResult(await Mediator.Send(new ShowMenuItemCommand(menuItemId)));
    }
}
