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
    [HttpGet(ApiRoutes.Identity.Roles)]
    public async Task<IActionResult> GetRoles()
        => HandleResult(await Mediator.Send(new GetRolesQuery()));

    [HttpGet($"{ApiRoutes.Identity.Roles}/{{id:guid}}")]
    public async Task<IActionResult> GetRoleById(Guid id)
        => HandleResult(await Mediator.Send(new GetRoleByIdQuery(id)));

    [HttpPost(ApiRoutes.Identity.Roles)]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto request)
        => HandleResult(await Mediator.Send(new CreateRoleCommand(request)));

    [HttpPut($"{ApiRoutes.Identity.Roles}/{{id:guid}}")]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleDto request)
        => HandleResult(await Mediator.Send(new UpdateRoleCommand(id, request)));

    [HttpDelete($"{ApiRoutes.Identity.Roles}/{{id:guid}}")]
    public async Task<IActionResult> DeleteRole(Guid id)
        => HandleResult(await Mediator.Send(new DeleteRoleCommand(id)));

    [HttpGet($"{ApiRoutes.Identity.Roles}/permissions")]
    public async Task<IActionResult> GetPermissions()
        => HandleResult(await Mediator.Send(new GetPermissionsQuery()));

    [HttpPost($"{ApiRoutes.Identity.Roles}/{{id:guid}}/permissions/assign")]
    public async Task<IActionResult> AssignPermissions(Guid id, [FromBody] List<Guid> permissionIds)
        => HandleResult(await Mediator.Send(new AssignPermissionsCommand(id, permissionIds)));

    [HttpPost($"{ApiRoutes.Identity.Roles}/{{id:guid}}/permissions/remove")]
    public async Task<IActionResult> RemovePermissions(Guid id, [FromBody] List<Guid> permissionIds)
        => HandleResult(await Mediator.Send(new RemovePermissionsCommand(id, permissionIds)));

    [HttpPost($"{ApiRoutes.Identity.Users}/{{userId:guid}}/roles/assign/{{roleId:guid}}")]
    public async Task<IActionResult> AssignRoleToUser(Guid userId, Guid roleId)
        => HandleResult(await Mediator.Send(new AssignRoleToUserCommand(userId, roleId)));

    [HttpPost($"{ApiRoutes.Identity.Users}/{{userId:guid}}/roles/remove/{{roleId:guid}}")]
    public async Task<IActionResult> RemoveRoleFromUser(Guid userId, Guid roleId)
        => HandleResult(await Mediator.Send(new RemoveRoleFromUserCommand(userId, roleId)));
}
