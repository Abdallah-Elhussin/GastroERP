using Asp.Versioning;
using GastroErp.Application.Features.Inventory.Commands;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Application.Features.Inventory.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Inventory;

/// <summary>
/// Inventory Item management
/// </summary>
[ApiVersion("1.0")]
public class ItemController : BaseApiController
{
    [HttpGet(ApiRoutes.Inventory.Items)]
    [HasPermission(Permissions.Inventory.View)]
    public async Task<IActionResult> GetItems([FromQuery] PaginationQuery query)
    {
        return HandlePagedResult(await Mediator.Send(new GetInventoryItemsQuery(TenantId, null, null, query.Search, query.Page, query.PageSize)));
    }

    [HttpGet($"{ApiRoutes.Inventory.Items}/{{id:guid}}")]
    [HasPermission(Permissions.Inventory.View)]
    public async Task<IActionResult> GetItemById(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetInventoryItemByIdQuery(id)));
    }

    [HttpPost(ApiRoutes.Inventory.Items)]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> CreateItem([FromBody] CreateInventoryItemDto dto)
    {
        return HandleResult(await Mediator.Send(new CreateInventoryItemCommand(dto)));
    }

    [HttpPut($"{ApiRoutes.Inventory.Items}/{{id:guid}}")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> UpdateItem(Guid id, [FromBody] UpdateInventoryItemDto dto)
    {
        return HandleResult(await Mediator.Send(new UpdateInventoryItemCommand(id, dto)));
    }
}
