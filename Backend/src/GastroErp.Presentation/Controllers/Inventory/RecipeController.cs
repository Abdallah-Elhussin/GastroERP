using Asp.Versioning;
using GastroErp.Application.Features.Inventory.Commands;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Application.Features.Inventory.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Inventory;

/// <summary>
/// Recipe management
/// </summary>
[ApiVersion("1.0")]
public class RecipeController : BaseApiController
{
    [HttpGet(ApiRoutes.Inventory.Recipes)]
    [HasPermission(Permissions.Recipe.View)]
    public async Task<IActionResult> GetRecipes([FromQuery] PaginationQuery query)
    {
        return HandlePagedResult(await Mediator.Send(new GetRecipesQuery(TenantId, query.Page, query.PageSize)));
    }

    [HttpGet($"{ApiRoutes.Inventory.Recipes}/{{id:guid}}")]
    [HasPermission(Permissions.Recipe.View)]
    public async Task<IActionResult> GetRecipeById(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetRecipeByIdQuery(id)));
    }

    [HttpPost(ApiRoutes.Inventory.Recipes)]
    [HasPermission(Permissions.Recipe.Create)]
    public async Task<IActionResult> CreateRecipe([FromBody] CreateRecipeDto dto)
    {
        return HandleResult(await Mediator.Send(new CreateRecipeCommand(dto)));
    }

    [HttpPut($"{ApiRoutes.Inventory.Recipes}/{{id:guid}}")]
    [HasPermission(Permissions.Recipe.Update)]
    public async Task<IActionResult> UpdateRecipe(Guid id, [FromBody] UpdateRecipeDto dto)
    {
        return HandleResult(await Mediator.Send(new UpdateRecipeCommand(id, dto)));
    }
}
