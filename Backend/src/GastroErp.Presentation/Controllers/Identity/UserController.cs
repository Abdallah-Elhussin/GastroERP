using Asp.Versioning;
using GastroErp.Application.Features.Identity.Commands.Users;
using GastroErp.Application.Features.Identity.DTOs;
using GastroErp.Application.Features.Identity.Queries.Users;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Identity;

/// <summary>
/// User management endpoints (ترميز المستخدمين).
/// </summary>
[ApiVersion("1.0")]
public class UserController : BaseApiController
{
    [HttpGet(ApiRoutes.Identity.Users)]
    [HasPermission(Permissions.Identity.UsersView)]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? searchTerm = null,
        [FromQuery] Guid? branchId = null,
        [FromQuery] Guid? roleId = null,
        [FromQuery] bool? isActive = null)
        => HandlePagedResult(await Mediator.Send(
            new GetUsersQuery(pageNumber, pageSize, searchTerm, branchId, roleId, isActive)));

    [HttpGet($"{ApiRoutes.Identity.Users}/license-status")]
    [HasPermission(Permissions.Identity.UsersView)]
    public async Task<IActionResult> GetLicenseStatus()
        => HandleResult(await Mediator.Send(new GetUserLicenseStatusQuery()));

    [HttpGet($"{ApiRoutes.Identity.Users}/{{id:guid}}")]
    [HasPermission(Permissions.Identity.UsersView)]
    public async Task<IActionResult> GetUserById(Guid id)
        => HandleResult(await Mediator.Send(new GetUserByIdQuery(id)));

    [HttpPost(ApiRoutes.Identity.Users)]
    [HasPermission(Permissions.Identity.UsersCreate)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto request)
        => HandleResult(await Mediator.Send(new CreateUserCommand(request)));

    [HttpPut($"{ApiRoutes.Identity.Users}/{{id:guid}}")]
    [HasPermission(Permissions.Identity.UsersEdit)]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto request)
        => HandleResult(await Mediator.Send(new UpdateUserCommand(id, request)));

    [HttpDelete($"{ApiRoutes.Identity.Users}/{{id:guid}}")]
    [HasPermission(Permissions.Identity.UsersDelete)]
    public async Task<IActionResult> DeleteUser(Guid id)
        => HandleResult(await Mediator.Send(new DeleteUserCommand(id)));

    [HttpPost($"{ApiRoutes.Identity.Users}/{{id:guid}}/restore")]
    [HasPermission(Permissions.Identity.UsersEdit)]
    public async Task<IActionResult> RestoreUser(Guid id)
        => HandleResult(await Mediator.Send(new RestoreUserCommand(id)));

    [HttpPost($"{ApiRoutes.Identity.Users}/{{id:guid}}/activate")]
    [HasPermission(Permissions.Identity.UsersEdit)]
    public async Task<IActionResult> ActivateUser(Guid id)
        => HandleResult(await Mediator.Send(new ActivateUserCommand(id)));

    [HttpPost($"{ApiRoutes.Identity.Users}/{{id:guid}}/deactivate")]
    [HasPermission(Permissions.Identity.UsersEdit)]
    public async Task<IActionResult> DeactivateUser(Guid id)
        => HandleResult(await Mediator.Send(new DeactivateUserCommand(id)));

    [HttpPost($"{ApiRoutes.Identity.Users}/{{id:guid}}/lock")]
    [HasPermission(Permissions.Identity.UsersLockUnlock)]
    public async Task<IActionResult> LockUser(Guid id, [FromQuery] DateTimeOffset? until = null)
        => HandleResult(await Mediator.Send(new LockUserCommand(id, until ?? DateTimeOffset.UtcNow.AddYears(100))));

    [HttpPost($"{ApiRoutes.Identity.Users}/{{id:guid}}/unlock")]
    [HasPermission(Permissions.Identity.UsersLockUnlock)]
    public async Task<IActionResult> UnlockUser(Guid id)
        => HandleResult(await Mediator.Send(new UnlockUserCommand(id)));

    [HttpPost($"{ApiRoutes.Identity.Users}/{{id:guid}}/reset-password")]
    [HasPermission(Permissions.Identity.UsersResetPassword)]
    public async Task<IActionResult> ResetPassword(Guid id, [FromBody] ResetUserPasswordDto request)
        => HandleResult(await Mediator.Send(new AdminResetUserPasswordCommand(id, request.NewPassword)));
}
