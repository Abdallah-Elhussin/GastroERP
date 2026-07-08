using Asp.Versioning;
using GastroErp.Application.Features.Inventory.Commands;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Application.Features.Inventory.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Inventory;

/// <summary>
/// Supplier management
/// </summary>
[ApiVersion("1.0")]
public class SupplierController : BaseApiController
{
    [HttpGet(ApiRoutes.Inventory.Suppliers)]
    [HasPermission(Permissions.Supplier.View)]
    public async Task<IActionResult> GetSuppliers([FromQuery] PaginationQuery query)
    {
        return HandlePagedResult(await Mediator.Send(new GetSuppliersQuery(TenantId, null, null, query.Page, query.PageSize)));
    }

    [HttpGet($"{ApiRoutes.Inventory.Suppliers}/{{id:guid}}")]
    [HasPermission(Permissions.Supplier.View)]
    public async Task<IActionResult> GetSupplierById(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetSupplierByIdQuery(id)));
    }

    [HttpPost(ApiRoutes.Inventory.Suppliers)]
    [HasPermission(Permissions.Supplier.Create)]
    public async Task<IActionResult> CreateSupplier([FromBody] CreateSupplierDto dto)
    {
        return HandleResult(await Mediator.Send(new CreateSupplierCommand(dto)));
    }

    [HttpPut($"{ApiRoutes.Inventory.Suppliers}/{{id:guid}}")]
    [HasPermission(Permissions.Supplier.Update)]
    public async Task<IActionResult> UpdateSupplier(Guid id, [FromBody] UpdateSupplierDto dto)
    {
        return HandleResult(await Mediator.Send(new UpdateSupplierCommand(id, dto)));
    }
}
