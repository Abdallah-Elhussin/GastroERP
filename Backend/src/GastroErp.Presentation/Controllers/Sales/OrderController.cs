using Asp.Versioning;
using GastroErp.Application.Features.Sales.Commands;
using GastroErp.Application.Features.Sales.DTOs;
using GastroErp.Application.Features.Sales.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Sales;

/// <summary>
/// POS Sales Order management
/// </summary>
[ApiVersion("1.0")]
public class OrderController : BaseApiController
{
    private Guid CurrentUserId => HttpContext.RequestServices
        .GetRequiredService<GastroErp.Application.Common.Interfaces.ICurrentUser>().Id ?? Guid.Empty;

    [HttpGet(ApiRoutes.Sales.Orders)]
    [HasPermission(Permissions.Sales.View)]
    public async Task<IActionResult> GetOrders([FromQuery] OrderFilterDto filter)
    {
        return HandlePagedResult(await Mediator.Send(new GetOrdersQuery(TenantId, filter)));
    }

    [HttpGet($"{ApiRoutes.Sales.Orders}/{{id:guid}}")]
    [HasPermission(Permissions.Sales.View)]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetOrderByIdQuery(id)));
    }

    [HttpGet($"{ApiRoutes.Sales.Orders}/{{id:guid}}/history")]
    [HasPermission(Permissions.Sales.View)]
    public async Task<IActionResult> GetOrderStatusHistory(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetOrderStatusHistoryQuery(id)));
    }

    [HttpGet($"{ApiRoutes.Sales.Orders}/branch/{{branchId:guid}}/active")]
    [HasPermission(Permissions.Sales.View)]
    public async Task<IActionResult> GetActiveOrdersByBranch(Guid branchId)
    {
        return HandleResult(await Mediator.Send(new GetActiveOrdersByBranchQuery(branchId)));
    }

    [HttpPost(ApiRoutes.Sales.Orders)]
    [HasPermission(Permissions.Sales.Create)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        return HandleResult(await Mediator.Send(new CreateOrderCommand(TenantId, CurrentUserId, dto)));
    }

    [HttpPost($"{ApiRoutes.Sales.Orders}/{{id:guid}}/items")]
    [HasPermission(Permissions.Sales.Update)]
    public async Task<IActionResult> AddOrderItem(Guid id, [FromBody] AddOrderItemDto dto)
    {
        return HandleResult(await Mediator.Send(new AddOrderItemCommand(id, dto)));
    }

    [HttpDelete($"{ApiRoutes.Sales.Orders}/{{id:guid}}/items/{{itemId:guid}}")]
    [HasPermission(Permissions.Sales.Update)]
    public async Task<IActionResult> RemoveOrderItem(Guid id, Guid itemId)
    {
        return HandleResult(await Mediator.Send(new RemoveOrderItemCommand(id, itemId)));
    }

    [HttpPost($"{ApiRoutes.Sales.Orders}/{{id:guid}}/items/{{itemId:guid}}/void")]
    [HasPermission(Permissions.Sales.VoidItem)]
    public async Task<IActionResult> VoidOrderItem(Guid id, Guid itemId, [FromBody] VoidOrderItemDto dto)
    {
        return HandleResult(await Mediator.Send(new VoidOrderItemCommand(id, itemId, dto)));
    }

    [HttpPost($"{ApiRoutes.Sales.Orders}/{{id:guid}}/discounts")]
    [HasPermission(Permissions.Sales.Update)]
    public async Task<IActionResult> ApplyOrderDiscount(Guid id, [FromBody] ApplyOrderDiscountDto dto)
    {
        return HandleResult(await Mediator.Send(new ApplyOrderDiscountCommand(id, dto)));
    }

    [HttpPost($"{ApiRoutes.Sales.Orders}/{{id:guid}}/submit")]
    [HasPermission(Permissions.Sales.Update)]
    public async Task<IActionResult> SubmitOrder(Guid id, [FromQuery] Guid deviceId)
    {
        return HandleResult(await Mediator.Send(new SubmitOrderCommand(id, CurrentUserId, deviceId)));
    }

    [HttpPost($"{ApiRoutes.Sales.Orders}/{{id:guid}}/confirm")]
    [HasPermission(Permissions.Sales.Update)]
    public async Task<IActionResult> ConfirmOrder(Guid id, [FromQuery] Guid deviceId)
    {
        return HandleResult(await Mediator.Send(new ConfirmOrderCommand(id, CurrentUserId, deviceId)));
    }

    [HttpPatch($"{ApiRoutes.Sales.Orders}/{{id:guid}}/status")]
    [HasPermission(Permissions.Sales.Update)]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromQuery] Guid deviceId, [FromBody] UpdateOrderStatusDto dto)
    {
        return HandleResult(await Mediator.Send(new UpdateOrderStatusCommand(id, CurrentUserId, deviceId, dto)));
    }

    [HttpPost($"{ApiRoutes.Sales.Orders}/{{id:guid}}/complete")]
    [HasPermission(Permissions.Sales.Complete)]
    public async Task<IActionResult> CompleteOrder(Guid id, [FromQuery] Guid deviceId)
    {
        return HandleResult(await Mediator.Send(new CompleteOrderCommand(id, CurrentUserId, deviceId)));
    }

    [HttpPost($"{ApiRoutes.Sales.Orders}/{{id:guid}}/cancel")]
    [HasPermission(Permissions.Sales.Cancel)]
    public async Task<IActionResult> CancelOrder(Guid id, [FromQuery] Guid deviceId, [FromBody] CancelOrderDto dto)
    {
        return HandleResult(await Mediator.Send(new CancelOrderCommand(id, CurrentUserId, deviceId, dto)));
    }

    [HttpPost($"{ApiRoutes.Sales.Orders}/{{id:guid}}/reopen")]
    [HasPermission(Permissions.Sales.Reopen)]
    public async Task<IActionResult> ReopenOrder(Guid id, [FromQuery] Guid deviceId, [FromBody] ReopenOrderDto dto)
    {
        return HandleResult(await Mediator.Send(new ReopenOrderCommand(id, CurrentUserId, deviceId, dto)));
    }
}
