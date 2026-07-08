using Asp.Versioning;
using GastroErp.Application.Features.Sales.Commands;
using GastroErp.Application.Features.Sales.DTOs;
using GastroErp.Application.Features.Sales.Queries;
using GastroErp.Domain.Enums;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Sales;

[ApiVersion("1.0")]
public class CashMovementController : BaseApiController
{
    private Guid CurrentUserId => HttpContext.RequestServices
        .GetRequiredService<GastroErp.Application.Common.Interfaces.ICurrentUser>().Id ?? Guid.Empty;

    [HttpGet(ApiRoutes.Sales.CashMovements)]
    [HasPermission(Permissions.CashMovement.View)]
    public async Task<IActionResult> GetMovements([FromQuery] CashMovementFilterDto filter)
        => HandlePagedResult(await Mediator.Send(new GetCashMovementsQuery(TenantId, filter)));

    [HttpPost($"{ApiRoutes.Sales.CashMovements}/cash-in")]
    [HasPermission(Permissions.CashMovement.Create)]
    public async Task<IActionResult> CashIn([FromBody] CreateCashMovementDto dto)
        => HandleResult(await Mediator.Send(new CreateCashMovementCommand(CurrentUserId,
            dto with { MovementType = CashMovementType.CashIn })));

    [HttpPost($"{ApiRoutes.Sales.CashMovements}/cash-out")]
    [HasPermission(Permissions.CashMovement.Create)]
    public async Task<IActionResult> CashOut([FromBody] CreateCashMovementDto dto)
        => HandleResult(await Mediator.Send(new CreateCashMovementCommand(CurrentUserId,
            dto with { MovementType = CashMovementType.CashOut })));

    [HttpPost($"{ApiRoutes.Sales.CashMovements}/expense")]
    [HasPermission(Permissions.CashMovement.Create)]
    public async Task<IActionResult> Expense([FromBody] CreateCashMovementDto dto)
        => HandleResult(await Mediator.Send(new CreateCashMovementCommand(CurrentUserId,
            dto with { MovementType = CashMovementType.Expense })));

    [HttpPost($"{ApiRoutes.Sales.CashMovements}/safe-deposit")]
    [HasPermission(Permissions.CashMovement.Create)]
    public async Task<IActionResult> SafeDeposit([FromBody] CreateCashMovementDto dto)
        => HandleResult(await Mediator.Send(new CreateCashMovementCommand(CurrentUserId,
            dto with { MovementType = CashMovementType.SafeDeposit })));

    [HttpPost($"{ApiRoutes.Sales.CashMovements}/safe-withdrawal")]
    [HasPermission(Permissions.CashMovement.Create)]
    public async Task<IActionResult> SafeWithdrawal([FromBody] CreateCashMovementDto dto)
        => HandleResult(await Mediator.Send(new CreateCashMovementCommand(CurrentUserId,
            dto with { MovementType = CashMovementType.SafeWithdrawal })));
}
