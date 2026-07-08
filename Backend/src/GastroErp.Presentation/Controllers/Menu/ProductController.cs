using Asp.Versioning;
using GastroErp.Application.Features.Menu.Commands;
using GastroErp.Application.Features.Menu.DTOs;
using GastroErp.Application.Features.Menu.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Menu;

/// <summary>
/// Product management
/// </summary>
[ApiVersion("1.0")]
public class ProductController : BaseApiController
{
    [HttpGet(ApiRoutes.Menu.Products)]
    [HasPermission(Permissions.Product.View)]
    public async Task<IActionResult> GetProducts([FromQuery] PaginationQuery query)
    {
        return HandlePagedResult(await Mediator.Send(new GetProductsQuery(TenantId, null, null, null, query.Search, query.Page, query.PageSize)));
    }

    [HttpGet($"{ApiRoutes.Menu.Products}/{{id:guid}}")]
    [HasPermission(Permissions.Product.View)]
    public async Task<IActionResult> GetProductById(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetProductByIdQuery(id)));
    }

    [HttpPost(ApiRoutes.Menu.Products)]
    [HasPermission(Permissions.Product.Create)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
    {
        var finalDto = dto with { TenantId = TenantId };
        return HandleResult(await Mediator.Send(new CreateProductCommand(finalDto)));
    }

    [HttpPut($"{ApiRoutes.Menu.Products}/{{id:guid}}")]
    [HasPermission(Permissions.Product.Update)]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductDto dto)
    {
        return HandleResult(await Mediator.Send(new UpdateProductCommand(id, dto)));
    }

    [HttpPut($"{ApiRoutes.Menu.Products}/{{id:guid}}/price")]
    [HasPermission(Permissions.Product.Update)]
    public async Task<IActionResult> UpdateProductPrice(Guid id, [FromBody] UpdateProductPriceDto dto)
    {
        return HandleResult(await Mediator.Send(new UpdateProductPriceCommand(id, dto)));
    }

    [HttpPut($"{ApiRoutes.Menu.Products}/{{id:guid}}/category/{{categoryId:guid}}")]
    [HasPermission(Permissions.Product.Update)]
    public async Task<IActionResult> SetProductCategory(Guid id, Guid categoryId)
    {
        return HandleResult(await Mediator.Send(new SetProductCategoryCommand(id, categoryId)));
    }

    [HttpPut($"{ApiRoutes.Menu.Products}/{{id:guid}}/featured")]
    [HasPermission(Permissions.Product.Update)]
    public async Task<IActionResult> SetProductFeatured(Guid id, [FromQuery] bool isFeatured)
    {
        return HandleResult(await Mediator.Send(new SetProductFeaturedCommand(id, isFeatured)));
    }

    [HttpPut($"{ApiRoutes.Menu.Products}/{{id:guid}}/unavailable")]
    [HasPermission(Permissions.Product.Update)]
    public async Task<IActionResult> MarkProductUnavailable(Guid id, [FromQuery] string reason)
    {
        return HandleResult(await Mediator.Send(new MarkProductUnavailableCommand(id, reason)));
    }

    [HttpPut($"{ApiRoutes.Menu.Products}/{{id:guid}}/available")]
    [HasPermission(Permissions.Product.Update)]
    public async Task<IActionResult> MarkProductAvailable(Guid id)
    {
        return HandleResult(await Mediator.Send(new MarkProductAvailableCommand(id)));
    }

    // ─── Product Images ─────────────────────────────────────────────────────────

    [HttpPost($"{ApiRoutes.Menu.Products}/{{id:guid}}/images")]
    [HasPermission(Permissions.Product.Update)]
    public async Task<IActionResult> AddProductImage(Guid id, [FromQuery] string imageUrl, [FromQuery] string? thumbnailUrl, [FromQuery] string? altText, [FromQuery] bool isPrimary)
    {
        return HandleResult(await Mediator.Send(new AddProductImageCommand(id, imageUrl, thumbnailUrl, altText, isPrimary)));
    }

    [HttpDelete($"{ApiRoutes.Menu.Products}/{{id:guid}}/images/{{imageId:guid}}")]
    [HasPermission(Permissions.Product.Update)]
    public async Task<IActionResult> RemoveProductImage(Guid id, Guid imageId)
    {
        return HandleResult(await Mediator.Send(new RemoveProductImageCommand(id, imageId)));
    }

    [HttpPut($"{ApiRoutes.Menu.Products}/{{id:guid}}/images/{{imageId:guid}}/primary")]
    [HasPermission(Permissions.Product.Update)]
    public async Task<IActionResult> SetProductPrimaryImage(Guid id, Guid imageId)
    {
        return HandleResult(await Mediator.Send(new SetProductPrimaryImageCommand(id, imageId)));
    }

    // ─── Price Levels ───────────────────────────────────────────────────────────

    [HttpPut($"{ApiRoutes.Menu.Products}/{{id:guid}}/price-levels/{{priceLevelId:guid}}")]
    [HasPermission(Permissions.Product.Update)]
    public async Task<IActionResult> SetProductPriceLevel(Guid id, Guid priceLevelId, [FromQuery] decimal price)
    {
        return HandleResult(await Mediator.Send(new SetProductPriceLevelCommand(id, priceLevelId, price)));
    }

    // ─── Modifier Groups ────────────────────────────────────────────────────────

    [HttpPost($"{ApiRoutes.Menu.Products}/modifier-groups")]
    [HasPermission(Permissions.Product.Update)]
    public async Task<IActionResult> AddModifierGroup([FromBody] CreateModifierGroupDto dto)
    {
        return HandleResult(await Mediator.Send(new AddModifierGroupCommand(dto)));
    }

    [HttpDelete($"{ApiRoutes.Menu.Products}/{{id:guid}}/modifier-groups/{{groupId:guid}}")]
    [HasPermission(Permissions.Product.Update)]
    public async Task<IActionResult> RemoveModifierGroup(Guid id, Guid groupId)
    {
        return HandleResult(await Mediator.Send(new RemoveModifierGroupCommand(id, groupId)));
    }

    [HttpPost($"{ApiRoutes.Menu.Products}/modifier-groups/{{groupId:guid}}/modifiers")]
    [HasPermission(Permissions.Product.Update)]
    public async Task<IActionResult> AddModifier(Guid groupId, [FromBody] AddModifierDto dto)
    {
        return HandleResult(await Mediator.Send(new AddModifierCommand(groupId, dto)));
    }

    [HttpDelete($"{ApiRoutes.Menu.Products}/modifier-groups/{{groupId:guid}}/modifiers/{{modifierId:guid}}")]
    [HasPermission(Permissions.Product.Update)]
    public async Task<IActionResult> RemoveModifier(Guid groupId, Guid modifierId)
    {
        return HandleResult(await Mediator.Send(new RemoveModifierCommand(groupId, modifierId)));
    }

    [HttpPut($"{ApiRoutes.Menu.Products}/modifier-groups/{{groupId:guid}}/deactivate")]
    [HasPermission(Permissions.Product.Update)]
    public async Task<IActionResult> DeactivateModifierGroup(Guid groupId)
    {
        return HandleResult(await Mediator.Send(new DeactivateModifierGroupCommand(groupId)));
    }

    // ─── Option Groups ──────────────────────────────────────────────────────────

    [HttpPost($"{ApiRoutes.Menu.Products}/option-groups")]
    [HasPermission(Permissions.Product.Update)]
    public async Task<IActionResult> AddOptionGroup([FromBody] CreateOptionGroupDto dto)
    {
        return HandleResult(await Mediator.Send(new AddOptionGroupCommand(dto)));
    }

    [HttpDelete($"{ApiRoutes.Menu.Products}/{{id:guid}}/option-groups/{{groupId:guid}}")]
    [HasPermission(Permissions.Product.Update)]
    public async Task<IActionResult> RemoveOptionGroup(Guid id, Guid groupId)
    {
        return HandleResult(await Mediator.Send(new RemoveOptionGroupCommand(id, groupId)));
    }

    [HttpPut($"{ApiRoutes.Menu.Products}/option-groups/{{groupId:guid}}")]
    [HasPermission(Permissions.Product.Update)]
    public async Task<IActionResult> UpdateOptionGroup(Guid groupId, [FromBody] UpdateOptionGroupDto dto)
    {
        return HandleResult(await Mediator.Send(new UpdateOptionGroupCommand(groupId, dto)));
    }

    [HttpPut($"{ApiRoutes.Menu.Products}/option-groups/{{groupId:guid}}/deactivate")]
    [HasPermission(Permissions.Product.Update)]
    public async Task<IActionResult> DeactivateOptionGroup(Guid groupId)
    {
        return HandleResult(await Mediator.Send(new DeactivateOptionGroupCommand(groupId)));
    }

    [HttpPost($"{ApiRoutes.Menu.Products}/option-groups/{{groupId:guid}}/options")]
    [HasPermission(Permissions.Product.Update)]
    public async Task<IActionResult> AddOption(Guid groupId, [FromBody] AddOptionDto dto)
    {
        return HandleResult(await Mediator.Send(new AddOptionCommand(groupId, dto)));
    }

    [HttpDelete($"{ApiRoutes.Menu.Products}/option-groups/{{groupId:guid}}/options/{{optionId:guid}}")]
    [HasPermission(Permissions.Product.Update)]
    public async Task<IActionResult> RemoveOption(Guid groupId, Guid optionId)
    {
        return HandleResult(await Mediator.Send(new RemoveOptionCommand(groupId, optionId)));
    }

    [HttpPut($"{ApiRoutes.Menu.Products}/options/{{optionId:guid}}")]
    [HasPermission(Permissions.Product.Update)]
    public async Task<IActionResult> UpdateOption(Guid optionId, [FromBody] UpdateOptionDto dto)
    {
        return HandleResult(await Mediator.Send(new UpdateOptionCommand(optionId, dto)));
    }
}
