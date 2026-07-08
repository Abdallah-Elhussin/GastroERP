using Asp.Versioning;
using GastroErp.Application.Features.Organization.Commands;
using GastroErp.Application.Features.Organization.DTOs;
using GastroErp.Application.Features.Organization.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Organization;

/// <summary>
/// Device (POS, KDS) management
/// </summary>
[ApiVersion("1.0")]
public class DeviceController : BaseApiController
{
    /// <summary>
    /// Get all devices
    /// </summary>
    [HttpGet(ApiRoutes.Organization.Devices)]
    [HasPermission(Permissions.Device.View)]
    public async Task<IActionResult> GetDevices([FromQuery] PaginationQuery query)
    {
        return HandlePagedResult(await Mediator.Send(new GetDevicesQuery(TenantId, query.Page, query.PageSize))); // Update later if paginated
    }

    /// <summary>
    /// Get a device by its ID
    /// </summary>
    [HttpGet($"{ApiRoutes.Organization.Devices}/{{id:guid}}")]
    [HasPermission(Permissions.Device.View)]
    public async Task<IActionResult> GetDeviceById(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetDeviceByIdQuery(id)));
    }

    /// <summary>
    /// Create a new device
    /// </summary>
    [HttpPost(ApiRoutes.Organization.Devices)]
    [HasPermission(Permissions.Device.Manage)]
    public async Task<IActionResult> CreateDevice([FromBody] CreateDeviceDto dto)
    {
        return HandleResult(await Mediator.Send(new CreateDeviceCommand(dto)));
    }

    /// <summary>
    /// Update an existing device
    /// </summary>
    [HttpPut($"{ApiRoutes.Organization.Devices}/{{id:guid}}")]
    [HasPermission(Permissions.Device.Manage)]
    public async Task<IActionResult> UpdateDevice(Guid id, [FromBody] UpdateDeviceDto dto)
    {
        return HandleResult(await Mediator.Send(new UpdateDeviceCommand(id, dto)));
    }

    /// <summary>
    /// Link a device to a branch
    /// </summary>
    [HttpPost($"{ApiRoutes.Organization.Devices}/{{id:guid}}/link-branch")]
    [HasPermission(Permissions.Device.Manage)]
    public async Task<IActionResult> LinkBranchDevice(Guid id, [FromBody] LinkBranchDeviceDto dto)
    {
        return HandleResult(await Mediator.Send(new ActivateDeviceCommand(id, dto.BranchId)));
    }

    /// <summary>
    /// Assign a device to a branch
    /// </summary>
    [HttpPost($"{ApiRoutes.Organization.Devices}/{{id:guid}}/assign")]
    [HasPermission(Permissions.Device.Manage)]
    public async Task<IActionResult> AssignDevice(Guid id, [FromBody] Guid branchId)
    {
        return HandleResult(await Mediator.Send(new AssignDeviceCommand(id, branchId, User.Identity?.Name)));
    }

    /// <summary>
    /// Unassign a device from a branch
    /// </summary>
    [HttpPost($"{ApiRoutes.Organization.Devices}/{{id:guid}}/unassign")]
    [HasPermission(Permissions.Device.Manage)]
    public async Task<IActionResult> UnassignDevice(Guid id, [FromBody] Guid branchId)
    {
        return HandleResult(await Mediator.Send(new UnassignDeviceCommand(id, branchId, User.Identity?.Name)));
    }

    /// <summary>
    /// Deactivate a device
    /// </summary>
    [HttpPost($"{ApiRoutes.Organization.Devices}/{{id:guid}}/deactivate")]
    [HasPermission(Permissions.Device.Manage)]
    public async Task<IActionResult> DeactivateDevice(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeactivateDeviceCommand(id)));
    }
}
