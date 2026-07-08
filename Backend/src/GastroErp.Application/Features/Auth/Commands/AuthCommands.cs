using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Auth.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Auth.Commands;

public record LoginCommand(LoginDto Dto) : IRequest<Result<AuthResponseDto>>;
public record RefreshTokenCommand(RefreshTokenDto Dto) : IRequest<Result<AuthResponseDto>>;
public record LogoutCommand() : IRequest<Result>;
public record ChangePasswordCommand(ChangePasswordDto Dto) : IRequest<Result>;
public record ForgotPasswordCommand(ForgotPasswordDto Dto) : IRequest<Result>;
public record ResetPasswordCommand(ResetPasswordDto Dto) : IRequest<Result>;
public record SwitchTenantCommand(SwitchTenantDto Dto) : IRequest<Result<AuthResponseDto>>;
