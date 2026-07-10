using Asp.Versioning;
using GastroErp.Application.Features.Sales.Commands;
using GastroErp.Application.Features.Sales.DTOs;
using GastroErp.Application.Features.Sales.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Sales;

[ApiVersion("1.0")]
public class KitchenController : BaseApiController
{
    [HttpGet($"{ApiRoutes.Sales.Kitchen}/board")]
    [HasPermission(Permissions.Kitchen.View)]
    public async Task<IActionResult> GetKdsBoard([FromQuery] Guid? branchId, [FromQuery] Guid? stationId)
        => HandleResult(await Mediator.Send(new GetKdsBoardQuery(TenantId, branchId, stationId)));

    [HttpPost($"{ApiRoutes.Sales.Kitchen}/dispatch")]
    [HasPermission(Permissions.Sales.Update)]
    public async Task<IActionResult> DispatchFromPos([FromBody] DispatchPosToKitchenDto dto)
    {
        var userId = HttpContext.RequestServices
            .GetRequiredService<GastroErp.Application.Common.Interfaces.ICurrentUser>().Id ?? Guid.Empty;
        return HandleResult(await Mediator.Send(new DispatchPosToKitchenCommand(TenantId, userId, dto)));
    }

    [HttpGet($"{ApiRoutes.Sales.Kitchen}/stations")]
    [HasPermission(Permissions.Kitchen.View)]
    public async Task<IActionResult> GetStations([FromQuery] Guid? branchId)
        => HandleResult(await Mediator.Send(new GetKitchenStationsQuery(TenantId, branchId)));

    [HttpPost($"{ApiRoutes.Sales.Kitchen}/stations")]
    [HasPermission(Permissions.Kitchen.Manage)]
    public async Task<IActionResult> CreateStation([FromBody] CreateKitchenStationDto dto)
        => HandleResult(await Mediator.Send(new CreateKitchenStationCommand(TenantId, dto)));

    [HttpPut($"{ApiRoutes.Sales.Kitchen}/stations/{{id:guid}}")]
    [HasPermission(Permissions.Kitchen.Manage)]
    public async Task<IActionResult> UpdateStation(Guid id, [FromBody] UpdateKitchenStationDto dto)
        => HandleResult(await Mediator.Send(new UpdateKitchenStationCommand(id, dto)));

    [HttpGet($"{ApiRoutes.Sales.Kitchen}/tickets")]
    [HasPermission(Permissions.Kitchen.View)]
    public async Task<IActionResult> GetTickets([FromQuery] KitchenTicketFilterDto filter)
        => HandlePagedResult(await Mediator.Send(new GetKitchenTicketsQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Sales.Kitchen}/tickets/{{id:guid}}")]
    [HasPermission(Permissions.Kitchen.View)]
    public async Task<IActionResult> GetTicketById(Guid id)
        => HandleResult(await Mediator.Send(new GetKitchenTicketByIdQuery(id)));

    [HttpGet($"{ApiRoutes.Sales.Kitchen}/stations/{{stationId:guid}}/tickets/active")]
    [HasPermission(Permissions.Kitchen.View)]
    public async Task<IActionResult> GetActiveTicketsByStation(Guid stationId)
        => HandleResult(await Mediator.Send(new GetActiveKitchenTicketsByStationQuery(stationId)));

    [HttpPatch($"{ApiRoutes.Sales.Kitchen}/tickets/{{id:guid}}/start")]
    [HasPermission(Permissions.Kitchen.Manage)]
    public async Task<IActionResult> StartTicket(Guid id)
        => HandleResult(await Mediator.Send(new StartKitchenTicketCommand(id)));

    [HttpPatch($"{ApiRoutes.Sales.Kitchen}/tickets/{{id:guid}}/ready")]
    [HasPermission(Permissions.Kitchen.Manage)]
    public async Task<IActionResult> MarkTicketReady(Guid id)
        => HandleResult(await Mediator.Send(new MarkKitchenTicketReadyCommand(id)));

    [HttpPatch($"{ApiRoutes.Sales.Kitchen}/tickets/{{id:guid}}/complete")]
    [HasPermission(Permissions.Kitchen.Manage)]
    public async Task<IActionResult> CompleteTicket(Guid id)
        => HandleResult(await Mediator.Send(new CompleteKitchenTicketCommand(id)));

    [HttpPatch($"{ApiRoutes.Sales.Kitchen}/items/{{id:guid}}/ready")]
    [HasPermission(Permissions.Kitchen.Manage)]
    public async Task<IActionResult> MarkItemReady(Guid id)
        => HandleResult(await Mediator.Send(new MarkKitchenItemReadyCommand(id)));
}
