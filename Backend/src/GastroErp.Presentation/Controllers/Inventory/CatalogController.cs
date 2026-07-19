using Asp.Versioning;
using GastroErp.Application.Features.Inventory.Commands;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Application.Features.Inventory.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Inventory;

/// <summary>
/// Inventory catalog master data (categories, units).
/// </summary>
[ApiVersion("1.0")]
public class CatalogController : BaseApiController
{
    // ─── Categories ───────────────────────────────────────────────────────────

    [HttpGet(ApiRoutes.Inventory.Categories)]
    [HasPermission(Permissions.Inventory.View)]
    public async Task<IActionResult> GetCategories([FromQuery] PaginationQuery query)
    {
        return HandlePagedResult(await Mediator.Send(new GetInventoryCategoriesQuery(TenantId, null, query.Page, query.PageSize)));
    }

    [HttpGet($"{ApiRoutes.Inventory.Categories}/{{id:guid}}")]
    [HasPermission(Permissions.Inventory.View)]
    public async Task<IActionResult> GetCategoryById(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetInventoryCategoryByIdQuery(id)));
    }

    [HttpPost(ApiRoutes.Inventory.Categories)]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateInventoryCategoryDto dto)
    {
        var payload = dto with { TenantId = TenantId };
        return HandleResult(await Mediator.Send(new CreateInventoryCategoryCommand(payload)));
    }

    [HttpPut($"{ApiRoutes.Inventory.Categories}/{{id:guid}}")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateInventoryCategoryDto dto)
    {
        return HandleResult(await Mediator.Send(new UpdateInventoryCategoryCommand(id, dto)));
    }

    [HttpPost($"{ApiRoutes.Inventory.Categories}/{{id:guid}}/activate")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> ActivateCategory(Guid id)
    {
        return HandleResult(await Mediator.Send(new ActivateInventoryCategoryCommand(id)));
    }

    [HttpPost($"{ApiRoutes.Inventory.Categories}/{{id:guid}}/deactivate")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> DeactivateCategory(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeactivateInventoryCategoryCommand(id)));
    }

    [HttpDelete($"{ApiRoutes.Inventory.Categories}/{{id:guid}}")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeleteInventoryCategoryCommand(id)));
    }

    // ─── Units ────────────────────────────────────────────────────────────────

    [HttpGet(ApiRoutes.Inventory.Units)]
    [HasPermission(Permissions.Inventory.View)]
    public async Task<IActionResult> GetUnits()
    {
        return HandleResult(await Mediator.Send(new GetInventoryUnitsQuery(TenantId, null)));
    }

    [HttpPost(ApiRoutes.Inventory.Units)]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> CreateUnit([FromBody] CreateInventoryUnitDto dto)
    {
        var payload = dto with { TenantId = TenantId };
        return HandleResult(await Mediator.Send(new CreateInventoryUnitCommand(payload)));
    }

    [HttpPut($"{ApiRoutes.Inventory.Units}/{{id:guid}}")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> UpdateUnit(Guid id, [FromBody] UpdateInventoryUnitDto dto)
    {
        return HandleResult(await Mediator.Send(new UpdateInventoryUnitCommand(id, dto)));
    }

    [HttpPost($"{ApiRoutes.Inventory.Units}/{{id:guid}}/activate")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> ActivateUnit(Guid id)
    {
        return HandleResult(await Mediator.Send(new ActivateInventoryUnitCommand(id)));
    }

    [HttpPost($"{ApiRoutes.Inventory.Units}/{{id:guid}}/deactivate")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> DeactivateUnit(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeactivateInventoryUnitCommand(id)));
    }

    [HttpDelete($"{ApiRoutes.Inventory.Units}/{{id:guid}}")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> DeleteUnit(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeleteInventoryUnitCommand(id)));
    }
}
