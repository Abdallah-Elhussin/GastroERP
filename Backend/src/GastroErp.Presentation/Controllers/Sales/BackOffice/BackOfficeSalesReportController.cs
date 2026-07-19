using Asp.Versioning;
using GastroErp.Application.Features.Sales.BackOffice.Reports;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Sales.BackOffice;

[ApiVersion("1.0")]
public class BackOfficeSalesReportController : BaseApiController
{
    [HttpGet(ApiRoutes.BackOfficeSales.Reports)]
    [HasPermission(Permissions.BackOfficeSales.ViewReports)]
    public async Task<IActionResult> Get(
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? branchId = null,
        [FromQuery] int topCustomers = 20,
        [FromQuery] int topItems = 20)
        => HandleResult(await Mediator.Send(new GetBackOfficeSalesReportQuery(
            TenantId, from, to, customerId, branchId, topCustomers, topItems)));
}
