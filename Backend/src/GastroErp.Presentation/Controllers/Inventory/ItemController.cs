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

    [HttpGet($"{ApiRoutes.Inventory.Items}/{{id:guid}}/stock-by-warehouse")]
    [HasPermission(Permissions.Inventory.View)]
    public async Task<IActionResult> GetStockByWarehouse(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetInventoryItemStockByWarehouseQuery(id)));
    }

    [HttpGet($"{ApiRoutes.Inventory.Items}/{{id:guid}}/movements")]
    [HasPermission(Permissions.Inventory.View)]
    public async Task<IActionResult> GetMovements(Guid id, [FromQuery] PaginationQuery query)
    {
        return HandlePagedResult(await Mediator.Send(new GetInventoryItemMovementsQuery(id, query.Page, query.PageSize)));
    }

    [HttpGet($"{ApiRoutes.Inventory.Items}/{{id:guid}}/purchase-history")]
    [HasPermission(Permissions.Inventory.View)]
    public async Task<IActionResult> GetPurchaseHistory(Guid id, [FromQuery] PaginationQuery query)
    {
        return HandlePagedResult(await Mediator.Send(new GetInventoryItemPurchaseHistoryQuery(id, query.Page, query.PageSize)));
    }

    [HttpGet($"{ApiRoutes.Inventory.Items}/{{id:guid}}/sales-history")]
    [HasPermission(Permissions.Inventory.View)]
    public async Task<IActionResult> GetSalesHistory(Guid id, [FromQuery] PaginationQuery query)
    {
        return HandlePagedResult(await Mediator.Send(new GetInventoryItemSalesHistoryQuery(id, query.Page, query.PageSize)));
    }

    [HttpPost(ApiRoutes.Inventory.Items)]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> CreateItem([FromBody] CreateInventoryItemDto dto)
    {
        var payload = dto with { TenantId = TenantId };
        return HandleResult(await Mediator.Send(new CreateInventoryItemCommand(payload)));
    }

    [HttpPut($"{ApiRoutes.Inventory.Items}/{{id:guid}}")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> UpdateItem(Guid id, [FromBody] UpdateInventoryItemDto dto)
    {
        return HandleResult(await Mediator.Send(new UpdateInventoryItemCommand(id, dto)));
    }
}
