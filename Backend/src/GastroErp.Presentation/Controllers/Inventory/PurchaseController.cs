using Asp.Versioning;
using GastroErp.Application.Features.Inventory.Commands;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Application.Features.Inventory.Queries;
using GastroErp.Domain.Enums;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Inventory;

/// <summary>أوامر الشراء — لا تأثير محاسبي أو مخزني.</summary>
[ApiVersion("1.0")]
public class PurchaseController : BaseApiController
{
    [HttpGet($"{ApiRoutes.Inventory.Purchases}/dashboard")]
    [HasPermission(Permissions.Purchase.View)]
    public async Task<IActionResult> GetDashboard()
        => HandleResult(await Mediator.Send(new GetPurchaseOrderDashboardQuery(TenantId)));

    [HttpGet(ApiRoutes.Inventory.Purchases)]
    [HasPermission(Permissions.Purchase.View)]
    public async Task<IActionResult> GetPurchases(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null,
        [FromQuery] PurchaseOrderStatus? status = null,
        [FromQuery] Guid? supplierId = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null)
        => HandlePagedResult(await Mediator.Send(new GetPurchaseOrdersQuery(
            TenantId, supplierId, status, warehouseId, search, from, to, page, pageSize)));

    [HttpGet($"{ApiRoutes.Inventory.Purchases}/{{id:guid}}")]
    [HasPermission(Permissions.Purchase.View)]
    public async Task<IActionResult> GetPurchaseById(Guid id)
        => HandleResult(await Mediator.Send(new GetPurchaseOrderByIdQuery(id)));

    [HttpPost(ApiRoutes.Inventory.Purchases)]
    [HasPermission(Permissions.Purchase.Create)]
    public async Task<IActionResult> CreatePurchase([FromBody] CreatePurchaseOrderDto dto)
        => HandleResult(await Mediator.Send(new CreatePurchaseOrderCommand(dto)));

    [HttpPut($"{ApiRoutes.Inventory.Purchases}/{{id:guid}}")]
    [HasPermission(Permissions.Purchase.Create)]
    public async Task<IActionResult> UpdatePurchase(Guid id, [FromBody] UpdatePurchaseOrderDto dto)
        => HandleResult(await Mediator.Send(new UpdatePurchaseOrderCommand(id, dto)));

    [HttpDelete($"{ApiRoutes.Inventory.Purchases}/{{id:guid}}")]
    [HasPermission(Permissions.Purchase.Cancel)]
    public async Task<IActionResult> DeletePurchase(Guid id)
        => HandleResult(await Mediator.Send(new DeletePurchaseOrderCommand(id)));

    [HttpPost($"{ApiRoutes.Inventory.Purchases}/{{id:guid}}/copy")]
    [HasPermission(Permissions.Purchase.Create)]
    public async Task<IActionResult> CopyPurchase(Guid id)
        => HandleResult(await Mediator.Send(new CopyPurchaseOrderCommand(id)));

    [HttpPost($"{ApiRoutes.Inventory.Purchases}/{{id:guid}}/lines")]
    [HasPermission(Permissions.Purchase.Create)]
    public async Task<IActionResult> AddLine(Guid id, [FromBody] AddPurchaseOrderLineDto dto)
        => HandleResult(await Mediator.Send(new AddPurchaseOrderLineCommand(id, dto)));

    [HttpDelete($"{ApiRoutes.Inventory.Purchases}/{{id:guid}}/lines/{{lineId:guid}}")]
    [HasPermission(Permissions.Purchase.Create)]
    public async Task<IActionResult> RemoveLine(Guid id, Guid lineId)
        => HandleResult(await Mediator.Send(new RemovePurchaseOrderLineCommand(id, lineId)));

    [HttpPost($"{ApiRoutes.Inventory.Purchases}/{{id:guid}}/approve")]
    [HasPermission(Permissions.Purchase.Approve)]
    public async Task<IActionResult> ApprovePurchase(Guid id)
        => HandleResult(await Mediator.Send(new ApprovePurchaseOrderCommand(id)));

    [HttpPost($"{ApiRoutes.Inventory.Purchases}/{{id:guid}}/cancel")]
    [HasPermission(Permissions.Purchase.Cancel)]
    public async Task<IActionResult> CancelPurchase(Guid id)
        => HandleResult(await Mediator.Send(new CancelPurchaseOrderCommand(id)));

    [HttpPost($"{ApiRoutes.Inventory.Purchases}/{{id:guid}}/reject")]
    [HasPermission(Permissions.Purchase.Approve)]
    public async Task<IActionResult> RejectPurchase(Guid id)
        => HandleResult(await Mediator.Send(new RejectPurchaseOrderCommand(id)));

    [HttpPost($"{ApiRoutes.Inventory.Purchases}/{{id:guid}}/close")]
    [HasPermission(Permissions.Purchase.Approve)]
    public async Task<IActionResult> ClosePurchase(Guid id)
        => HandleResult(await Mediator.Send(new ClosePurchaseOrderCommand(id)));

    [HttpPost($"{ApiRoutes.Inventory.Purchases}/{{id:guid}}/send")]
    [HasPermission(Permissions.Purchase.Approve)]
    public async Task<IActionResult> SendPurchase(Guid id)
        => HandleResult(await Mediator.Send(new SendPurchaseOrderToSupplierCommand(id)));
}
