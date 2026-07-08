using Asp.Versioning;
using GastroErp.Application.Features.Identity.Commands.Users;
using GastroErp.Application.Features.Identity.DTOs;
using GastroErp.Application.Features.Identity.Queries.Users;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Identity;

/// <summary>
/// User management endpoints
/// </summary>
[ApiVersion("1.0")]
[Authorize]
public class UserController : BaseApiController
{
    [HttpGet(ApiRoutes.Identity.Users)]
    public async Task<IActionResult> GetUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTerm = null)
        => HandleResult(await Mediator.Send(new GetUsersQuery(pageNumber, pageSize, searchTerm)));

    [HttpGet($"{ApiRoutes.Identity.Users}/{{id:guid}}")]
    public async Task<IActionResult> GetUserById(Guid id)
        => HandleResult(await Mediator.Send(new GetUserByIdQuery(id)));

    [HttpPost(ApiRoutes.Identity.Users)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto request)
        => HandleResult(await Mediator.Send(new CreateUserCommand(request)));

    [HttpPut($"{ApiRoutes.Identity.Users}/{{id:guid}}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto request)
        => HandleResult(await Mediator.Send(new UpdateUserCommand(id, request)));

    [HttpDelete($"{ApiRoutes.Identity.Users}/{{id:guid}}")]
    public async Task<IActionResult> DeleteUser(Guid id)
        => HandleResult(await Mediator.Send(new DeleteUserCommand(id)));

    [HttpPost($"{ApiRoutes.Identity.Users}/{{id:guid}}/restore")]
    public async Task<IActionResult> RestoreUser(Guid id)
        => HandleResult(await Mediator.Send(new RestoreUserCommand(id)));

    [HttpPost($"{ApiRoutes.Identity.Users}/{{id:guid}}/activate")]
    public async Task<IActionResult> ActivateUser(Guid id)
        => HandleResult(await Mediator.Send(new ActivateUserCommand(id)));

    [HttpPost($"{ApiRoutes.Identity.Users}/{{id:guid}}/deactivate")]
    public async Task<IActionResult> DeactivateUser(Guid id)
        => HandleResult(await Mediator.Send(new DeactivateUserCommand(id)));

    [HttpPost($"{ApiRoutes.Identity.Users}/{{id:guid}}/lock")]
    public async Task<IActionResult> LockUser(Guid id, [FromQuery] DateTimeOffset until)
        => HandleResult(await Mediator.Send(new LockUserCommand(id, until)));

    [HttpPost($"{ApiRoutes.Identity.Users}/{{id:guid}}/unlock")]
    public async Task<IActionResult> UnlockUser(Guid id)
        => HandleResult(await Mediator.Send(new UnlockUserCommand(id)));

    [HttpPost($"{ApiRoutes.Identity.Users}/{{id:guid}}/reset-password")]
    public async Task<IActionResult> ResetPassword(Guid id, [FromBody] string newPassword)
        => HandleResult(await Mediator.Send(new AdminResetUserPasswordCommand(id, newPassword)));
}
