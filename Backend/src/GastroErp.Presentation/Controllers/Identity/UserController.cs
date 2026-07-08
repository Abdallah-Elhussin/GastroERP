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
    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTerm = null)
    {
        return HandleResult(await Mediator.Send(new GetUsersQuery(pageNumber, pageSize, searchTerm)));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetUserByIdQuery(id)));
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto request)
    {
        return HandleResult(await Mediator.Send(new CreateUserCommand(request)));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto request)
    {
        return HandleResult(await Mediator.Send(new UpdateUserCommand(id, request)));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeleteUserCommand(id)));
    }

    [HttpPost("{id:guid}/restore")]
    public async Task<IActionResult> RestoreUser(Guid id)
    {
        return HandleResult(await Mediator.Send(new RestoreUserCommand(id)));
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> ActivateUser(Guid id)
    {
        return HandleResult(await Mediator.Send(new ActivateUserCommand(id)));
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> DeactivateUser(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeactivateUserCommand(id)));
    }

    [HttpPost("{id:guid}/lock")]
    public async Task<IActionResult> LockUser(Guid id, [FromQuery] DateTimeOffset until)
    {
        return HandleResult(await Mediator.Send(new LockUserCommand(id, until)));
    }

    [HttpPost("{id:guid}/unlock")]
    public async Task<IActionResult> UnlockUser(Guid id)
    {
        return HandleResult(await Mediator.Send(new UnlockUserCommand(id)));
    }

    [HttpPost("{id:guid}/reset-password")]
    public async Task<IActionResult> ResetPassword(Guid id, [FromBody] string newPassword)
    {
        return HandleResult(await Mediator.Send(new AdminResetUserPasswordCommand(id, newPassword)));
    }
}
