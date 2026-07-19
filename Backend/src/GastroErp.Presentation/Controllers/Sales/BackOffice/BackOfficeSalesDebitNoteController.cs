using Asp.Versioning;
using GastroErp.Application.Features.Sales.BackOffice.DebitNotes;
using GastroErp.Domain.Enums;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Sales.BackOffice;

[ApiVersion("1.0")]
public class BackOfficeSalesDebitNoteController : BaseApiController
{
    private Guid CurrentUserId => HttpContext.RequestServices
        .GetRequiredService<GastroErp.Application.Common.Interfaces.ICurrentUser>().Id ?? Guid.Empty;

    [HttpGet(ApiRoutes.BackOfficeSales.DebitNotes)]
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
        => HandlePagedResult(await Mediator.Send(new GetBackOfficeSalesDebitNotesQuery(
            TenantId, status, customerId, invoiceId, branchId, search, from, to,
            query.Page, query.PageSize)));

    [HttpGet($"{ApiRoutes.BackOfficeSales.DebitNotes}/{{id:guid}}")]
    [HasPermission(Permissions.BackOfficeSales.View)]
    public async Task<IActionResult> GetById(Guid id)
        => HandleResult(await Mediator.Send(new GetBackOfficeSalesDebitNoteByIdQuery(id)));

    [HttpPost(ApiRoutes.BackOfficeSales.DebitNotes)]
    [HasPermission(Permissions.BackOfficeSales.Create)]
    public async Task<IActionResult> Create([FromBody] CreateBackOfficeSalesDebitNoteDto dto)
        => HandleResult(await Mediator.Send(new CreateBackOfficeSalesDebitNoteCommand(TenantId, dto)));

    [HttpPut($"{ApiRoutes.BackOfficeSales.DebitNotes}/{{id:guid}}")]
    [HasPermission(Permissions.BackOfficeSales.Update)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBackOfficeSalesDebitNoteDto dto)
        => HandleResult(await Mediator.Send(new UpdateBackOfficeSalesDebitNoteCommand(id, dto)));

    [HttpPost($"{ApiRoutes.BackOfficeSales.DebitNotes}/{{id:guid}}/approve")]
    [HasPermission(Permissions.BackOfficeSales.Approve)]
    public async Task<IActionResult> Approve(Guid id)
        => HandleResult(await Mediator.Send(new ApproveBackOfficeSalesDebitNoteCommand(id, CurrentUserId)));

    [HttpPost($"{ApiRoutes.BackOfficeSales.DebitNotes}/{{id:guid}}/unapprove")]
    [HasPermission(Permissions.BackOfficeSales.Unapprove)]
    public async Task<IActionResult> Unapprove(Guid id)
        => HandleResult(await Mediator.Send(new UnapproveBackOfficeSalesDebitNoteCommand(id)));

    [HttpPost($"{ApiRoutes.BackOfficeSales.DebitNotes}/{{id:guid}}/post")]
    [HasPermission(Permissions.BackOfficeSales.Post)]
    public async Task<IActionResult> Post(Guid id)
        => HandleResult(await Mediator.Send(new PostBackOfficeSalesDebitNoteCommand(id, CurrentUserId)));

    [HttpPost($"{ApiRoutes.BackOfficeSales.DebitNotes}/{{id:guid}}/unpost")]
    [HasPermission(Permissions.BackOfficeSales.Unpost)]
    public async Task<IActionResult> Unpost(Guid id)
        => HandleResult(await Mediator.Send(new UnpostBackOfficeSalesDebitNoteCommand(id, CurrentUserId)));

    [HttpPost($"{ApiRoutes.BackOfficeSales.DebitNotes}/{{id:guid}}/cancel")]
    [HasPermission(Permissions.BackOfficeSales.Cancel)]
    public async Task<IActionResult> Cancel(Guid id)
        => HandleResult(await Mediator.Send(new CancelBackOfficeSalesDebitNoteCommand(id)));
}
