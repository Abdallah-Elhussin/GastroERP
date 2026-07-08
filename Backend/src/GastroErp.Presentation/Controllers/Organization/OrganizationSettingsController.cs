using Asp.Versioning;
using GastroErp.Application.Features.Organization.Commands;
using GastroErp.Application.Features.Organization.DTOs;
using GastroErp.Application.Features.Organization.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GastroErp.Presentation.Controllers.Organization;

/// <summary>
/// Organization Settings management
/// </summary>
[ApiVersion("1.0")]
public class OrganizationSettingsController : BaseApiController
{
    /// <summary>
    /// Get organization settings
    /// </summary>
    [HttpGet(ApiRoutes.Organization.Settings)]
    [HasPermission(Permissions.Tenant.View)]
    public async Task<IActionResult> GetSettings()
    {
        return HandleResult(await Mediator.Send(new GetOrganizationSettingsQuery(TenantId)));
    }

    /// <summary>
    /// Update organization settings
    /// </summary>
    [HttpPut(ApiRoutes.Organization.Settings)]
    [HasPermission(Permissions.Tenant.Manage)]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateOrganizationSettingsDto dto)
    {
        return HandleResult(await Mediator.Send(new UpdateOrganizationSettingsCommand(TenantId, dto)));
    }
}
