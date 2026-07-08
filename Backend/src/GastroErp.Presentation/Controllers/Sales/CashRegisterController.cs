using Asp.Versioning;
using GastroErp.Application.Features.Sales.Commands;
using GastroErp.Application.Features.Sales.DTOs;
using GastroErp.Application.Features.Sales.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Sales;

[ApiVersion("1.0")]
public class CashRegisterController : BaseApiController
{
    private Guid CurrentUserId => HttpContext.RequestServices
        .GetRequiredService<GastroErp.Application.Common.Interfaces.ICurrentUser>().Id ?? Guid.Empty;

    [HttpGet(ApiRoutes.Sales.CashRegisters)]
    [HasPermission(Permissions.CashRegister.View)]
    public async Task<IActionResult> GetRegisters([FromQuery] CashRegisterFilterDto filter)
        => HandlePagedResult(await Mediator.Send(new GetCashRegistersQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Sales.CashRegisters}/{{id:guid}}")]
    [HasPermission(Permissions.CashRegister.View)]
    public async Task<IActionResult> GetRegisterById(Guid id)
        => HandleResult(await Mediator.Send(new GetCashRegisterByIdQuery(id)));

    [HttpGet($"{ApiRoutes.Sales.CashRegisters}/branch/{{branchId:guid}}/current")]
    [HasPermission(Permissions.CashRegister.View)]
    public async Task<IActionResult> GetCurrentRegister(Guid branchId)
        => HandleResult(await Mediator.Send(new GetCurrentCashRegisterQuery(branchId)));

    [HttpPost(ApiRoutes.Sales.CashRegisters)]
    [HasPermission(Permissions.CashRegister.Open)]
    public async Task<IActionResult> CreateRegister([FromBody] CreateCashRegisterDto dto)
        => HandleResult(await Mediator.Send(new CreateCashRegisterCommand(TenantId, dto)));

    [HttpPost($"{ApiRoutes.Sales.CashRegisters}/{{id:guid}}/open")]
    [HasPermission(Permissions.CashRegister.Open)]
    public async Task<IActionResult> OpenRegister(Guid id, [FromBody] OpenCashRegisterDto dto)
        => HandleResult(await Mediator.Send(new OpenCashRegisterCommand(id, CurrentUserId, dto)));

    [HttpPost($"{ApiRoutes.Sales.CashRegisters}/{{id:guid}}/close")]
    [HasPermission(Permissions.CashRegister.Close)]
    public async Task<IActionResult> CloseRegister(Guid id, [FromBody] CloseCashRegisterDto dto)
        => HandleResult(await Mediator.Send(new CloseCashRegisterCommand(id, CurrentUserId, dto)));

    [HttpPost($"{ApiRoutes.Sales.CashRegisters}/{{id:guid}}/suspend")]
    [HasPermission(Permissions.CashRegister.Close)]
    public async Task<IActionResult> SuspendRegister(Guid id)
        => HandleResult(await Mediator.Send(new SuspendCashRegisterCommand(id)));

    [HttpPost($"{ApiRoutes.Sales.CashRegisters}/{{id:guid}}/resume")]
    [HasPermission(Permissions.CashRegister.Open)]
    public async Task<IActionResult> ResumeRegister(Guid id)
        => HandleResult(await Mediator.Send(new ResumeCashRegisterCommand(id)));
}
