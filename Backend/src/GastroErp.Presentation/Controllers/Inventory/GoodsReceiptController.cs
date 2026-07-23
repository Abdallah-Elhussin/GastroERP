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
/// Goods Receipt Note (سند الفحص والاستلام) — PO-based + exceptional direct receipt.
/// </summary>
[ApiVersion("1.0")]
public class GoodsReceiptController : BaseApiController
{
    [HttpGet(ApiRoutes.Inventory.GoodsReceipts)]
    [HasPermission(Permissions.Inventory.View)]
    public async Task<IActionResult> GetGoodsReceipts(
        [FromQuery] PaginationQuery query,
        [FromQuery] Guid? supplierId = null,
        [FromQuery] GoodsReceiptStatus? status = null,
        [FromQuery] string? search = null,
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null)
        => HandlePagedResult(await Mediator.Send(new GetGoodsReceiptsQuery(
            TenantId, supplierId, status, search, from, to, query.Page, query.PageSize)));

    [HttpGet($"{ApiRoutes.Inventory.GoodsReceipts}/{{id:guid}}")]
    [HasPermission(Permissions.Inventory.View)]
    public async Task<IActionResult> GetGoodsReceiptById(Guid id)
        => HandleResult(await Mediator.Send(new GetGoodsReceiptByIdQuery(id)));

    [HttpGet($"{ApiRoutes.Inventory.GoodsReceipts}/preview-from-po/{{purchaseOrderId:guid}}")]
    [HasPermission(Permissions.Inventory.View)]
    public async Task<IActionResult> PreviewFromPo(Guid purchaseOrderId)
        => HandleResult(await Mediator.Send(new PreviewGoodsReceiptFromPoQuery(TenantId, purchaseOrderId)));

    [HttpGet($"{ApiRoutes.Inventory.GoodsReceipts}/next-number")]
    [HasPermission(Permissions.Inventory.View)]
    public async Task<IActionResult> GetNextNumber()
        => HandleResult(await Mediator.Send(new GetNextGoodsReceiptNumberQuery(TenantId)));

    [HttpPost(ApiRoutes.Inventory.GoodsReceipts)]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> CreateGoodsReceipt([FromBody] CreateGoodsReceiptDto dto)
    {
        var payload = dto with { TenantId = TenantId };
        return HandleResult(await Mediator.Send(new CreateGoodsReceiptCommand(payload)));
    }

    [HttpPut($"{ApiRoutes.Inventory.GoodsReceipts}/{{id:guid}}")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGoodsReceiptDto dto)
        => HandleResult(await Mediator.Send(new UpdateGoodsReceiptCommand(id, dto)));

    [HttpPost($"{ApiRoutes.Inventory.GoodsReceipts}/{{id:guid}}/lines")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> AddLine(Guid id, [FromBody] AddGoodsReceiptLineDto dto)
        => HandleResult(await Mediator.Send(new AddGoodsReceiptLineCommand(id, dto)));

    [HttpPost($"{ApiRoutes.Inventory.GoodsReceipts}/{{id:guid}}/approve")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> Approve(Guid id)
        => HandleResult(await Mediator.Send(new ApproveGoodsReceiptCommand(id)));

    [HttpPost($"{ApiRoutes.Inventory.GoodsReceipts}/{{id:guid}}/confirm")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> Confirm(Guid id)
        => HandleResult(await Mediator.Send(new ConfirmGoodsReceiptCommand(id)));

    [HttpPost($"{ApiRoutes.Inventory.GoodsReceipts}/{{id:guid}}/post")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> Post(Guid id)
        => HandleResult(await Mediator.Send(new ConfirmGoodsReceiptCommand(id)));

    [HttpPost($"{ApiRoutes.Inventory.GoodsReceipts}/{{id:guid}}/unpost")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> Unpost(Guid id)
        => HandleResult(await Mediator.Send(new UnpostGoodsReceiptCommand(id)));

    [HttpPost($"{ApiRoutes.Inventory.GoodsReceipts}/{{id:guid}}/cancel")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> Cancel(Guid id)
        => HandleResult(await Mediator.Send(new CancelGoodsReceiptCommand(id)));
}
