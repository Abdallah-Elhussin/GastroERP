using Asp.Versioning;
using GastroErp.Application.Features.Sales.BackOffice.Invoices;
using GastroErp.Domain.Enums;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Sales.BackOffice;

[ApiVersion("1.0")]
public class BackOfficeSalesInvoiceController : BaseApiController
{
    private Guid CurrentUserId => HttpContext.RequestServices
        .GetRequiredService<GastroErp.Application.Common.Interfaces.ICurrentUser>().Id ?? Guid.Empty;

    [HttpGet(ApiRoutes.BackOfficeSales.Invoices)]
    [HasPermission(Permissions.BackOfficeSales.View)]
    public async Task<IActionResult> List(
        [FromQuery] PaginationQuery query,
        [FromQuery] BackOfficeSalesDocumentStatus? status = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? branchId = null,
        [FromQuery] BackOfficeSalesInvoiceNature? nature = null,
        [FromQuery] BackOfficeSalesPaymentMode? paymentMode = null,
        [FromQuery] string? search = null,
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null)
        => HandlePagedResult(await Mediator.Send(new GetBackOfficeSalesInvoicesQuery(
            TenantId, status, customerId, branchId, nature, paymentMode, search, from, to,
            query.Page, query.PageSize)));

    [HttpGet($"{ApiRoutes.BackOfficeSales.Invoices}/{{id:guid}}")]
    [HasPermission(Permissions.BackOfficeSales.View)]
    public async Task<IActionResult> GetById(Guid id)
        => HandleResult(await Mediator.Send(new GetBackOfficeSalesInvoiceByIdQuery(id)));

    [HttpPost(ApiRoutes.BackOfficeSales.Invoices)]
    [HasPermission(Permissions.BackOfficeSales.Create)]
    public async Task<IActionResult> Create([FromBody] CreateBackOfficeSalesInvoiceDto dto)
        => HandleResult(await Mediator.Send(new CreateBackOfficeSalesInvoiceCommand(TenantId, dto)));

    [HttpPut($"{ApiRoutes.BackOfficeSales.Invoices}/{{id:guid}}")]
    [HasPermission(Permissions.BackOfficeSales.Update)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBackOfficeSalesInvoiceDto dto)
        => HandleResult(await Mediator.Send(new UpdateBackOfficeSalesInvoiceCommand(id, dto)));

    [HttpPost($"{ApiRoutes.BackOfficeSales.Invoices}/{{id:guid}}/approve")]
    [HasPermission(Permissions.BackOfficeSales.Approve)]
    public async Task<IActionResult> Approve(Guid id)
        => HandleResult(await Mediator.Send(new ApproveBackOfficeSalesInvoiceCommand(id, CurrentUserId)));

    [HttpPost($"{ApiRoutes.BackOfficeSales.Invoices}/{{id:guid}}/unapprove")]
    [HasPermission(Permissions.BackOfficeSales.Unapprove)]
    public async Task<IActionResult> Unapprove(Guid id)
        => HandleResult(await Mediator.Send(new UnapproveBackOfficeSalesInvoiceCommand(id)));

    [HttpPost($"{ApiRoutes.BackOfficeSales.Invoices}/{{id:guid}}/post")]
    [HasPermission(Permissions.BackOfficeSales.Post)]
    public async Task<IActionResult> Post(Guid id)
        => HandleResult(await Mediator.Send(new PostBackOfficeSalesInvoiceCommand(id, CurrentUserId)));

    [HttpPost($"{ApiRoutes.BackOfficeSales.Invoices}/{{id:guid}}/unpost")]
    [HasPermission(Permissions.BackOfficeSales.Unpost)]
    public async Task<IActionResult> Unpost(Guid id)
        => HandleResult(await Mediator.Send(new UnpostBackOfficeSalesInvoiceCommand(id, CurrentUserId)));

    [HttpPost($"{ApiRoutes.BackOfficeSales.Invoices}/{{id:guid}}/cancel")]
    [HasPermission(Permissions.BackOfficeSales.Cancel)]
    public async Task<IActionResult> Cancel(Guid id)
        => HandleResult(await Mediator.Send(new CancelBackOfficeSalesInvoiceCommand(id)));
}
