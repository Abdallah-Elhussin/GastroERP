using Asp.Versioning;
using GastroErp.Application.Features.Inventory.Commands;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Application.Features.Inventory.Queries;
using GastroErp.Domain.Enums;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Inventory;

/// <summary>
/// Purchase Return (مرتجع المشتريات) — BeforeInvoice / AfterInvoice / Direct.
/// </summary>
[ApiVersion("1.0")]
public class PurchaseReturnController : BaseApiController
{
    private Guid CurrentUserId => HttpContext.RequestServices
        .GetRequiredService<GastroErp.Application.Common.Interfaces.ICurrentUser>().Id ?? Guid.Empty;

    [HttpGet(ApiRoutes.Inventory.PurchaseReturns)]
    [HasPermission(Permissions.Inventory.View)]
    public async Task<IActionResult> GetPurchaseReturns(
        [FromQuery] PaginationQuery query,
        [FromQuery] Guid? supplierId = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] PurchaseReturnType? returnType = null,
        [FromQuery] PurchasingDocumentStatus? status = null,
        [FromQuery] string? search = null,
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null)
        => HandlePagedResult(await Mediator.Send(new GetPurchaseReturnsQuery(
            TenantId, supplierId, warehouseId, returnType, status, search, from, to, query.Page, query.PageSize)));

    [HttpGet($"{ApiRoutes.Inventory.PurchaseReturns}/{{id:guid}}")]
    [HasPermission(Permissions.Inventory.View)]
    public async Task<IActionResult> GetPurchaseReturnById(Guid id)
        => HandleResult(await Mediator.Send(new GetPurchaseReturnByIdQuery(id)));

    [HttpGet($"{ApiRoutes.Inventory.PurchaseReturns}/preview-from-grn/{{goodsReceiptId:guid}}")]
    [HasPermission(Permissions.Inventory.View)]
    public async Task<IActionResult> PreviewFromGrn(Guid goodsReceiptId)
        => HandleResult(await Mediator.Send(new PreviewPurchaseReturnFromGrnQuery(TenantId, goodsReceiptId)));

    [HttpGet($"{ApiRoutes.Inventory.PurchaseReturns}/preview-from-invoice/{{purchaseInvoiceId:guid}}")]
    [HasPermission(Permissions.Inventory.View)]
    public async Task<IActionResult> PreviewFromInvoice(Guid purchaseInvoiceId)
        => HandleResult(await Mediator.Send(new PreviewPurchaseReturnFromInvoiceQuery(TenantId, purchaseInvoiceId)));

    [HttpGet($"{ApiRoutes.Inventory.PurchaseReturns}/reasons")]
    [HasPermission(Permissions.Inventory.View)]
    public async Task<IActionResult> GetReasons([FromQuery] bool activeOnly = true)
        => HandleResult(await Mediator.Send(new GetPurchaseReturnReasonsQuery(TenantId, activeOnly)));

    [HttpPost($"{ApiRoutes.Inventory.PurchaseReturns}/reasons/seed")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> SeedReasons()
        => HandleResult(await Mediator.Send(new SeedPurchaseReturnReasonsCommand(TenantId)));

    [HttpPost(ApiRoutes.Inventory.PurchaseReturns)]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> CreatePurchaseReturn([FromBody] CreatePurchaseReturnDto dto)
        => HandleResult(await Mediator.Send(new CreatePurchaseReturnCommand(TenantId, dto)));

    [HttpPut($"{ApiRoutes.Inventory.PurchaseReturns}/{{id:guid}}")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePurchaseReturnDto dto)
        => HandleResult(await Mediator.Send(new UpdatePurchaseReturnCommand(id, dto)));

    [HttpPost($"{ApiRoutes.Inventory.PurchaseReturns}/{{id:guid}}/lines")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> AddPurchaseReturnLine(Guid id, [FromBody] AddPurchaseReturnLineDto dto)
        => HandleResult(await Mediator.Send(new AddPurchaseReturnLineCommand(id, dto)));

    [HttpPost($"{ApiRoutes.Inventory.PurchaseReturns}/{{id:guid}}/approve")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> ApprovePurchaseReturn(Guid id)
        => HandleResult(await Mediator.Send(new ApprovePurchaseReturnCommand(id)));

    [HttpPost($"{ApiRoutes.Inventory.PurchaseReturns}/{{id:guid}}/post")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> Post(Guid id)
        => HandleResult(await Mediator.Send(new PostPurchaseReturnCommand(id, CurrentUserId)));

    [HttpPost($"{ApiRoutes.Inventory.PurchaseReturns}/{{id:guid}}/unpost")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> Unpost(Guid id)
        => HandleResult(await Mediator.Send(new UnpostPurchaseReturnCommand(id, CurrentUserId)));

    [HttpPost($"{ApiRoutes.Inventory.PurchaseReturns}/{{id:guid}}/cancel")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> Cancel(Guid id)
        => HandleResult(await Mediator.Send(new CancelPurchaseReturnCommand(id)));
}
