using Asp.Versioning;
using GastroErp.Application.Features.Inventory.Commands;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Application.Features.Inventory.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Inventory;

/// <summary>
/// Stock ledger and operational documents (transfer / adjust / waste).
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

    // ─── Transfers ────────────────────────────────────────────────────────────

    [HttpGet(ApiRoutes.Inventory.Transfers)]
    [HasPermission(Permissions.Stock.View)]
    public async Task<IActionResult> GetTransfers([FromQuery] PaginationQuery query)
    {
        return HandlePagedResult(await Mediator.Send(new GetStockTransfersQuery(
            TenantId,
            SourceWarehouseId: null,
            DestinationWarehouseId: null,
            PageNumber: query.Page,
            PageSize: query.PageSize)));
    }

    [HttpPost(ApiRoutes.Inventory.Transfers)]
    [HasPermission(Permissions.Stock.Transfer)]
    public async Task<IActionResult> CreateTransfer([FromBody] CreateStockTransferDto dto)
    {
        var payload = dto with { TenantId = TenantId };
        return HandleResult(await Mediator.Send(new CreateStockTransferCommand(payload)));
    }

    [HttpPost($"{ApiRoutes.Inventory.Transfers}/{{id:guid}}/lines")]
    [HasPermission(Permissions.Stock.Transfer)]
    public async Task<IActionResult> AddTransferLine(Guid id, [FromBody] AddTransferLineDto dto)
    {
        return HandleResult(await Mediator.Send(new AddTransferLineCommand(id, dto)));
    }

    [HttpPost($"{ApiRoutes.Inventory.Transfers}/{{id:guid}}/ship")]
    [HasPermission(Permissions.Stock.Transfer)]
    public async Task<IActionResult> ShipTransfer(Guid id)
    {
        return HandleResult(await Mediator.Send(new ShipStockTransferCommand(id)));
    }

    [HttpPost($"{ApiRoutes.Inventory.Transfers}/{{id:guid}}/complete")]
    [HasPermission(Permissions.Stock.Transfer)]
    public async Task<IActionResult> CompleteTransfer(Guid id)
    {
        return HandleResult(await Mediator.Send(new CompleteStockTransferCommand(id)));
    }

    [HttpPost($"{ApiRoutes.Inventory.Transfers}/{{id:guid}}/cancel")]
    [HasPermission(Permissions.Stock.Transfer)]
    public async Task<IActionResult> CancelTransfer(Guid id)
    {
        return HandleResult(await Mediator.Send(new CancelStockTransferCommand(id)));
    }

    // Legacy create aliases (keep for compatibility)
    [HttpPost($"{ApiRoutes.Inventory.Stock}/transfer")]
    [HasPermission(Permissions.Stock.Transfer)]
    public async Task<IActionResult> StockTransfer([FromBody] CreateStockTransferDto dto)
    {
        var payload = dto with { TenantId = TenantId };
        return HandleResult(await Mediator.Send(new CreateStockTransferCommand(payload)));
    }

    // ─── Adjustments ──────────────────────────────────────────────────────────

    [HttpGet(ApiRoutes.Inventory.Adjustments)]
    [HasPermission(Permissions.Stock.View)]
    public async Task<IActionResult> GetAdjustments([FromQuery] PaginationQuery query)
    {
        return HandlePagedResult(await Mediator.Send(new GetStockAdjustmentsQuery(TenantId, null, query.Page, query.PageSize)));
    }

    [HttpPost(ApiRoutes.Inventory.Adjustments)]
    [HasPermission(Permissions.Stock.Adjust)]
    public async Task<IActionResult> CreateAdjustment([FromBody] CreateStockAdjustmentDto dto)
    {
        var payload = dto with { TenantId = TenantId };
        return HandleResult(await Mediator.Send(new CreateStockAdjustmentCommand(payload)));
    }

    [HttpPost($"{ApiRoutes.Inventory.Adjustments}/{{id:guid}}/confirm")]
    [HasPermission(Permissions.Stock.Adjust)]
    public async Task<IActionResult> ConfirmAdjustment(Guid id)
    {
        return HandleResult(await Mediator.Send(new ConfirmStockAdjustmentCommand(id)));
    }

    [HttpPost($"{ApiRoutes.Inventory.Stock}/adjust")]
    [HasPermission(Permissions.Stock.Adjust)]
    public async Task<IActionResult> StockAdjustment([FromBody] CreateStockAdjustmentDto dto)
    {
        var payload = dto with { TenantId = TenantId };
        return HandleResult(await Mediator.Send(new CreateStockAdjustmentCommand(payload)));
    }

    // ─── Waste ────────────────────────────────────────────────────────────────

    [HttpGet(ApiRoutes.Inventory.Waste)]
    [HasPermission(Permissions.Stock.View)]
    public async Task<IActionResult> GetWaste([FromQuery] PaginationQuery query)
    {
        return HandlePagedResult(await Mediator.Send(new GetWasteRecordsQuery(TenantId, null, query.Page, query.PageSize)));
    }

    [HttpPost(ApiRoutes.Inventory.Waste)]
    [HasPermission(Permissions.Stock.Waste)]
    public async Task<IActionResult> CreateWaste([FromBody] CreateWasteRecordDto dto)
    {
        var payload = dto with { TenantId = TenantId };
        return HandleResult(await Mediator.Send(new CreateWasteRecordCommand(payload)));
    }

    [HttpPost($"{ApiRoutes.Inventory.Waste}/{{id:guid}}/confirm")]
    [HasPermission(Permissions.Stock.Waste)]
    public async Task<IActionResult> ConfirmWaste(Guid id)
    {
        return HandleResult(await Mediator.Send(new ConfirmWasteRecordCommand(id)));
    }

    [HttpPost($"{ApiRoutes.Inventory.Stock}/waste")]
    [HasPermission(Permissions.Stock.Waste)]
    public async Task<IActionResult> RecordWaste([FromBody] CreateWasteRecordDto dto)
    {
        var payload = dto with { TenantId = TenantId };
        return HandleResult(await Mediator.Send(new CreateWasteRecordCommand(payload)));
    }
}
