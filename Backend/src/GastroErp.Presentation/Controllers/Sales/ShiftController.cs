using Asp.Versioning;
using GastroErp.Application.Features.Sales.Commands;
using GastroErp.Application.Features.Sales.DTOs;
using GastroErp.Application.Features.Sales.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Sales;

[ApiVersion("1.0")]
public class ShiftController : BaseApiController
{
    private Guid CurrentUserId => HttpContext.RequestServices
        .GetRequiredService<GastroErp.Application.Common.Interfaces.ICurrentUser>().Id ?? Guid.Empty;

    [HttpGet(ApiRoutes.Sales.Shifts)]
    [HasPermission(Permissions.Shift.View)]
    public async Task<IActionResult> GetShifts([FromQuery] ShiftFilterDto filter)
        => HandlePagedResult(await Mediator.Send(new GetShiftsQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Sales.Shifts}/{{id:guid}}")]
    [HasPermission(Permissions.Shift.View)]
    public async Task<IActionResult> GetShiftById(Guid id)
        => HandleResult(await Mediator.Send(new GetShiftByIdQuery(id)));

    [HttpGet($"{ApiRoutes.Sales.Shifts}/current")]
    [HasPermission(Permissions.Shift.View)]
    public async Task<IActionResult> GetCurrentShift([FromQuery] Guid deviceId)
        => HandleResult(await Mediator.Send(new GetCurrentShiftQuery(CurrentUserId, deviceId)));

    [HttpPost($"{ApiRoutes.Sales.Shifts}/open")]
    [HasPermission(Permissions.Shift.Open)]
    public async Task<IActionResult> OpenShift([FromBody] OpenShiftDto dto)
        => HandleResult(await Mediator.Send(new OpenShiftCommand(TenantId, CurrentUserId, dto)));

    [HttpPost($"{ApiRoutes.Sales.Shifts}/{{id:guid}}/close")]
    [HasPermission(Permissions.Shift.Close)]
    public async Task<IActionResult> CloseShift(Guid id, [FromBody] CloseShiftDto dto)
        => HandleResult(await Mediator.Send(new CloseShiftCommand(id, CurrentUserId, dto)));

    [HttpPost($"{ApiRoutes.Sales.Shifts}/{{id:guid}}/suspend")]
    [HasPermission(Permissions.Shift.Suspend)]
    public async Task<IActionResult> SuspendShift(Guid id)
        => HandleResult(await Mediator.Send(new SuspendShiftCommand(id)));

    [HttpPost($"{ApiRoutes.Sales.Shifts}/{{id:guid}}/resume")]
    [HasPermission(Permissions.Shift.Resume)]
    public async Task<IActionResult> ResumeShift(Guid id)
        => HandleResult(await Mediator.Send(new ResumeShiftCommand(id)));

    [HttpPost($"{ApiRoutes.Sales.Shifts}/{{id:guid}}/reconcile")]
    [HasPermission(Permissions.Shift.Close)]
    public async Task<IActionResult> ReconcileShift(Guid id, [FromBody] ReconcileShiftDto dto)
        => HandleResult(await Mediator.Send(new ReconcileShiftCommand(id, CurrentUserId, dto)));
}
