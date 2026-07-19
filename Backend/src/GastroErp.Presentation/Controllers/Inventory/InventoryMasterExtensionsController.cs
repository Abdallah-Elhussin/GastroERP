using Asp.Versioning;
using GastroErp.Application.Features.Inventory.Commands;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Application.Features.Inventory.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Inventory;

/// <summary>
/// Phase J — brands, manufacturers, attributes, price lists.
/// </summary>
[ApiVersion("1.0")]
public class InventoryMasterExtensionsController : BaseApiController
{
    // ─── Brands ───────────────────────────────────────────────────────────────

    [HttpGet(ApiRoutes.Inventory.Brands)]
    [HasPermission(Permissions.Inventory.View)]
    public async Task<IActionResult> GetBrands([FromQuery] bool? isActive = null)
        => HandleResult(await Mediator.Send(new GetInventoryBrandsQuery(TenantId, isActive)));

    [HttpPost(ApiRoutes.Inventory.Brands)]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> CreateBrand([FromBody] UpsertInventoryBrandDto dto)
        => HandleResult(await Mediator.Send(new CreateInventoryBrandCommand(dto with { TenantId = TenantId })));

    [HttpPut($"{ApiRoutes.Inventory.Brands}/{{id:guid}}")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> UpdateBrand(Guid id, [FromBody] UpsertInventoryBrandDto dto)
        => HandleResult(await Mediator.Send(new UpdateInventoryBrandCommand(id, dto with { TenantId = TenantId })));

    [HttpPost($"{ApiRoutes.Inventory.Brands}/{{id:guid}}/activate")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> ActivateBrand(Guid id)
        => HandleResult(await Mediator.Send(new ActivateInventoryBrandCommand(id)));

    [HttpPost($"{ApiRoutes.Inventory.Brands}/{{id:guid}}/deactivate")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> DeactivateBrand(Guid id)
        => HandleResult(await Mediator.Send(new DeactivateInventoryBrandCommand(id)));

    // ─── Manufacturers ────────────────────────────────────────────────────────

    [HttpGet(ApiRoutes.Inventory.Manufacturers)]
    [HasPermission(Permissions.Inventory.View)]
    public async Task<IActionResult> GetManufacturers([FromQuery] bool? isActive = null)
        => HandleResult(await Mediator.Send(new GetInventoryManufacturersQuery(TenantId, isActive)));

    [HttpPost(ApiRoutes.Inventory.Manufacturers)]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> CreateManufacturer([FromBody] UpsertInventoryManufacturerDto dto)
        => HandleResult(await Mediator.Send(new CreateInventoryManufacturerCommand(dto with { TenantId = TenantId })));

    [HttpPut($"{ApiRoutes.Inventory.Manufacturers}/{{id:guid}}")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> UpdateManufacturer(Guid id, [FromBody] UpsertInventoryManufacturerDto dto)
        => HandleResult(await Mediator.Send(new UpdateInventoryManufacturerCommand(id, dto with { TenantId = TenantId })));

    [HttpPost($"{ApiRoutes.Inventory.Manufacturers}/{{id:guid}}/activate")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> ActivateManufacturer(Guid id)
        => HandleResult(await Mediator.Send(new ActivateInventoryManufacturerCommand(id)));

    [HttpPost($"{ApiRoutes.Inventory.Manufacturers}/{{id:guid}}/deactivate")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> DeactivateManufacturer(Guid id)
        => HandleResult(await Mediator.Send(new DeactivateInventoryManufacturerCommand(id)));

    // ─── Attributes ───────────────────────────────────────────────────────────

    [HttpGet(ApiRoutes.Inventory.Attributes)]
    [HasPermission(Permissions.Inventory.View)]
    public async Task<IActionResult> GetAttributes([FromQuery] bool? isActive = null)
        => HandleResult(await Mediator.Send(new GetInventoryAttributesQuery(TenantId, isActive)));

    [HttpGet($"{ApiRoutes.Inventory.Attributes}/{{id:guid}}")]
    [HasPermission(Permissions.Inventory.View)]
    public async Task<IActionResult> GetAttributeById(Guid id)
        => HandleResult(await Mediator.Send(new GetInventoryAttributeByIdQuery(id)));

    [HttpPost(ApiRoutes.Inventory.Attributes)]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> CreateAttribute([FromBody] UpsertInventoryAttributeDto dto)
        => HandleResult(await Mediator.Send(new CreateInventoryAttributeCommand(dto with { TenantId = TenantId })));

    [HttpPut($"{ApiRoutes.Inventory.Attributes}/{{id:guid}}")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> UpdateAttribute(Guid id, [FromBody] UpsertInventoryAttributeDto dto)
        => HandleResult(await Mediator.Send(new UpdateInventoryAttributeCommand(id, dto with { TenantId = TenantId })));

    [HttpPost($"{ApiRoutes.Inventory.Attributes}/{{id:guid}}/activate")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> ActivateAttribute(Guid id)
        => HandleResult(await Mediator.Send(new ActivateInventoryAttributeCommand(id)));

    [HttpPost($"{ApiRoutes.Inventory.Attributes}/{{id:guid}}/deactivate")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> DeactivateAttribute(Guid id)
        => HandleResult(await Mediator.Send(new DeactivateInventoryAttributeCommand(id)));

    [HttpPost($"{ApiRoutes.Inventory.Attributes}/{{id:guid}}/values")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> AddAttributeValue(Guid id, [FromBody] AddInventoryAttributeValueDto dto)
        => HandleResult(await Mediator.Send(new AddInventoryAttributeValueCommand(id, dto)));

    [HttpDelete($"{ApiRoutes.Inventory.Attributes}/{{id:guid}}/values/{{valueId:guid}}")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> RemoveAttributeValue(Guid id, Guid valueId)
        => HandleResult(await Mediator.Send(new RemoveInventoryAttributeValueCommand(id, valueId)));

    // ─── Price Lists ──────────────────────────────────────────────────────────

    [HttpGet(ApiRoutes.Inventory.PriceLists)]
    [HasPermission(Permissions.Inventory.View)]
    public async Task<IActionResult> GetPriceLists([FromQuery] bool? isActive = null)
        => HandleResult(await Mediator.Send(new GetInventoryPriceListsQuery(TenantId, isActive)));

    [HttpGet($"{ApiRoutes.Inventory.PriceLists}/{{id:guid}}")]
    [HasPermission(Permissions.Inventory.View)]
    public async Task<IActionResult> GetPriceListById(Guid id)
        => HandleResult(await Mediator.Send(new GetInventoryPriceListByIdQuery(id)));

    [HttpPost(ApiRoutes.Inventory.PriceLists)]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> CreatePriceList([FromBody] UpsertInventoryPriceListDto dto)
        => HandleResult(await Mediator.Send(new CreateInventoryPriceListCommand(dto with { TenantId = TenantId })));

    [HttpPut($"{ApiRoutes.Inventory.PriceLists}/{{id:guid}}")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> UpdatePriceList(Guid id, [FromBody] UpsertInventoryPriceListDto dto)
        => HandleResult(await Mediator.Send(new UpdateInventoryPriceListCommand(id, dto with { TenantId = TenantId })));

    [HttpPost($"{ApiRoutes.Inventory.PriceLists}/{{id:guid}}/activate")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> ActivatePriceList(Guid id)
        => HandleResult(await Mediator.Send(new ActivateInventoryPriceListCommand(id)));

    [HttpPost($"{ApiRoutes.Inventory.PriceLists}/{{id:guid}}/deactivate")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> DeactivatePriceList(Guid id)
        => HandleResult(await Mediator.Send(new DeactivateInventoryPriceListCommand(id)));

    [HttpPost($"{ApiRoutes.Inventory.PriceLists}/{{id:guid}}/lines")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> UpsertPriceListLine(Guid id, [FromBody] UpsertInventoryPriceListLineDto dto)
        => HandleResult(await Mediator.Send(new UpsertInventoryPriceListLineCommand(id, dto)));

    [HttpDelete($"{ApiRoutes.Inventory.PriceLists}/{{id:guid}}/lines/{{lineId:guid}}")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> RemovePriceListLine(Guid id, Guid lineId)
        => HandleResult(await Mediator.Send(new RemoveInventoryPriceListLineCommand(id, lineId)));
}
