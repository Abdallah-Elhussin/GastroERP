using Asp.Versioning;
using GastroErp.Application.Features.Inventory.Commands;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Application.Features.Inventory.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Inventory;

/// <summary>
/// Purchase Orders &amp; Invoices
/// </summary>
[ApiVersion("1.0")]
public class PurchaseController : BaseApiController
{
    [HttpGet(ApiRoutes.Inventory.Purchases)]
    [HasPermission(Permissions.Purchase.View)]
    public async Task<IActionResult> GetPurchases([FromQuery] PaginationQuery query)
    {
        return HandlePagedResult(await Mediator.Send(new GetPurchaseOrdersQuery(TenantId, null, null, query.Page, query.PageSize)));
    }

    [HttpGet($"{ApiRoutes.Inventory.Purchases}/{{id:guid}}")]
    [HasPermission(Permissions.Purchase.View)]
    public async Task<IActionResult> GetPurchaseById(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetPurchaseOrderByIdQuery(id)));
    }

    [HttpPost(ApiRoutes.Inventory.Purchases)]
    [HasPermission(Permissions.Purchase.Create)]
    public async Task<IActionResult> CreatePurchase([FromBody] CreatePurchaseOrderDto dto)
    {
        return HandleResult(await Mediator.Send(new CreatePurchaseOrderCommand(dto)));
    }

    [HttpPost($"{ApiRoutes.Inventory.Purchases}/{{id:guid}}/approve")]
    [HasPermission(Permissions.Purchase.Approve)]
    public async Task<IActionResult> ApprovePurchase(Guid id)
    {
        return HandleResult(await Mediator.Send(new ApprovePurchaseOrderCommand(id)));
    }

    [HttpPost($"{ApiRoutes.Inventory.Purchases}/{{id:guid}}/cancel")]
    [HasPermission(Permissions.Purchase.Cancel)]
    public async Task<IActionResult> CancelPurchase(Guid id)
    {
        return HandleResult(await Mediator.Send(new CancelPurchaseOrderCommand(id)));
    }

    [HttpPost($"{ApiRoutes.Inventory.Purchases}/{{id:guid}}/reject")]
    [HasPermission(Permissions.Purchase.Approve)]
    public async Task<IActionResult> RejectPurchase(Guid id)
    {
        return HandleResult(await Mediator.Send(new RejectPurchaseOrderCommand(id)));
    }

    [HttpPost($"{ApiRoutes.Inventory.Purchases}/{{id:guid}}/close")]
    [HasPermission(Permissions.Purchase.Approve)]
    public async Task<IActionResult> ClosePurchase(Guid id)
    {
        return HandleResult(await Mediator.Send(new ClosePurchaseOrderCommand(id)));
    }
}
