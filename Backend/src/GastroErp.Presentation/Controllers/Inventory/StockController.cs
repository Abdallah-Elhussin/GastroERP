using Asp.Versioning;
using GastroErp.Application.Features.Inventory.Commands;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Application.Features.Inventory.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Inventory;

/// <summary>
/// Stock transactions and tracking
/// </summary>
[ApiVersion("1.0")]
public class StockController : BaseApiController
{
    [HttpGet(ApiRoutes.Inventory.Stock)]
    [HasPermission(Permissions.Stock.View)]
    public async Task<IActionResult> GetStock([FromQuery] PaginationQuery query)
    {
        return HandlePagedResult(await Mediator.Send(new GetInventoryTransactionsQuery(TenantId, null, query.Page, query.PageSize)));
    }

    [HttpPost($"{ApiRoutes.Inventory.Stock}/transfer")]
    [HasPermission(Permissions.Stock.Transfer)]
    public async Task<IActionResult> StockTransfer([FromBody] CreateStockTransferDto dto)
    {
        return HandleResult(await Mediator.Send(new CreateStockTransferCommand(dto)));
    }

    [HttpPost($"{ApiRoutes.Inventory.Stock}/adjust")]
    [HasPermission(Permissions.Stock.Adjust)]
    public async Task<IActionResult> StockAdjustment([FromBody] CreateStockAdjustmentDto dto)
    {
        return HandleResult(await Mediator.Send(new CreateStockAdjustmentCommand(dto)));
    }

    [HttpPost($"{ApiRoutes.Inventory.Stock}/waste")]
    [HasPermission(Permissions.Stock.Waste)]
    public async Task<IActionResult> RecordWaste([FromBody] CreateWasteRecordDto dto)
    {
        return HandleResult(await Mediator.Send(new CreateWasteRecordCommand(dto)));
    }
}
