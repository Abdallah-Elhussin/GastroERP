using Asp.Versioning;
using GastroErp.Application.Features.Inventory.Commands;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Application.Features.Inventory.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Inventory;

/// <summary>
/// Stock Transfer — Draft → Approve → Post/Ship (TRO) → Receive (TRI) | Cancel.
/// </summary>
[ApiVersion("1.0")]
public class StockTransferController : BaseApiController
{
    [HttpGet(ApiRoutes.Inventory.StockTransfers)]
    [HasPermission(Permissions.Stock.View)]
    public async Task<IActionResult> GetList(
        [FromQuery] PaginationQuery query,
        [FromQuery] byte? status = null,
        [FromQuery] Guid? sourceWarehouseId = null,
        [FromQuery] Guid? destinationWarehouseId = null,
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null)
        => HandlePagedResult(await Mediator.Send(new GetStockTransfersQuery(
            TenantId, sourceWarehouseId, destinationWarehouseId, status, query.Search, from, to, query.Page, query.PageSize)));

    [HttpGet($"{ApiRoutes.Inventory.StockTransfers}/{{id:guid}}")]
    [HasPermission(Permissions.Stock.View)]
    public async Task<IActionResult> GetById(Guid id)
        => HandleResult(await Mediator.Send(new GetStockTransferByIdQuery(id)));

    [HttpPost($"{ApiRoutes.Inventory.StockTransfers}/next-number")]
    [HasPermission(Permissions.Stock.Transfer)]
    public async Task<IActionResult> NextNumber()
        => HandleResult(await Mediator.Send(new GenerateStockTransferNumberCommand(TenantId)));

    [HttpPost(ApiRoutes.Inventory.StockTransfers)]
    [HasPermission(Permissions.Stock.Transfer)]
    public async Task<IActionResult> Create([FromBody] CreateStockTransferDto dto)
        => HandleResult(await Mediator.Send(new CreateStockTransferCommand(dto with { TenantId = TenantId })));

    [HttpPut($"{ApiRoutes.Inventory.StockTransfers}/{{id:guid}}")]
    [HasPermission(Permissions.Stock.Transfer)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStockTransferDto dto)
        => HandleResult(await Mediator.Send(new UpdateStockTransferCommand(id, dto with { TenantId = TenantId })));

    [HttpPost($"{ApiRoutes.Inventory.StockTransfers}/{{id:guid}}/lines")]
    [HasPermission(Permissions.Stock.Transfer)]
    public async Task<IActionResult> AddLine(Guid id, [FromBody] AddTransferLineDto dto)
        => HandleResult(await Mediator.Send(new AddTransferLineCommand(id, dto)));

    [HttpPost($"{ApiRoutes.Inventory.StockTransfers}/{{id:guid}}/approve")]
    [HasPermission(Permissions.Stock.Transfer)]
    public async Task<IActionResult> Approve(Guid id)
        => HandleResult(await Mediator.Send(new ApproveStockTransferCommand(id)));

    [HttpPost($"{ApiRoutes.Inventory.StockTransfers}/{{id:guid}}/unapprove")]
    [HasPermission(Permissions.Stock.Transfer)]
    public async Task<IActionResult> Unapprove(Guid id)
        => HandleResult(await Mediator.Send(new UnapproveStockTransferCommand(id)));

    [HttpPost($"{ApiRoutes.Inventory.StockTransfers}/{{id:guid}}/post")]
    [HasPermission(Permissions.Stock.Transfer)]
    public async Task<IActionResult> Post(Guid id)
        => HandleResult(await Mediator.Send(new ShipStockTransferCommand(id)));

    [HttpPost($"{ApiRoutes.Inventory.StockTransfers}/{{id:guid}}/ship")]
    [HasPermission(Permissions.Stock.Transfer)]
    public async Task<IActionResult> Ship(Guid id)
        => HandleResult(await Mediator.Send(new ShipStockTransferCommand(id)));

    [HttpPost($"{ApiRoutes.Inventory.StockTransfers}/{{id:guid}}/receive")]
    [HasPermission(Permissions.Stock.Transfer)]
    public async Task<IActionResult> Receive(Guid id)
        => HandleResult(await Mediator.Send(new CompleteStockTransferCommand(id)));

    [HttpPost($"{ApiRoutes.Inventory.StockTransfers}/{{id:guid}}/complete")]
    [HasPermission(Permissions.Stock.Transfer)]
    public async Task<IActionResult> Complete(Guid id)
        => HandleResult(await Mediator.Send(new CompleteStockTransferCommand(id)));

    [HttpPost($"{ApiRoutes.Inventory.StockTransfers}/{{id:guid}}/cancel")]
    [HasPermission(Permissions.Stock.Transfer)]
    public async Task<IActionResult> Cancel(Guid id)
        => HandleResult(await Mediator.Send(new CancelStockTransferCommand(id)));

    [HttpDelete($"{ApiRoutes.Inventory.StockTransfers}/{{id:guid}}")]
    [HasPermission(Permissions.Stock.Transfer)]
    public async Task<IActionResult> Delete(Guid id)
        => HandleResult(await Mediator.Send(new DeleteStockTransferCommand(id, TenantId)));
}
