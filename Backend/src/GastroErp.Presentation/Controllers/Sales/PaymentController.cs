using Asp.Versioning;
using GastroErp.Application.Features.Sales.Commands;
using GastroErp.Application.Features.Sales.DTOs;
using GastroErp.Application.Features.Sales.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Sales;

[ApiVersion("1.0")]
public class PaymentController : BaseApiController
{
    private Guid CurrentUserId => HttpContext.RequestServices
        .GetRequiredService<GastroErp.Application.Common.Interfaces.ICurrentUser>().Id ?? Guid.Empty;

    [HttpGet(ApiRoutes.Sales.Payments)]
    [HasPermission(Permissions.Payments.View)]
    public async Task<IActionResult> GetPayments([FromQuery] PaymentFilterDto filter)
        => HandlePagedResult(await Mediator.Send(new GetPaymentsQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Sales.Payments}/{{id:guid}}")]
    [HasPermission(Permissions.Payments.View)]
    public async Task<IActionResult> GetPaymentById(Guid id)
        => HandleResult(await Mediator.Send(new GetPaymentByIdQuery(id)));

    [HttpGet($"{ApiRoutes.Sales.Payments}/order/{{orderId:guid}}")]
    [HasPermission(Permissions.Payments.View)]
    public async Task<IActionResult> GetPaymentsByOrder(Guid orderId)
        => HandleResult(await Mediator.Send(new GetPaymentsByOrderQuery(orderId)));

    [HttpPost(ApiRoutes.Sales.Payments)]
    [HasPermission(Permissions.Payments.Create)]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentDto dto)
        => HandleResult(await Mediator.Send(new CreatePaymentCommand(TenantId, CurrentUserId, dto)));

    [HttpPost($"{ApiRoutes.Sales.Payments}/{{id:guid}}/refund")]
    [HasPermission(Permissions.Payments.Refund)]
    public async Task<IActionResult> RefundPayment(Guid id, [FromBody] RefundPaymentDto dto)
        => HandleResult(await Mediator.Send(new RefundPaymentCommand(id, CurrentUserId, dto)));

    [HttpPost($"{ApiRoutes.Sales.Payments}/{{id:guid}}/void")]
    [HasPermission(Permissions.Payments.Void)]
    public async Task<IActionResult> VoidPayment(Guid id, [FromBody] VoidPaymentDto dto)
        => HandleResult(await Mediator.Send(new VoidPaymentCommand(id, CurrentUserId, dto)));
}
