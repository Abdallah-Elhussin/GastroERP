using Asp.Versioning;
using GastroErp.Application.Features.Inventory.Commands;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Application.Features.Inventory.Queries;
using GastroErp.Domain.Enums;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Inventory;

[ApiVersion("1.0")]
public class SupplierController : BaseApiController
{
    [HttpGet(ApiRoutes.Inventory.Suppliers)]
    [HasPermission(Permissions.Supplier.View)]
    public async Task<IActionResult> GetSuppliers(
        [FromQuery] PaginationQuery query,
        [FromQuery] bool? isActive = null,
        [FromQuery] bool? isPreferred = null,
        [FromQuery] bool? isBlacklisted = null,
        [FromQuery] bool? hasBalance = null,
        [FromQuery] bool? overCreditLimit = null,
        [FromQuery] SupplierCategory? category = null,
        [FromQuery] string? city = null,
        [FromQuery] string? country = null,
        [FromQuery] string? search = null,
        [FromQuery] string? code = null)
        => HandlePagedResult(await Mediator.Send(new GetSuppliersQuery(
            TenantId, isActive, isPreferred, isBlacklisted, hasBalance, overCreditLimit,
            category, city, country, search, code, query.Page, query.PageSize)));

    [HttpGet($"{ApiRoutes.Inventory.Suppliers}/{{id:guid}}")]
    [HasPermission(Permissions.Supplier.View)]
    public async Task<IActionResult> GetSupplierById(Guid id, [FromQuery] bool includeStats = true)
        => HandleResult(await Mediator.Send(new GetSupplierByIdQuery(id, includeStats)));

    [HttpGet($"{ApiRoutes.Inventory.Suppliers}/{{id:guid}}/purchasing-defaults")]
    [HasPermission(Permissions.Supplier.View)]
    public async Task<IActionResult> GetPurchasingDefaults(Guid id)
        => HandleResult(await Mediator.Send(new GetSupplierPurchasingDefaultsQuery(id)));

    [HttpPost(ApiRoutes.Inventory.Suppliers)]
    [HasPermission(Permissions.Supplier.Create)]
    public async Task<IActionResult> CreateSupplier([FromBody] CreateSupplierDto dto)
        => HandleResult(await Mediator.Send(new CreateSupplierCommand(dto with { TenantId = TenantId })));

    [HttpPut($"{ApiRoutes.Inventory.Suppliers}/{{id:guid}}")]
    [HasPermission(Permissions.Supplier.Update)]
    public async Task<IActionResult> UpdateSupplier(Guid id, [FromBody] UpdateSupplierDto dto)
        => HandleResult(await Mediator.Send(new UpdateSupplierCommand(id, dto)));

    [HttpPut($"{ApiRoutes.Inventory.Suppliers}/{{id:guid}}/master")]
    [HasPermission(Permissions.Supplier.Update)]
    public async Task<IActionResult> UpsertMaster(Guid id, [FromBody] UpsertSupplierMasterDto dto)
        => HandleResult(await Mediator.Send(new UpsertSupplierMasterCommand(id, dto)));

    [HttpPut($"{ApiRoutes.Inventory.Suppliers}/{{id:guid}}/financial")]
    [HasPermission(Permissions.Supplier.UpdateFinancial)]
    public async Task<IActionResult> UpdateFinancial(Guid id, [FromBody] UpdateSupplierFinancialDto dto)
        => HandleResult(await Mediator.Send(new UpdateSupplierFinancialCommand(id, dto)));

    [HttpPost($"{ApiRoutes.Inventory.Suppliers}/{{id:guid}}/activate")]
    [HasPermission(Permissions.Supplier.Activate)]
    public async Task<IActionResult> Activate(Guid id)
        => HandleResult(await Mediator.Send(new ActivateSupplierCommand(id)));

    [HttpPost($"{ApiRoutes.Inventory.Suppliers}/{{id:guid}}/deactivate")]
    [HasPermission(Permissions.Supplier.Deactivate)]
    public async Task<IActionResult> Deactivate(Guid id)
        => HandleResult(await Mediator.Send(new DeactivateSupplierCommand(id)));

    [HttpPost($"{ApiRoutes.Inventory.Suppliers}/{{id:guid}}/blacklist")]
    [HasPermission(Permissions.Supplier.Blacklist)]
    public async Task<IActionResult> Blacklist(Guid id, [FromBody] BlacklistRequest? body = null)
        => HandleResult(await Mediator.Send(new BlacklistSupplierCommand(id, body?.Reason)));

    [HttpPost($"{ApiRoutes.Inventory.Suppliers}/{{id:guid}}/clear-blacklist")]
    [HasPermission(Permissions.Supplier.ClearBlacklist)]
    public async Task<IActionResult> ClearBlacklist(Guid id)
        => HandleResult(await Mediator.Send(new ClearSupplierBlacklistCommand(id)));

    [HttpDelete($"{ApiRoutes.Inventory.Suppliers}/{{id:guid}}")]
    [HasPermission(Permissions.Supplier.Delete)]
    public async Task<IActionResult> Delete(Guid id)
        => HandleResult(await Mediator.Send(new DeleteSupplierCommand(id)));

    [HttpPost($"{ApiRoutes.Inventory.Suppliers}/{{supplierId:guid}}/contacts")]
    [HasPermission(Permissions.Supplier.Update)]
    public async Task<IActionResult> AddContact(Guid supplierId, [FromBody] AddSupplierContactDto dto)
        => HandleResult(await Mediator.Send(new AddSupplierContactCommand(supplierId, dto)));

    [HttpDelete($"{ApiRoutes.Inventory.Suppliers}/{{supplierId:guid}}/contacts/{{contactId:guid}}")]
    [HasPermission(Permissions.Supplier.Update)]
    public async Task<IActionResult> RemoveContact(Guid supplierId, Guid contactId)
        => HandleResult(await Mediator.Send(new RemoveSupplierContactCommand(supplierId, contactId)));

    [HttpPost($"{ApiRoutes.Inventory.Suppliers}/{{supplierId:guid}}/payment-methods/{{paymentMethodId:guid}}/default")]
    [HasPermission(Permissions.Supplier.Update)]
    public async Task<IActionResult> SetDefaultPaymentMethod(Guid supplierId, Guid paymentMethodId)
        => HandleResult(await Mediator.Send(new SetSupplierDefaultPaymentMethodCommand(supplierId, paymentMethodId)));

    [HttpDelete($"{ApiRoutes.Inventory.Suppliers}/{{supplierId:guid}}/payment-methods/{{paymentMethodId:guid}}")]
    [HasPermission(Permissions.Supplier.Update)]
    public async Task<IActionResult> RemovePaymentMethod(Guid supplierId, Guid paymentMethodId)
        => HandleResult(await Mediator.Send(new RemoveSupplierPaymentMethodCommand(supplierId, paymentMethodId)));

    [HttpDelete($"{ApiRoutes.Inventory.Suppliers}/{{supplierId:guid}}/attachments/{{attachmentId:guid}}")]
    [HasPermission(Permissions.Supplier.ManageAttachments)]
    public async Task<IActionResult> RemoveAttachment(Guid supplierId, Guid attachmentId)
        => HandleResult(await Mediator.Send(new RemoveSupplierAttachmentCommand(supplierId, attachmentId)));
}

public record BlacklistRequest(string? Reason);
