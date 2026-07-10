using Asp.Versioning;
using GastroErp.Application.Features.Inventory.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Inventory;

/// <summary>
/// Inventory catalog master data (categories, units)
/// </summary>
[ApiVersion("1.0")]
public class CatalogController : BaseApiController
{
    [HttpGet(ApiRoutes.Inventory.Categories)]
    [HasPermission(Permissions.Inventory.View)]
    public async Task<IActionResult> GetCategories([FromQuery] PaginationQuery query)
    {
        return HandlePagedResult(await Mediator.Send(new GetInventoryCategoriesQuery(TenantId, true, query.Page, query.PageSize)));
    }

    [HttpGet(ApiRoutes.Inventory.Units)]
    [HasPermission(Permissions.Inventory.View)]
    public async Task<IActionResult> GetUnits()
    {
        return HandleResult(await Mediator.Send(new GetInventoryUnitsQuery(TenantId, true)));
    }
}
