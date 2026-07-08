using Asp.Versioning;
using GastroErp.Application.Features.Inventory.Commands;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Application.Features.Inventory.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Inventory;

/// <summary>
/// Warehouse management
/// </summary>
[ApiVersion("1.0")]
public class WarehouseController : BaseApiController
{
    [HttpGet(ApiRoutes.Inventory.Warehouses)]
    [HasPermission(Permissions.Warehouse.View)]
    public async Task<IActionResult> GetWarehouses([FromQuery] PaginationQuery query)
    {
        return HandlePagedResult(await Mediator.Send(new GetWarehousesQuery(TenantId, BranchId, null, query.Page, query.PageSize)));
    }

    [HttpGet($"{ApiRoutes.Inventory.Warehouses}/{{id:guid}}")]
    [HasPermission(Permissions.Warehouse.View)]
    public async Task<IActionResult> GetWarehouseById(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetWarehouseByIdQuery(id)));
    }

    [HttpPost(ApiRoutes.Inventory.Warehouses)]
    [HasPermission(Permissions.Warehouse.Create)]
    public async Task<IActionResult> CreateWarehouse([FromBody] CreateWarehouseDto dto)
    {
        return HandleResult(await Mediator.Send(new CreateWarehouseCommand(dto)));
    }

    [HttpPut($"{ApiRoutes.Inventory.Warehouses}/{{id:guid}}")]
    [HasPermission(Permissions.Warehouse.Update)]
    public async Task<IActionResult> UpdateWarehouse(Guid id, [FromBody] UpdateWarehouseDto dto)
    {
        return HandleResult(await Mediator.Send(new UpdateWarehouseCommand(id, dto)));
    }
}
