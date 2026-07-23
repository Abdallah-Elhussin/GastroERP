using Asp.Versioning;
using GastroErp.Application.Features.Inventory.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Inventory;

/// <summary>لوحة المشتريات — ملخص تشغيلي لأوامر الشراء والاستلام والفواتير والموردين.</summary>
[ApiVersion("1.0")]
public class PurchasingDashboardController : BaseApiController
{
    [HttpGet(ApiRoutes.Inventory.PurchasingDashboard)]
    [HasPermission(Permissions.Purchase.View)]
    public async Task<IActionResult> GetDashboard()
        => HandleResult(await Mediator.Send(new GetPurchasingDashboardQuery(TenantId)));
}
