using Asp.Versioning;
using GastroErp.Application.Features.Inventory.Commands;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Application.Features.Inventory.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Inventory;

/// <summary>
/// Purchase Return management
/// </summary>
[ApiVersion("1.0")]
public class PurchaseReturnController : BaseApiController
{
    [HttpGet(ApiRoutes.Inventory.PurchaseReturns)]
    [HasPermission(Permissions.Inventory.View)]
    public async Task<IActionResult> GetPurchaseReturns([FromQuery] Guid? supplierId, [FromQuery] Guid? warehouseId, [FromQuery] PaginationQuery query)
    {
        return HandlePagedResult(await Mediator.Send(new GetPurchaseReturnsQuery(TenantId, supplierId, warehouseId, query.Page, query.PageSize)));
    }

    [HttpGet($"{ApiRoutes.Inventory.PurchaseReturns}/{{id:guid}}")]
    [HasPermission(Permissions.Inventory.View)]
    public async Task<IActionResult> GetPurchaseReturnById(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetPurchaseReturnByIdQuery(id)));
    }

    [HttpPost(ApiRoutes.Inventory.PurchaseReturns)]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> CreatePurchaseReturn([FromBody] CreatePurchaseReturnDto dto)
    {
        return HandleResult(await Mediator.Send(new CreatePurchaseReturnCommand(TenantId, dto)));
    }

    [HttpPost($"{ApiRoutes.Inventory.PurchaseReturns}/{{id:guid}}/lines")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> AddPurchaseReturnLine(Guid id, [FromBody] AddPurchaseReturnLineDto dto)
    {
        return HandleResult(await Mediator.Send(new AddPurchaseReturnLineCommand(id, dto)));
    }

    [HttpPost($"{ApiRoutes.Inventory.PurchaseReturns}/{{id:guid}}/approve")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> ApprovePurchaseReturn(Guid id)
    {
        return HandleResult(await Mediator.Send(new ApprovePurchaseReturnCommand(id)));
    }
}
