using Asp.Versioning;
using GastroErp.Application.Features.Auth.Commands;
using GastroErp.Application.Features.Auth.Queries;
using GastroErp.Application.Features.Auth.DTOs;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Auth;

/// <summary>
/// Authentication and User Identity management
/// </summary>
[ApiVersion("1.0")]
public class AuthController : BaseApiController
{
    /// <summary>
    /// Authenticate user and return JWT token
    /// </summary>
    [HttpPost(ApiRoutes.Auth.Login)]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        return HandleResult(await Mediator.Send(new LoginCommand(request)));
    }

    /// <summary>
    /// Refresh an expired JWT token using a valid refresh token
    /// </summary>
    [HttpPost(ApiRoutes.Auth.Refresh)]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto request)
    {
        return HandleResult(await Mediator.Send(new RefreshTokenCommand(request)));
    }

    /// <summary>
    /// Logout the current user and invalidate their refresh token
    /// </summary>
    [HttpPost(ApiRoutes.Auth.Logout)]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        return HandleResult(await Mediator.Send(new LogoutCommand()));
    }

    /// <summary>
    /// Change the current user's password
    /// </summary>
    [HttpPost(ApiRoutes.Auth.ChangePassword)]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
    {
        return HandleResult(await Mediator.Send(new ChangePasswordCommand(request)));
    }

    /// <summary>
    /// Request a password reset email
    /// </summary>
    [HttpPost(ApiRoutes.Auth.ForgotPassword)]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
    {
        return HandleResult(await Mediator.Send(new ForgotPasswordCommand(request)));
    }

    /// <summary>
    /// Reset password using token sent to email
    /// </summary>
    [HttpPost(ApiRoutes.Auth.ResetPassword)]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
    {
        return HandleResult(await Mediator.Send(new ResetPasswordCommand(request)));
    }

    /// <summary>
    /// Get the current authenticated user's profile and permissions
    /// </summary>
    [HttpGet(ApiRoutes.Auth.Me)]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        return HandleResult(await Mediator.Send(new GetCurrentUserQuery()));
    }

    /// <summary>
    /// Switch to another tenant the user has access to
    /// </summary>
    [HttpPost(ApiRoutes.Auth.SwitchTenant)]
    [Authorize]
    public async Task<IActionResult> SwitchTenant([FromBody] SwitchTenantDto request)
    {
        return HandleResult(await Mediator.Send(new SwitchTenantCommand(request)));
    }
}
