using Asp.Versioning;
using GastroErp.Application.Features.Sales.Commands;
using GastroErp.Application.Features.Sales.DTOs;
using GastroErp.Application.Features.Sales.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Sales;

[ApiVersion("1.0")]
public class TableReservationController : BaseApiController
{
    private Guid CurrentUserId => HttpContext.RequestServices
        .GetRequiredService<GastroErp.Application.Common.Interfaces.ICurrentUser>().Id ?? Guid.Empty;

    [HttpGet(ApiRoutes.Sales.TableReservations)]
    [HasPermission(Permissions.Reservation.View)]
    public async Task<IActionResult> GetReservations([FromQuery] TableReservationFilterDto filter)
        => HandlePagedResult(await Mediator.Send(new GetTableReservationsQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Sales.TableReservations}/{{id:guid}}")]
    [HasPermission(Permissions.Reservation.View)]
    public async Task<IActionResult> GetReservationById(Guid id)
        => HandleResult(await Mediator.Send(new GetTableReservationByIdQuery(id)));

    [HttpPost(ApiRoutes.Sales.TableReservations)]
    [HasPermission(Permissions.Reservation.Create)]
    public async Task<IActionResult> CreateReservation([FromBody] CreateTableReservationDto dto)
        => HandleResult(await Mediator.Send(new CreateTableReservationCommand(TenantId, dto)));

    [HttpPost($"{ApiRoutes.Sales.TableReservations}/{{id:guid}}/confirm")]
    [HasPermission(Permissions.Reservation.Manage)]
    public async Task<IActionResult> ConfirmReservation(Guid id)
        => HandleResult(await Mediator.Send(new ConfirmTableReservationCommand(id)));

    [HttpPost($"{ApiRoutes.Sales.TableReservations}/{{id:guid}}/seat")]
    [HasPermission(Permissions.Reservation.Manage)]
    public async Task<IActionResult> SeatReservation(Guid id, [FromBody] SeatReservationDto dto)
        => HandleResult(await Mediator.Send(new SeatTableReservationCommand(id, TenantId, CurrentUserId, dto)));

    [HttpPost($"{ApiRoutes.Sales.TableReservations}/{{id:guid}}/cancel")]
    [HasPermission(Permissions.Reservation.Manage)]
    public async Task<IActionResult> CancelReservation(Guid id, [FromBody] CancelTableReservationDto dto)
        => HandleResult(await Mediator.Send(new CancelTableReservationCommand(id, dto)));
}
