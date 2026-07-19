using Asp.Versioning;
using GastroErp.Application.Features.Inventory.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Inventory;

/// <summary>
/// Inventory dashboard KPIs and operational snapshots (Phase F).
/// </summary>
[ApiVersion("1.0")]
public class InventoryDashboardController : BaseApiController
{
    [HttpGet(ApiRoutes.Inventory.Dashboard)]
    [HasPermission(Permissions.Inventory.View)]
    public async Task<IActionResult> GetDashboard()
    {
        return HandleResult(await Mediator.Send(new GetInventoryDashboardQuery(TenantId)));
    }
}
