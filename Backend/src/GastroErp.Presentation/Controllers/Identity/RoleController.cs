using Asp.Versioning;
using GastroErp.Application.Features.Identity.Commands.Roles;
using GastroErp.Application.Features.Identity.DTOs;
using GastroErp.Application.Features.Identity.Queries.Roles;
using GastroErp.Presentation.Authorization;
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
    [HasPermission(Permissions.Identity.RolesView)]
    public async Task<IActionResult> GetRoles()
        => HandleResult(await Mediator.Send(new GetRolesQuery()));

    [HttpGet($"{ApiRoutes.Identity.Roles}/{{id:guid}}")]
    [HasPermission(Permissions.Identity.RolesView)]
    public async Task<IActionResult> GetRoleById(Guid id)
        => HandleResult(await Mediator.Send(new GetRoleByIdQuery(id)));

    [HttpPost(ApiRoutes.Identity.Roles)]
    [HasPermission(Permissions.Identity.RolesManage)]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto request)
        => HandleResult(await Mediator.Send(new CreateRoleCommand(request)));

    [HttpPut($"{ApiRoutes.Identity.Roles}/{{id:guid}}")]
    [HasPermission(Permissions.Identity.RolesManage)]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleDto request)
        => HandleResult(await Mediator.Send(new UpdateRoleCommand(id, request)));

    [HttpDelete($"{ApiRoutes.Identity.Roles}/{{id:guid}}")]
    [HasPermission(Permissions.Identity.RolesManage)]
    public async Task<IActionResult> DeleteRole(Guid id)
        => HandleResult(await Mediator.Send(new DeleteRoleCommand(id)));

    [HttpGet($"{ApiRoutes.Identity.Roles}/permissions")]
    [HasPermission(Permissions.Identity.PermissionsView)]
    public async Task<IActionResult> GetPermissions()
        => HandleResult(await Mediator.Send(new GetPermissionsQuery()));

    [HttpGet($"{ApiRoutes.Identity.Roles}/{{id:guid}}/permissions")]
    [HasPermission(Permissions.Identity.PermissionsView)]
    public async Task<IActionResult> GetRolePermissions(Guid id)
        => HandleResult(await Mediator.Send(new GetRolePermissionsQuery(id)));

    [HttpPut($"{ApiRoutes.Identity.Roles}/{{id:guid}}/permissions")]
    [HasPermission(Permissions.Identity.PermissionsManage)]
    public async Task<IActionResult> ReplacePermissions(Guid id, [FromBody] List<Guid> permissionIds)
        => HandleResult(await Mediator.Send(new ReplaceRolePermissionsCommand(id, permissionIds)));

    [HttpPost($"{ApiRoutes.Identity.Roles}/{{id:guid}}/permissions/assign")]
    [HasPermission(Permissions.Identity.PermissionsManage)]
    public async Task<IActionResult> AssignPermissions(Guid id, [FromBody] List<Guid> permissionIds)
        => HandleResult(await Mediator.Send(new AssignPermissionsCommand(id, permissionIds)));

    [HttpPost($"{ApiRoutes.Identity.Roles}/{{id:guid}}/permissions/remove")]
    [HasPermission(Permissions.Identity.PermissionsManage)]
    public async Task<IActionResult> RemovePermissions(Guid id, [FromBody] List<Guid> permissionIds)
        => HandleResult(await Mediator.Send(new RemovePermissionsCommand(id, permissionIds)));

    [HttpPost($"{ApiRoutes.Identity.Roles}/{{id:guid}}/copy")]
    [HasPermission(Permissions.Identity.RolesManage)]
    public async Task<IActionResult> CopyRole(Guid id, [FromBody] CopyRoleRequest? body = null)
        => HandleResult(await Mediator.Send(new CopyRoleCommand(id, body?.Name, body?.NameAr)));

    [HttpPost($"{ApiRoutes.Identity.Users}/{{userId:guid}}/roles/assign/{{roleId:guid}}")]
    [HasPermission(Permissions.Identity.UsersManage)]
    public async Task<IActionResult> AssignRoleToUser(Guid userId, Guid roleId)
        => HandleResult(await Mediator.Send(new AssignRoleToUserCommand(userId, roleId)));

    [HttpPost($"{ApiRoutes.Identity.Users}/{{userId:guid}}/roles/remove/{{roleId:guid}}")]
    [HasPermission(Permissions.Identity.UsersManage)]
    public async Task<IActionResult> RemoveRoleFromUser(Guid userId, Guid roleId)
        => HandleResult(await Mediator.Send(new RemoveRoleFromUserCommand(userId, roleId)));
}

public record CopyRoleRequest(string? Name = null, string? NameAr = null);
