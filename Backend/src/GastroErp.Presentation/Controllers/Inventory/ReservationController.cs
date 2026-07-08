using Asp.Versioning;
using GastroErp.Application.Features.Inventory.Commands;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Application.Features.Inventory.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Inventory;

/// <summary>
/// Inventory Reservation management
/// </summary>
[ApiVersion("1.0")]
public class ReservationController : BaseApiController
{
    [HttpGet(ApiRoutes.Inventory.Reservations)]
    [HasPermission(Permissions.Inventory.View)]
    public async Task<IActionResult> GetReservations([FromQuery] Guid? warehouseId, [FromQuery] Guid? inventoryItemId, [FromQuery] PaginationQuery query)
    {
        return HandlePagedResult(await Mediator.Send(new GetInventoryReservationsQuery(TenantId, warehouseId, inventoryItemId, query.Page, query.PageSize)));
    }

    [HttpPost(ApiRoutes.Inventory.Reservations)]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> ReserveStock([FromBody] ReserveStockDto dto)
    {
        return HandleResult(await Mediator.Send(new ReserveStockCommand(TenantId, dto)));
    }

    [HttpPost($"{ApiRoutes.Inventory.Reservations}/{{id:guid}}/release")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> ReleaseStock(Guid id)
    {
        return HandleResult(await Mediator.Send(new ReleaseStockCommand(id)));
    }

    [HttpPost($"{ApiRoutes.Inventory.Reservations}/{{id:guid}}/expire")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> ExpireStock(Guid id)
    {
        return HandleResult(await Mediator.Send(new ExpireStockReservationCommand(id)));
    }
}
