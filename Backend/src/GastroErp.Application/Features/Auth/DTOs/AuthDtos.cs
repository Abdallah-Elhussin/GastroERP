namespace GastroErp.Application.Features.Auth.DTOs;

public record LoginDto(string Email, string Password);
public record AuthResponseDto(string Token, string RefreshToken, int ExpiresIn);
public record RefreshTokenDto(string Token, string RefreshToken);
public record ChangePasswordDto(string OldPassword, string NewPassword);
public record ForgotPasswordDto(string Email);
public record ResetPasswordDto(string Email, string Token, string NewPassword);
public record SwitchTenantDto(Guid TargetTenantId);
public record CurrentUserDto(string Id, string Email, string FullName, Guid TenantId, string[] Roles, string[] Permissions);
