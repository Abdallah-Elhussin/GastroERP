using Asp.Versioning;
using GastroErp.Application.Features.Sales.BackOffice.DeliveryNotes;
using GastroErp.Domain.Enums;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Sales.BackOffice;

[ApiVersion("1.0")]
public class BackOfficeSalesDeliveryNoteController : BaseApiController
{
    private Guid CurrentUserId => HttpContext.RequestServices
        .GetRequiredService<GastroErp.Application.Common.Interfaces.ICurrentUser>().Id ?? Guid.Empty;

    [HttpGet(ApiRoutes.BackOfficeSales.DeliveryNotes)]
    [HasPermission(Permissions.BackOfficeSales.View)]
    public async Task<IActionResult> List(
        [FromQuery] PaginationQuery query,
        [FromQuery] BackOfficeSalesDocumentStatus? status = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? orderId = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] Guid? branchId = null,
        [FromQuery] string? search = null,
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null)
        => HandlePagedResult(await Mediator.Send(new GetBackOfficeSalesDeliveryNotesQuery(
            TenantId, status, customerId, orderId, warehouseId, branchId, search, from, to,
            query.Page, query.PageSize)));

    [HttpGet($"{ApiRoutes.BackOfficeSales.DeliveryNotes}/{{id:guid}}")]
    [HasPermission(Permissions.BackOfficeSales.View)]
    public async Task<IActionResult> GetById(Guid id)
        => HandleResult(await Mediator.Send(new GetBackOfficeSalesDeliveryNoteByIdQuery(id)));

    [HttpPost(ApiRoutes.BackOfficeSales.DeliveryNotes)]
    [HasPermission(Permissions.BackOfficeSales.Create)]
    public async Task<IActionResult> Create([FromBody] CreateBackOfficeSalesDeliveryNoteDto dto)
        => HandleResult(await Mediator.Send(new CreateBackOfficeSalesDeliveryNoteCommand(TenantId, dto)));

    [HttpPut($"{ApiRoutes.BackOfficeSales.DeliveryNotes}/{{id:guid}}")]
    [HasPermission(Permissions.BackOfficeSales.Update)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBackOfficeSalesDeliveryNoteDto dto)
        => HandleResult(await Mediator.Send(new UpdateBackOfficeSalesDeliveryNoteCommand(id, dto)));

    [HttpPost($"{ApiRoutes.BackOfficeSales.DeliveryNotes}/{{id:guid}}/approve")]
    [HasPermission(Permissions.BackOfficeSales.Approve)]
    public async Task<IActionResult> Approve(Guid id)
        => HandleResult(await Mediator.Send(new ApproveBackOfficeSalesDeliveryNoteCommand(id, CurrentUserId)));

    [HttpPost($"{ApiRoutes.BackOfficeSales.DeliveryNotes}/{{id:guid}}/unapprove")]
    [HasPermission(Permissions.BackOfficeSales.Unapprove)]
    public async Task<IActionResult> Unapprove(Guid id)
        => HandleResult(await Mediator.Send(new UnapproveBackOfficeSalesDeliveryNoteCommand(id)));

    [HttpPost($"{ApiRoutes.BackOfficeSales.DeliveryNotes}/{{id:guid}}/post")]
    [HasPermission(Permissions.BackOfficeSales.Post)]
    public async Task<IActionResult> Post(Guid id)
        => HandleResult(await Mediator.Send(new PostBackOfficeSalesDeliveryNoteCommand(id, CurrentUserId)));

    [HttpPost($"{ApiRoutes.BackOfficeSales.DeliveryNotes}/{{id:guid}}/unpost")]
    [HasPermission(Permissions.BackOfficeSales.Unpost)]
    public async Task<IActionResult> Unpost(Guid id)
        => HandleResult(await Mediator.Send(new UnpostBackOfficeSalesDeliveryNoteCommand(id, CurrentUserId)));

    [HttpPost($"{ApiRoutes.BackOfficeSales.DeliveryNotes}/{{id:guid}}/cancel")]
    [HasPermission(Permissions.BackOfficeSales.Cancel)]
    public async Task<IActionResult> Cancel(Guid id)
        => HandleResult(await Mediator.Send(new CancelBackOfficeSalesDeliveryNoteCommand(id)));
}
