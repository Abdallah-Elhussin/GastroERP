using Asp.Versioning;
using GastroErp.Application.Features.Inventory.ValuationGroups.Commands;
using GastroErp.Application.Features.Inventory.ValuationGroups.Dtos;
using GastroErp.Application.Features.Inventory.ValuationGroups.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Inventory;

/// <summary>
/// Inventory valuation groups (accounting grouping + optional cost center).
/// </summary>
[ApiVersion("1.0")]
public class InventoryValuationGroupsController : BaseApiController
{
    [HttpGet(ApiRoutes.Inventory.ValuationGroups)]
    [HasPermission(Permissions.Inventory.ValuationGroups.View)]
    public async Task<IActionResult> Get(
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] Guid? costCenterId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
        => HandlePagedResult(await Mediator.Send(new GetInventoryValuationGroupsQuery(
            TenantId, search, isActive, costCenterId, sortBy, sortDesc, page, pageSize)));

    [HttpGet($"{ApiRoutes.Inventory.ValuationGroups}/lookup")]
    [HasPermission(Permissions.Inventory.ValuationGroups.View)]
    public async Task<IActionResult> Lookup([FromQuery] bool activeOnly = true)
        => HandleResult(await Mediator.Send(new GetInventoryValuationGroupLookupQuery(TenantId, activeOnly)));

    [HttpGet($"{ApiRoutes.Inventory.ValuationGroups}/{{id:guid}}")]
    [HasPermission(Permissions.Inventory.ValuationGroups.View)]
    public async Task<IActionResult> GetById(Guid id)
        => HandleResult(await Mediator.Send(new GetInventoryValuationGroupByIdQuery(id, TenantId)));

    [HttpPost(ApiRoutes.Inventory.ValuationGroups)]
    [HasPermission(Permissions.Inventory.ValuationGroups.Create)]
    public async Task<IActionResult> Create([FromBody] CreateInventoryValuationGroupRequest request)
        => HandleResult(await Mediator.Send(new CreateInventoryValuationGroupCommand(
            request with { TenantId = TenantId })));

    [HttpPut($"{ApiRoutes.Inventory.ValuationGroups}/{{id:guid}}")]
    [HasPermission(Permissions.Inventory.ValuationGroups.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateInventoryValuationGroupRequest request)
        => HandleResult(await Mediator.Send(new UpdateInventoryValuationGroupCommand(
            id, request with { TenantId = TenantId })));

    [HttpDelete($"{ApiRoutes.Inventory.ValuationGroups}/{{id:guid}}")]
    [HasPermission(Permissions.Inventory.ValuationGroups.Delete)]
    public async Task<IActionResult> Delete(Guid id)
        => HandleResult(await Mediator.Send(new DeleteInventoryValuationGroupCommand(id, TenantId)));

    [HttpPost($"{ApiRoutes.Inventory.ValuationGroups}/{{id:guid}}/activate")]
    [HasPermission(Permissions.Inventory.ValuationGroups.Edit)]
    public async Task<IActionResult> Activate(Guid id)
        => HandleResult(await Mediator.Send(new ActivateInventoryValuationGroupCommand(id, TenantId, true)));

    [HttpPost($"{ApiRoutes.Inventory.ValuationGroups}/{{id:guid}}/deactivate")]
    [HasPermission(Permissions.Inventory.ValuationGroups.Edit)]
    public async Task<IActionResult> Deactivate(Guid id)
        => HandleResult(await Mediator.Send(new ActivateInventoryValuationGroupCommand(id, TenantId, false)));
}
