using Asp.Versioning;
using GastroErp.Application.Features.Inventory.Commands;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Application.Features.Inventory.Queries;
using GastroErp.Domain.Enums;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Inventory;

/// <summary>
/// Stock Count management
/// </summary>
[ApiVersion("1.0")]
public class StockCountController : BaseApiController
{
    [HttpGet(ApiRoutes.Inventory.StockCounts)]
    [HasPermission(Permissions.Inventory.View)]
    public async Task<IActionResult> GetStockCounts([FromQuery] Guid? warehouseId, [FromQuery] StockCountStatus? status, [FromQuery] PaginationQuery query)
    {
        return HandlePagedResult(await Mediator.Send(new GetStockCountsQuery(TenantId, warehouseId, status, query.Page, query.PageSize)));
    }

    [HttpGet($"{ApiRoutes.Inventory.StockCounts}/{{id:guid}}")]
    [HasPermission(Permissions.Inventory.View)]
    public async Task<IActionResult> GetStockCountById(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetStockCountByIdQuery(id)));
    }

    [HttpPost(ApiRoutes.Inventory.StockCounts)]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> CreateStockCount([FromBody] CreateStockCountDto dto)
    {
        return HandleResult(await Mediator.Send(new CreateStockCountCommand(TenantId, dto)));
    }

    [HttpPost($"{ApiRoutes.Inventory.StockCounts}/{{id:guid}}/lines")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> AddStockCountLine(Guid id, [FromBody] AddStockCountLineDto dto)
    {
        return HandleResult(await Mediator.Send(new AddStockCountLineCommand(id, dto)));
    }

    [HttpPost($"{ApiRoutes.Inventory.StockCounts}/{{id:guid}}/freeze")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> FreezeInventory(Guid id)
    {
        return HandleResult(await Mediator.Send(new FreezeInventoryCommand(id)));
    }

    [HttpPost($"{ApiRoutes.Inventory.StockCounts}/{{id:guid}}/approve")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> ApproveStockCount(Guid id)
    {
        return HandleResult(await Mediator.Send(new ApproveStockCountCommand(id)));
    }
}
