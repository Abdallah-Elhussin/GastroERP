using Asp.Versioning;
using GastroErp.Application.Features.Inventory.Commands;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Application.Features.Inventory.Queries;
using GastroErp.Domain.Enums;
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
    [HttpGet($"{ApiRoutes.Inventory.Warehouses}/lookup")]
    [HasPermission(Permissions.InventoryWarehouses.View)]
    public async Task<IActionResult> GetLookup([FromQuery] Guid? branchId = null, [FromQuery] bool activeOnly = true)
        => HandleResult(await Mediator.Send(new GetWarehouseLookupQuery(
            TenantId,
            branchId ?? BranchId,
            activeOnly)));

    [HttpGet(ApiRoutes.Inventory.Warehouses)]
    [HasPermission(Permissions.InventoryWarehouses.View)]
    public async Task<IActionResult> GetWarehouses(
        [FromQuery] string? search = null,
        [FromQuery] Guid? branchId = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] WarehouseType? warehouseType = null,
        [FromQuery] Guid? warehouseTypeId = null,
        [FromQuery] bool? isPosWarehouse = null,
        [FromQuery] bool? isDefault = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        return HandlePagedResult(await Mediator.Send(new GetWarehousesQuery(
            TenantId,
            branchId ?? BranchId,
            isActive,
            warehouseType,
            warehouseTypeId,
            isPosWarehouse,
            isDefault,
            search,
            sortBy,
            sortDesc,
            page,
            pageSize)));
    }

    [HttpGet($"{ApiRoutes.Inventory.Warehouses}/types")]
    [HasPermission(Permissions.InventoryWarehouses.View)]
    public async Task<IActionResult> GetTypes([FromQuery] bool? isActive = null)
        => HandleResult(await Mediator.Send(new GetWarehouseTypeDefinitionsQuery(TenantId, isActive)));

    [HttpPost($"{ApiRoutes.Inventory.Warehouses}/types")]
    [HasPermission(Permissions.InventoryWarehouses.Create)]
    public async Task<IActionResult> CreateType([FromBody] UpsertWarehouseTypeDefinitionDto dto)
        => HandleResult(await Mediator.Send(new CreateWarehouseTypeDefinitionCommand(dto with { TenantId = TenantId })));

    [HttpPut($"{ApiRoutes.Inventory.Warehouses}/types/{{id:guid}}")]
    [HasPermission(Permissions.InventoryWarehouses.Edit)]
    public async Task<IActionResult> UpdateType(Guid id, [FromBody] UpsertWarehouseTypeDefinitionDto dto)
        => HandleResult(await Mediator.Send(new UpdateWarehouseTypeDefinitionCommand(id, dto with { TenantId = TenantId })));

    [HttpPost($"{ApiRoutes.Inventory.Warehouses}/types/{{id:guid}}/activate")]
    [HasPermission(Permissions.InventoryWarehouses.Edit)]
    public async Task<IActionResult> ActivateType(Guid id)
        => HandleResult(await Mediator.Send(new ActivateWarehouseTypeDefinitionCommand(id, true)));

    [HttpPost($"{ApiRoutes.Inventory.Warehouses}/types/{{id:guid}}/deactivate")]
    [HasPermission(Permissions.InventoryWarehouses.Edit)]
    public async Task<IActionResult> DeactivateType(Guid id)
        => HandleResult(await Mediator.Send(new ActivateWarehouseTypeDefinitionCommand(id, false)));

    [HttpGet($"{ApiRoutes.Inventory.Warehouses}/{{id:guid}}")]
    [HasPermission(Permissions.InventoryWarehouses.View)]
    public async Task<IActionResult> GetWarehouseById(Guid id)
        => HandleResult(await Mediator.Send(new GetWarehouseByIdQuery(id)));

    [HttpPost(ApiRoutes.Inventory.Warehouses)]
    [HasPermission(Permissions.InventoryWarehouses.Create)]
    public async Task<IActionResult> CreateWarehouse([FromBody] CreateWarehouseDto dto)
        => HandleResult(await Mediator.Send(new CreateWarehouseCommand(dto with { TenantId = TenantId })));

    [HttpPut($"{ApiRoutes.Inventory.Warehouses}/{{id:guid}}")]
    [HasPermission(Permissions.InventoryWarehouses.Edit)]
    public async Task<IActionResult> UpdateWarehouse(Guid id, [FromBody] UpdateWarehouseDto dto)
        => HandleResult(await Mediator.Send(new UpdateWarehouseCommand(id, dto)));

    [HttpDelete($"{ApiRoutes.Inventory.Warehouses}/{{id:guid}}")]
    [HasPermission(Permissions.InventoryWarehouses.Delete)]
    public async Task<IActionResult> DeleteWarehouse(Guid id)
        => HandleResult(await Mediator.Send(new DeleteWarehouseCommand(id)));

    [HttpPost($"{ApiRoutes.Inventory.Warehouses}/{{id:guid}}/activate")]
    [HasPermission(Permissions.InventoryWarehouses.Edit)]
    public async Task<IActionResult> ActivateWarehouse(Guid id)
        => HandleResult(await Mediator.Send(new ActivateWarehouseCommand(id)));

    [HttpPost($"{ApiRoutes.Inventory.Warehouses}/{{id:guid}}/deactivate")]
    [HasPermission(Permissions.InventoryWarehouses.Edit)]
    public async Task<IActionResult> DeactivateWarehouse(Guid id)
        => HandleResult(await Mediator.Send(new DeactivateWarehouseCommand(id)));

    [HttpPost($"{ApiRoutes.Inventory.Warehouses}/{{id:guid}}/zones")]
    [HasPermission(Permissions.InventoryWarehouses.Edit)]
    public async Task<IActionResult> AddZone(Guid id, [FromBody] AddWarehouseZoneDto dto)
        => HandleResult(await Mediator.Send(new AddWarehouseZoneCommand(id, dto)));

    [HttpDelete($"{ApiRoutes.Inventory.Warehouses}/{{id:guid}}/zones/{{zoneId:guid}}")]
    [HasPermission(Permissions.InventoryWarehouses.Edit)]
    public async Task<IActionResult> RemoveZone(Guid id, Guid zoneId)
        => HandleResult(await Mediator.Send(new RemoveWarehouseZoneCommand(id, zoneId)));

    [HttpPost($"{ApiRoutes.Inventory.Warehouses}/{{id:guid}}/zones/{{zoneId:guid}}/shelves")]
    [HasPermission(Permissions.InventoryWarehouses.Edit)]
    public async Task<IActionResult> AddShelf(Guid id, Guid zoneId, [FromBody] AddWarehouseShelfDto dto)
        => HandleResult(await Mediator.Send(new AddWarehouseShelfCommand(id, zoneId, dto)));

    [HttpDelete($"{ApiRoutes.Inventory.Warehouses}/{{id:guid}}/zones/{{zoneId:guid}}/shelves/{{shelfId:guid}}")]
    [HasPermission(Permissions.InventoryWarehouses.Edit)]
    public async Task<IActionResult> RemoveShelf(Guid id, Guid zoneId, Guid shelfId)
        => HandleResult(await Mediator.Send(new RemoveWarehouseShelfCommand(id, zoneId, shelfId)));

    [HttpPost($"{ApiRoutes.Inventory.Warehouses}/{{id:guid}}/zones/{{zoneId:guid}}/shelves/{{shelfId:guid}}/bins")]
    [HasPermission(Permissions.InventoryWarehouses.Edit)]
    public async Task<IActionResult> AddBin(Guid id, Guid zoneId, Guid shelfId, [FromBody] AddWarehouseBinDto dto)
        => HandleResult(await Mediator.Send(new AddWarehouseBinCommand(id, zoneId, shelfId, dto)));

    [HttpDelete($"{ApiRoutes.Inventory.Warehouses}/{{id:guid}}/zones/{{zoneId:guid}}/shelves/{{shelfId:guid}}/bins/{{binId:guid}}")]
    [HasPermission(Permissions.InventoryWarehouses.Edit)]
    public async Task<IActionResult> RemoveBin(Guid id, Guid zoneId, Guid shelfId, Guid binId)
        => HandleResult(await Mediator.Send(new RemoveWarehouseBinCommand(id, zoneId, shelfId, binId)));
}
