using Asp.Versioning;
using GastroErp.Application.Features.Identity.Commands.Roles;
using GastroErp.Application.Features.Identity.DTOs;
using GastroErp.Application.Features.Identity.Queries.Roles;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Identity;

/// <summary>
/// Role and Permission management endpoints
/// </summary>
[ApiVersion("1.0")]
[Authorize]
public class RoleController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetRoles()
    {
        return HandleResult(await Mediator.Send(new GetRolesQuery()));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetRoleById(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetRoleByIdQuery(id)));
    }

    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto request)
    {
        return HandleResult(await Mediator.Send(new CreateRoleCommand(request)));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleDto request)
    {
        return HandleResult(await Mediator.Send(new UpdateRoleCommand(id, request)));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteRole(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeleteRoleCommand(id)));
    }

    [HttpGet("permissions")]
    public async Task<IActionResult> GetPermissions()
    {
        return HandleResult(await Mediator.Send(new GetPermissionsQuery()));
    }

    [HttpPost("{id:guid}/permissions/assign")]
    public async Task<IActionResult> AssignPermissions(Guid id, [FromBody] List<Guid> permissionIds)
    {
        return HandleResult(await Mediator.Send(new AssignPermissionsCommand(id, permissionIds)));
    }

    [HttpPost("{id:guid}/permissions/remove")]
    public async Task<IActionResult> RemovePermissions(Guid id, [FromBody] List<Guid> permissionIds)
    {
        return HandleResult(await Mediator.Send(new RemovePermissionsCommand(id, permissionIds)));
    }

    [HttpPost("users/{userId:guid}/roles/assign/{roleId:guid}")]
    public async Task<IActionResult> AssignRoleToUser(Guid userId, Guid roleId)
    {
        return HandleResult(await Mediator.Send(new AssignRoleToUserCommand(userId, roleId)));
    }

    [HttpPost("users/{userId:guid}/roles/remove/{roleId:guid}")]
    public async Task<IActionResult> RemoveRoleFromUser(Guid userId, Guid roleId)
    {
        return HandleResult(await Mediator.Send(new RemoveRoleFromUserCommand(userId, roleId)));
    }
}
