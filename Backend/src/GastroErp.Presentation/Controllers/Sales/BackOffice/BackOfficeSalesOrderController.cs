using Asp.Versioning;
using GastroErp.Application.Features.Sales.BackOffice.Conversions;
using GastroErp.Application.Features.Sales.BackOffice.Orders;
using GastroErp.Domain.Enums;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Sales.BackOffice;

[ApiVersion("1.0")]
public class BackOfficeSalesOrderController : BaseApiController
{
    private Guid CurrentUserId => HttpContext.RequestServices
        .GetRequiredService<GastroErp.Application.Common.Interfaces.ICurrentUser>().Id ?? Guid.Empty;

    [HttpGet(ApiRoutes.BackOfficeSales.Orders)]
    [HasPermission(Permissions.BackOfficeSales.View)]
    public async Task<IActionResult> List(
        [FromQuery] PaginationQuery query,
        [FromQuery] BackOfficeSalesDocumentStatus? status = null,
        [FromQuery] BackOfficeSalesFulfillmentStatus? fulfillmentStatus = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? branchId = null,
        [FromQuery] string? search = null,
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null)
        => HandlePagedResult(await Mediator.Send(new GetBackOfficeSalesOrdersQuery(
            TenantId, status, fulfillmentStatus, customerId, branchId, search, from, to,
            query.Page, query.PageSize)));

    [HttpGet($"{ApiRoutes.BackOfficeSales.Orders}/{{id:guid}}")]
    [HasPermission(Permissions.BackOfficeSales.View)]
    public async Task<IActionResult> GetById(Guid id)
        => HandleResult(await Mediator.Send(new GetBackOfficeSalesOrderByIdQuery(id)));

    [HttpPost(ApiRoutes.BackOfficeSales.Orders)]
    [HasPermission(Permissions.BackOfficeSales.Create)]
    public async Task<IActionResult> Create([FromBody] CreateBackOfficeSalesOrderDto dto)
        => HandleResult(await Mediator.Send(new CreateBackOfficeSalesOrderCommand(TenantId, dto)));

    [HttpPut($"{ApiRoutes.BackOfficeSales.Orders}/{{id:guid}}")]
    [HasPermission(Permissions.BackOfficeSales.Update)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBackOfficeSalesOrderDto dto)
        => HandleResult(await Mediator.Send(new UpdateBackOfficeSalesOrderCommand(id, dto)));

    [HttpPost($"{ApiRoutes.BackOfficeSales.Orders}/{{id:guid}}/approve")]
    [HasPermission(Permissions.BackOfficeSales.Approve)]
    public async Task<IActionResult> Approve(Guid id)
        => HandleResult(await Mediator.Send(new ApproveBackOfficeSalesOrderCommand(id, CurrentUserId)));

    [HttpPost($"{ApiRoutes.BackOfficeSales.Orders}/{{id:guid}}/unapprove")]
    [HasPermission(Permissions.BackOfficeSales.Unapprove)]
    public async Task<IActionResult> Unapprove(Guid id)
        => HandleResult(await Mediator.Send(new UnapproveBackOfficeSalesOrderCommand(id)));

    [HttpPost($"{ApiRoutes.BackOfficeSales.Orders}/{{id:guid}}/cancel")]
    [HasPermission(Permissions.BackOfficeSales.Cancel)]
    public async Task<IActionResult> Cancel(Guid id)
        => HandleResult(await Mediator.Send(new CancelBackOfficeSalesOrderCommand(id)));

    [HttpPost($"{ApiRoutes.BackOfficeSales.Orders}/{{id:guid}}/close")]
    [HasPermission(Permissions.BackOfficeSales.Update)]
    public async Task<IActionResult> Close(Guid id)
        => HandleResult(await Mediator.Send(new CloseBackOfficeSalesOrderCommand(id)));

    [HttpPost($"{ApiRoutes.BackOfficeSales.Orders}/{{id:guid}}/convert-to-invoice")]
    [HasPermission(Permissions.BackOfficeSales.Create)]
    public async Task<IActionResult> ConvertToInvoice(Guid id, [FromBody] ConvertBackOfficeSalesOrderToInvoiceDto dto)
        => HandleResult(await Mediator.Send(new ConvertOrderToInvoiceCommand(
            id, dto.PaymentMode, dto.Nature, dto.InvoiceDate, dto.WarehouseId, dto.CostCenterId,
            dto.DueDate, dto.InvoiceNumber, dto.Selection)));
}

public record ConvertBackOfficeSalesOrderToInvoiceDto(
    BackOfficeSalesPaymentMode PaymentMode,
    BackOfficeSalesInvoiceNature Nature = BackOfficeSalesInvoiceNature.Inventory,
    DateOnly? InvoiceDate = null,
    Guid? WarehouseId = null,
    Guid? CostCenterId = null,
    DateOnly? DueDate = null,
    string? InvoiceNumber = null,
    IReadOnlyList<ConvertOrderToInvoiceLineDto>? Selection = null);
