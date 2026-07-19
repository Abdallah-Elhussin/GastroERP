using Asp.Versioning;
using GastroErp.Application.Features.Inventory.ItemTypes.Commands;
using GastroErp.Application.Features.Inventory.ItemTypes.Dtos;
using GastroErp.Application.Features.Inventory.ItemTypes.Queries;
using GastroErp.Domain.Enums;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Inventory;

/// <summary>
/// Restaurant inventory item types (operational behavior catalog).
/// </summary>
[ApiVersion("1.0")]
public class InventoryItemTypesController : BaseApiController
{
    [HttpGet(ApiRoutes.Inventory.ItemTypes)]
    [HasPermission(Permissions.Inventory.ItemTypes.View)]
    public async Task<IActionResult> GetItemTypes(
        [FromQuery] string? search = null,
        [FromQuery] InventoryItemTypeCategory? category = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] bool? isInventory = null,
        [FromQuery] bool? canSell = null,
        [FromQuery] bool? canPurchase = null,
        [FromQuery] bool? isRecipe = null,
        [FromQuery] bool? isProduction = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        return HandlePagedResult(await Mediator.Send(new GetInventoryItemTypesQuery(
            TenantId,
            search,
            category,
            isActive,
            isInventory,
            canSell,
            canPurchase,
            isRecipe,
            isProduction,
            sortBy,
            sortDesc,
            page,
            pageSize)));
    }

    [HttpGet($"{ApiRoutes.Inventory.ItemTypes}/{{id:guid}}")]
    [HasPermission(Permissions.Inventory.ItemTypes.View)]
    public async Task<IActionResult> GetById(Guid id)
        => HandleResult(await Mediator.Send(new GetInventoryItemTypeByIdQuery(id, TenantId)));

    [HttpPost(ApiRoutes.Inventory.ItemTypes)]
    [HasPermission(Permissions.Inventory.ItemTypes.Create)]
    public async Task<IActionResult> Create([FromBody] CreateInventoryItemTypeRequest request)
        => HandleResult(await Mediator.Send(new CreateInventoryItemTypeCommand(
            request with { TenantId = TenantId })));

    [HttpPut($"{ApiRoutes.Inventory.ItemTypes}/{{id:guid}}")]
    [HasPermission(Permissions.Inventory.ItemTypes.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateInventoryItemTypeRequest request)
        => HandleResult(await Mediator.Send(new UpdateInventoryItemTypeCommand(
            id,
            request with { TenantId = TenantId })));

    [HttpDelete($"{ApiRoutes.Inventory.ItemTypes}/{{id:guid}}")]
    [HasPermission(Permissions.Inventory.ItemTypes.Delete)]
    public async Task<IActionResult> Delete(Guid id)
        => HandleResult(await Mediator.Send(new DeleteInventoryItemTypeCommand(id, TenantId)));

    [HttpPost($"{ApiRoutes.Inventory.ItemTypes}/{{id:guid}}/activate")]
    [HasPermission(Permissions.Inventory.ItemTypes.Edit)]
    public async Task<IActionResult> Activate(Guid id)
        => HandleResult(await Mediator.Send(new ActivateInventoryItemTypeCommand(id, TenantId, true)));

    [HttpPost($"{ApiRoutes.Inventory.ItemTypes}/{{id:guid}}/deactivate")]
    [HasPermission(Permissions.Inventory.ItemTypes.Edit)]
    public async Task<IActionResult> Deactivate(Guid id)
        => HandleResult(await Mediator.Send(new ActivateInventoryItemTypeCommand(id, TenantId, false)));
}
