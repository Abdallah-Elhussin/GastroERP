using Asp.Versioning;
using GastroErp.Application.Features.Delivery.Commands;
using GastroErp.Application.Features.Delivery.DTOs;
using GastroErp.Application.Features.Delivery.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Delivery;

[ApiVersion("1.0")]
public class DeliveryZoneController : BaseApiController
{
    [HttpGet(ApiRoutes.Sales.DeliveryZones)]
    [HasPermission(Permissions.DeliveryZone.View)]
    public async Task<IActionResult> GetZones([FromQuery] Guid? branchId)
        => HandleResult(await Mediator.Send(new GetDeliveryZonesQuery(TenantId, branchId)));

    [HttpGet($"{ApiRoutes.Sales.DeliveryZones}/{{id:guid}}")]
    [HasPermission(Permissions.DeliveryZone.View)]
    public async Task<IActionResult> GetZoneById(Guid id)
        => HandleResult(await Mediator.Send(new GetDeliveryZoneByIdQuery(id)));

    [HttpGet($"{ApiRoutes.Sales.DeliveryZones}/{{id:guid}}/fee")]
    [HasPermission(Permissions.DeliveryZone.View)]
    public async Task<IActionResult> GetZoneFee(Guid id, [FromQuery] decimal latitude, [FromQuery] decimal longitude)
        => HandleResult(await Mediator.Send(new GetDeliveryZoneFeeQuery(id, latitude, longitude)));

    [HttpPost(ApiRoutes.Sales.DeliveryZones)]
    [HasPermission(Permissions.DeliveryZone.Manage)]
    public async Task<IActionResult> CreateZone([FromBody] CreateDeliveryZoneDto dto)
        => HandleResult(await Mediator.Send(new CreateDeliveryZoneCommand(TenantId, dto)));

    [HttpPut($"{ApiRoutes.Sales.DeliveryZones}/{{id:guid}}")]
    [HasPermission(Permissions.DeliveryZone.Manage)]
    public async Task<IActionResult> UpdateZone(Guid id, [FromBody] UpdateDeliveryZoneDto dto)
        => HandleResult(await Mediator.Send(new UpdateDeliveryZoneCommand(id, dto)));
}

[ApiVersion("1.0")]
public class DeliveryDriverController : BaseApiController
{
    [HttpGet(ApiRoutes.Sales.DeliveryDrivers)]
    [HasPermission(Permissions.Driver.View)]
    public async Task<IActionResult> GetDrivers([FromQuery] Guid? branchId)
        => HandleResult(await Mediator.Send(new GetDeliveryDriversQuery(TenantId, branchId)));

    [HttpGet($"{ApiRoutes.Sales.DeliveryDrivers}/available")]
    [HasPermission(Permissions.Driver.View)]
    public async Task<IActionResult> GetAvailableDrivers([FromQuery] Guid branchId)
        => HandleResult(await Mediator.Send(new GetAvailableDriversQuery(TenantId, branchId)));

    [HttpPost(ApiRoutes.Sales.DeliveryDrivers)]
    [HasPermission(Permissions.Driver.Manage)]
    public async Task<IActionResult> CreateDriver([FromBody] CreateDeliveryDriverDto dto)
        => HandleResult(await Mediator.Send(new CreateDeliveryDriverCommand(TenantId, dto)));

    [HttpPut($"{ApiRoutes.Sales.DeliveryDrivers}/{{id:guid}}")]
    [HasPermission(Permissions.Driver.Manage)]
    public async Task<IActionResult> UpdateDriver(Guid id, [FromBody] UpdateDeliveryDriverDto dto)
        => HandleResult(await Mediator.Send(new UpdateDeliveryDriverCommand(id, dto)));

    [HttpPatch($"{ApiRoutes.Sales.DeliveryDrivers}/{{id:guid}}/status")]
    [HasPermission(Permissions.Driver.Manage)]
    public async Task<IActionResult> UpdateDriverStatus(Guid id, [FromBody] UpdateDriverStatusDto dto)
        => HandleResult(await Mediator.Send(new UpdateDriverStatusCommand(id, dto)));
}

[ApiVersion("1.0")]
public class DeliveryOrderController : BaseApiController
{
    private Guid CurrentUserId => HttpContext.RequestServices
        .GetRequiredService<GastroErp.Application.Common.Interfaces.ICurrentUser>().Id ?? Guid.Empty;

    [HttpGet(ApiRoutes.Sales.DeliveryOrders)]
    [HasPermission(Permissions.Delivery.View)]
    public async Task<IActionResult> GetOrders([FromQuery] DeliveryOrderFilterDto filter)
        => HandlePagedResult(await Mediator.Send(new GetDeliveryOrdersQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Sales.DeliveryOrders}/{{id:guid}}")]
    [HasPermission(Permissions.Delivery.View)]
    public async Task<IActionResult> GetOrderById(Guid id)
        => HandleResult(await Mediator.Send(new GetDeliveryOrderByIdQuery(id)));

    [HttpGet($"{ApiRoutes.Sales.DeliveryOrders}/sales-order/{{salesOrderId:guid}}")]
    [HasPermission(Permissions.Delivery.View)]
    public async Task<IActionResult> GetBySalesOrder(Guid salesOrderId)
        => HandleResult(await Mediator.Send(new GetDeliveryBySalesOrderQuery(salesOrderId)));

    [HttpGet($"{ApiRoutes.Sales.DeliveryOrders}/{{id:guid}}/tracking")]
    [HasPermission(Permissions.Delivery.View)]
    public async Task<IActionResult> GetTracking(Guid id)
        => HandleResult(await Mediator.Send(new GetDeliveryTrackingQuery(id)));

    [HttpGet($"{ApiRoutes.Sales.DeliveryOrders}/driver/{{driverId:guid}}/active")]
    [HasPermission(Permissions.Delivery.View)]
    public async Task<IActionResult> GetActiveByDriver(Guid driverId)
        => HandleResult(await Mediator.Send(new GetActiveDeliveriesByDriverQuery(driverId)));

    [HttpPost(ApiRoutes.Sales.DeliveryOrders)]
    [HasPermission(Permissions.Delivery.Manage)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateDeliveryOrderDto dto)
        => HandleResult(await Mediator.Send(new CreateDeliveryOrderCommand(TenantId, dto)));

    [HttpPost($"{ApiRoutes.Sales.DeliveryOrders}/{{id:guid}}/assign")]
    [HasPermission(Permissions.Delivery.Manage)]
    public async Task<IActionResult> Assign(Guid id, [FromBody] AssignDeliveryDto dto)
        => HandleResult(await Mediator.Send(new AssignDeliveryCommand(id, CurrentUserId, dto)));

    [HttpPost($"{ApiRoutes.Sales.DeliveryOrders}/{{id:guid}}/pickup")]
    [HasPermission(Permissions.Delivery.Manage)]
    public async Task<IActionResult> PickUp(Guid id, [FromBody] PickUpDeliveryDto dto)
        => HandleResult(await Mediator.Send(new PickUpDeliveryCommand(id, dto)));

    [HttpPost($"{ApiRoutes.Sales.DeliveryOrders}/{{id:guid}}/complete")]
    [HasPermission(Permissions.Delivery.Manage)]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteDeliveryDto dto)
        => HandleResult(await Mediator.Send(new CompleteDeliveryCommand(id, CurrentUserId, dto)));

    [HttpPost($"{ApiRoutes.Sales.DeliveryOrders}/{{id:guid}}/fail")]
    [HasPermission(Permissions.Delivery.Manage)]
    public async Task<IActionResult> Fail(Guid id, [FromBody] FailDeliveryDto dto)
        => HandleResult(await Mediator.Send(new FailDeliveryCommand(id, dto)));

    [HttpPost($"{ApiRoutes.Sales.DeliveryOrders}/{{id:guid}}/cancel")]
    [HasPermission(Permissions.Delivery.Manage)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelDeliveryDto dto)
        => HandleResult(await Mediator.Send(new CancelDeliveryCommand(id, dto)));
}
