using Asp.Versioning;
using GastroErp.Application.Features.Organization.Commands;
using GastroErp.Application.Features.Organization.DTOs;
using GastroErp.Application.Features.Organization.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Organization;

/// <summary>
/// Tenants (Organization) management
/// </summary>
[ApiVersion("1.0")]
public class OrganizationController : BaseApiController
{
    /// <summary>
    /// Get a paginated list of tenants
    /// </summary>
    [HttpGet(ApiRoutes.Organization.Tenants)]
    [HasPermission(Permissions.Organization.View)]
    public async Task<IActionResult> GetTenants([FromQuery] PaginationQuery query)
    {
        return HandlePagedResult(await Mediator.Send(new GetTenantsQuery(query.Page, query.PageSize)));
    }

    /// <summary>
    /// Get a tenant by its ID
    /// </summary>
    [HttpGet($"{ApiRoutes.Organization.Tenants}/{{id:guid}}")]
    [HasPermission(Permissions.Organization.View)]
    public async Task<IActionResult> GetTenantById(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetTenantByIdQuery(id)));
    }

    /// <summary>
    /// Create a new tenant
    /// </summary>
    [HttpPost(ApiRoutes.Organization.Tenants)]
    [HasPermission(Permissions.Organization.Create)]
    public async Task<IActionResult> CreateTenant([FromBody] CreateTenantDto dto)
    {
        return HandleResult(await Mediator.Send(new CreateTenantCommand(dto)));
    }

    /// <summary>
    /// Update an existing tenant
    /// </summary>
    [HttpPut($"{ApiRoutes.Organization.Tenants}/{{id:guid}}")]
    [HasPermission(Permissions.Organization.Update)]
    public async Task<IActionResult> UpdateTenant(Guid id, [FromBody] UpdateTenantDto dto)
    {
        return HandleResult(await Mediator.Send(new UpdateTenantCommand(id, dto)));
    }

    /// <summary>
    /// Deactivate a tenant
    /// </summary>
    [HttpDelete($"{ApiRoutes.Organization.Tenants}/{{id:guid}}")]
    [HasPermission(Permissions.Organization.Delete)]
    public async Task<IActionResult> DeactivateTenant(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeleteTenantCommand(id, "Deactivated via API")));
    }
}
