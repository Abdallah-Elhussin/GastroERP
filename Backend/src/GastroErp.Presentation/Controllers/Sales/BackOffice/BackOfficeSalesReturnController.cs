using Asp.Versioning;
using GastroErp.Application.Features.Sales.BackOffice.Returns;
using GastroErp.Domain.Enums;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Sales.BackOffice;

[ApiVersion("1.0")]
public class BackOfficeSalesReturnController : BaseApiController
{
    private Guid CurrentUserId => HttpContext.RequestServices
        .GetRequiredService<GastroErp.Application.Common.Interfaces.ICurrentUser>().Id ?? Guid.Empty;

    [HttpGet(ApiRoutes.BackOfficeSales.Returns)]
    [HasPermission(Permissions.BackOfficeSales.View)]
    public async Task<IActionResult> List(
        [FromQuery] PaginationQuery query,
        [FromQuery] BackOfficeSalesDocumentStatus? status = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? invoiceId = null,
        [FromQuery] Guid? branchId = null,
        [FromQuery] string? search = null,
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null)
        => HandlePagedResult(await Mediator.Send(new GetBackOfficeSalesReturnsQuery(
            TenantId, status, customerId, invoiceId, branchId, search, from, to,
            query.Page, query.PageSize)));

    [HttpGet($"{ApiRoutes.BackOfficeSales.Returns}/{{id:guid}}")]
    [HasPermission(Permissions.BackOfficeSales.View)]
    public async Task<IActionResult> GetById(Guid id)
        => HandleResult(await Mediator.Send(new GetBackOfficeSalesReturnByIdQuery(id)));

    [HttpPost(ApiRoutes.BackOfficeSales.Returns)]
    [HasPermission(Permissions.BackOfficeSales.Create)]
    public async Task<IActionResult> Create([FromBody] CreateBackOfficeSalesReturnDto dto)
        => HandleResult(await Mediator.Send(new CreateBackOfficeSalesReturnCommand(TenantId, dto)));

    [HttpPut($"{ApiRoutes.BackOfficeSales.Returns}/{{id:guid}}")]
    [HasPermission(Permissions.BackOfficeSales.Update)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBackOfficeSalesReturnDto dto)
        => HandleResult(await Mediator.Send(new UpdateBackOfficeSalesReturnCommand(id, dto)));

    [HttpPost($"{ApiRoutes.BackOfficeSales.Returns}/{{id:guid}}/approve")]
    [HasPermission(Permissions.BackOfficeSales.Approve)]
    public async Task<IActionResult> Approve(Guid id)
        => HandleResult(await Mediator.Send(new ApproveBackOfficeSalesReturnCommand(id, CurrentUserId)));

    [HttpPost($"{ApiRoutes.BackOfficeSales.Returns}/{{id:guid}}/unapprove")]
    [HasPermission(Permissions.BackOfficeSales.Unapprove)]
    public async Task<IActionResult> Unapprove(Guid id)
        => HandleResult(await Mediator.Send(new UnapproveBackOfficeSalesReturnCommand(id)));

    [HttpPost($"{ApiRoutes.BackOfficeSales.Returns}/{{id:guid}}/post")]
    [HasPermission(Permissions.BackOfficeSales.Post)]
    public async Task<IActionResult> Post(Guid id)
        => HandleResult(await Mediator.Send(new PostBackOfficeSalesReturnCommand(id, CurrentUserId)));

    [HttpPost($"{ApiRoutes.BackOfficeSales.Returns}/{{id:guid}}/unpost")]
    [HasPermission(Permissions.BackOfficeSales.Unpost)]
    public async Task<IActionResult> Unpost(Guid id)
        => HandleResult(await Mediator.Send(new UnpostBackOfficeSalesReturnCommand(id, CurrentUserId)));

    [HttpPost($"{ApiRoutes.BackOfficeSales.Returns}/{{id:guid}}/cancel")]
    [HasPermission(Permissions.BackOfficeSales.Cancel)]
    public async Task<IActionResult> Cancel(Guid id)
        => HandleResult(await Mediator.Send(new CancelBackOfficeSalesReturnCommand(id)));
}
