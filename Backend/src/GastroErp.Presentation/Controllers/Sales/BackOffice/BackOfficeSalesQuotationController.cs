using Asp.Versioning;
using GastroErp.Application.Features.Sales.BackOffice.Quotations;
using GastroErp.Domain.Enums;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Sales.BackOffice;

[ApiVersion("1.0")]
public class BackOfficeSalesQuotationController : BaseApiController
{
    private Guid CurrentUserId => HttpContext.RequestServices
        .GetRequiredService<GastroErp.Application.Common.Interfaces.ICurrentUser>().Id ?? Guid.Empty;

    [HttpGet(ApiRoutes.BackOfficeSales.Quotations)]
    [HasPermission(Permissions.BackOfficeSales.View)]
    public async Task<IActionResult> List(
        [FromQuery] PaginationQuery query,
        [FromQuery] BackOfficeSalesDocumentStatus? status = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? branchId = null,
        [FromQuery] string? search = null,
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null)
        => HandlePagedResult(await Mediator.Send(new GetBackOfficeSalesQuotationsQuery(
            TenantId, status, customerId, branchId, search, from, to,
            query.Page, query.PageSize)));

    [HttpGet($"{ApiRoutes.BackOfficeSales.Quotations}/{{id:guid}}")]
    [HasPermission(Permissions.BackOfficeSales.View)]
    public async Task<IActionResult> GetById(Guid id)
        => HandleResult(await Mediator.Send(new GetBackOfficeSalesQuotationByIdQuery(id)));

    [HttpPost(ApiRoutes.BackOfficeSales.Quotations)]
    [HasPermission(Permissions.BackOfficeSales.Create)]
    public async Task<IActionResult> Create([FromBody] CreateBackOfficeSalesQuotationDto dto)
        => HandleResult(await Mediator.Send(new CreateBackOfficeSalesQuotationCommand(TenantId, dto)));

    [HttpPut($"{ApiRoutes.BackOfficeSales.Quotations}/{{id:guid}}")]
    [HasPermission(Permissions.BackOfficeSales.Update)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBackOfficeSalesQuotationDto dto)
        => HandleResult(await Mediator.Send(new UpdateBackOfficeSalesQuotationCommand(id, dto)));

    [HttpPost($"{ApiRoutes.BackOfficeSales.Quotations}/{{id:guid}}/approve")]
    [HasPermission(Permissions.BackOfficeSales.Approve)]
    public async Task<IActionResult> Approve(Guid id)
        => HandleResult(await Mediator.Send(new ApproveBackOfficeSalesQuotationCommand(id, CurrentUserId)));

    [HttpPost($"{ApiRoutes.BackOfficeSales.Quotations}/{{id:guid}}/unapprove")]
    [HasPermission(Permissions.BackOfficeSales.Unapprove)]
    public async Task<IActionResult> Unapprove(Guid id)
        => HandleResult(await Mediator.Send(new UnapproveBackOfficeSalesQuotationCommand(id)));

    [HttpPost($"{ApiRoutes.BackOfficeSales.Quotations}/{{id:guid}}/cancel")]
    [HasPermission(Permissions.BackOfficeSales.Cancel)]
    public async Task<IActionResult> Cancel(Guid id)
        => HandleResult(await Mediator.Send(new CancelBackOfficeSalesQuotationCommand(id)));

    [HttpPost($"{ApiRoutes.BackOfficeSales.Quotations}/{{id:guid}}/convert-to-order")]
    [HasPermission(Permissions.BackOfficeSales.Create)]
    public async Task<IActionResult> ConvertToOrder(Guid id, [FromBody] ConvertBackOfficeSalesQuotationToOrderDto dto)
        => HandleResult(await Mediator.Send(new ConvertBackOfficeSalesQuotationToOrderCommand(
            id, CurrentUserId, dto.OrderDate, dto.ExpectedDeliveryDate, dto.OrderNumber)));
}

public record ConvertBackOfficeSalesQuotationToOrderDto(
    DateOnly? OrderDate = null,
    DateOnly? ExpectedDeliveryDate = null,
    string? OrderNumber = null);
