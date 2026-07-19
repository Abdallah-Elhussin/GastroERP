using Asp.Versioning;
using GastroErp.Application.Features.Inventory.Commands;
using GastroErp.Application.Features.Inventory.Queries;
using GastroErp.Domain.Enums;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Inventory;

[ApiVersion("1.0")]
public class PurchaseInvoiceController : BaseApiController
{
    private Guid CurrentUserId => HttpContext.RequestServices
        .GetRequiredService<GastroErp.Application.Common.Interfaces.ICurrentUser>().Id ?? Guid.Empty;

    [HttpGet(ApiRoutes.Inventory.PurchaseInvoices)]
    [HasPermission(Permissions.Inventory.View)]
    public async Task<IActionResult> List(
        [FromQuery] PaginationQuery query,
        [FromQuery] PurchaseInvoiceKind? kind = null,
        [FromQuery] PurchasingDocumentStatus? status = null,
        [FromQuery] Guid? supplierId = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] PurchaseInvoicePaymentMode? paymentMode = null,
        [FromQuery] DirectPurchaseNature? nature = null,
        [FromQuery] string? search = null,
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null)
        => HandlePagedResult(await Mediator.Send(new GetPurchaseInvoicesQuery(
            TenantId, kind, status, supplierId, warehouseId, paymentMode, nature, search, from, to,
            query.Page, query.PageSize)));

    [HttpGet($"{ApiRoutes.Inventory.PurchaseInvoices}/{{id:guid}}")]
    [HasPermission(Permissions.Inventory.View)]
    public async Task<IActionResult> GetById(Guid id)
        => HandleResult(await Mediator.Send(new GetPurchaseInvoiceByIdQuery(id)));

    [HttpPost(ApiRoutes.Inventory.PurchaseInvoices)]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseInvoiceDto dto)
        => HandleResult(await Mediator.Send(new CreatePurchaseInvoiceCommand(TenantId, dto)));

    [HttpPut($"{ApiRoutes.Inventory.PurchaseInvoices}/{{id:guid}}")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePurchaseInvoiceDto dto)
        => HandleResult(await Mediator.Send(new UpdatePurchaseInvoiceCommand(id, dto)));

    [HttpPost($"{ApiRoutes.Inventory.PurchaseInvoices}/{{id:guid}}/approve")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> Approve(Guid id)
        => HandleResult(await Mediator.Send(new ApprovePurchaseInvoiceCommand(id)));

    [HttpPost($"{ApiRoutes.Inventory.PurchaseInvoices}/{{id:guid}}/post")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> Post(Guid id)
        => HandleResult(await Mediator.Send(new PostPurchaseInvoiceCommand(id, CurrentUserId)));

    [HttpPost($"{ApiRoutes.Inventory.PurchaseInvoices}/{{id:guid}}/unpost")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> Unpost(Guid id)
        => HandleResult(await Mediator.Send(new UnpostPurchaseInvoiceCommand(id, CurrentUserId)));

    [HttpPost($"{ApiRoutes.Inventory.PurchaseInvoices}/{{id:guid}}/cancel")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> Cancel(Guid id)
        => HandleResult(await Mediator.Send(new CancelPurchaseInvoiceCommand(id)));
}
