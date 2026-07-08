using Asp.Versioning;
using GastroErp.Application.Features.Menu.Commands;
using GastroErp.Application.Features.Menu.DTOs;
using GastroErp.Application.Features.Menu.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Menu;

/// <summary>
/// Category management
/// </summary>
[ApiVersion("1.0")]
public class CategoryController : BaseApiController
{
    [HttpGet(ApiRoutes.Menu.Categories)]
    [HasPermission(Permissions.Category.View)]
    public async Task<IActionResult> GetCategories([FromQuery] Guid? parentCategoryId, [FromQuery] bool? isActive, [FromQuery] PaginationQuery query)
    {
        return HandlePagedResult(await Mediator.Send(new GetCategoriesQuery(TenantId, parentCategoryId, isActive, query.Page, query.PageSize)));
    }

    [HttpGet($"{ApiRoutes.Menu.Categories}/{{id:guid}}")]
    [HasPermission(Permissions.Category.View)]
    public async Task<IActionResult> GetCategoryById(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetCategoryByIdQuery(id)));
    }

    [HttpPost(ApiRoutes.Menu.Categories)]
    [HasPermission(Permissions.Category.Create)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        var finalDto = dto with { TenantId = TenantId };
        return HandleResult(await Mediator.Send(new CreateCategoryCommand(finalDto)));
    }

    [HttpPut($"{ApiRoutes.Menu.Categories}/{{id:guid}}")]
    [HasPermission(Permissions.Category.Update)]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryDto dto)
    {
        return HandleResult(await Mediator.Send(new UpdateCategoryCommand(id, dto)));
    }

    [HttpPut($"{ApiRoutes.Menu.Categories}/{{id:guid}}/image")]
    [HasPermission(Permissions.Category.Update)]
    public async Task<IActionResult> SetCategoryImage(Guid id, [FromQuery] string? imageUrl)
    {
        return HandleResult(await Mediator.Send(new SetCategoryImageCommand(id, imageUrl)));
    }

    [HttpPut($"{ApiRoutes.Menu.Categories}/{{id:guid}}/deactivate")]
    [HasPermission(Permissions.Category.Update)]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeactivateCategoryCommand(id)));
    }

    [HttpPut($"{ApiRoutes.Menu.Categories}/{{id:guid}}/activate")]
    [HasPermission(Permissions.Category.Update)]
    public async Task<IActionResult> Activate(Guid id)
    {
        return HandleResult(await Mediator.Send(new ActivateCategoryCommand(id)));
    }
}
