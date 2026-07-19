using Asp.Versioning;
using GastroErp.Application.Features.Sales.BackOffice.Dashboard;
using GastroErp.Domain.Enums;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Sales.BackOffice;

[ApiVersion("1.0")]
public class BackOfficeSalesDashboardController : BaseApiController
{
    [HttpGet(ApiRoutes.BackOfficeSales.Dashboard)]
    [HasPermission(Permissions.BackOfficeSales.View)]
    public async Task<IActionResult> Get(
        [FromQuery] DateOnly? fromDate = null,
        [FromQuery] DateOnly? toDate = null,
        [FromQuery] Guid? branchId = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] BackOfficeSalesDocumentStatus? status = null)
    {
        var filter = new BackOfficeSalesDashboardFilterDto(fromDate, toDate, branchId, customerId, status);
        return HandleResult(await Mediator.Send(new GetBackOfficeSalesDashboardQuery(TenantId, filter)));
    }
}
