using Asp.Versioning;
using GastroErp.Application.Features.Sales.Dashboard;
using GastroErp.Domain.Enums;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Sales;

[ApiVersion("1.0")]
public class SalesDashboardController : BaseApiController
{
    [HttpGet(ApiRoutes.Sales.Dashboard)]
    [HasPermission(Permissions.Sales.View)]
    public async Task<IActionResult> Get(
        [FromQuery] DateOnly? fromDate = null,
        [FromQuery] DateOnly? toDate = null,
        [FromQuery] Guid? companyId = null,
        [FromQuery] Guid? branchId = null,
        [FromQuery] Guid? cashierId = null,
        [FromQuery] Guid? deviceId = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] PaymentMethodType? paymentMethod = null,
        [FromQuery] OrderStatus? orderStatus = null)
    {
        var filter = new SalesDashboardFilterDto(
            fromDate, toDate, companyId, branchId, cashierId, deviceId, customerId, paymentMethod, orderStatus);
        return HandleResult(await Mediator.Send(new GetSalesDashboardQuery(TenantId, filter)));
    }
}
