using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Interfaces.Authentication;
using GastroErp.Application.Common.Interfaces.Security;
using GastroErp.Application.Common.Options;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Auth.DTOs;
using GastroErp.Domain.Entities.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GastroErp.Application.Features.Auth.Commands;

public class AuthCommandHandlers :
    IRequestHandler<LoginCommand, Result<AuthResponseDto>>,
    IRequestHandler<RefreshTokenCommand, Result<AuthResponseDto>>,
    IRequestHandler<LogoutCommand, Result>,
    IRequestHandler<ChangePasswordCommand, Result>,
    IRequestHandler<ForgotPasswordCommand, Result>,
    IRequestHandler<ResetPasswordCommand, Result>,
    IRequestHandler<SwitchTenantCommand, Result<AuthResponseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IClaimsFactory _claimsFactory;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ICurrentUser _currentUser;
    private readonly AuthJwtSettings _jwtSettings;

    public AuthCommandHandlers(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IClaimsFactory claimsFactory,
        IRefreshTokenService refreshTokenService,
        ICurrentUser currentUser,
        IOptions<AuthJwtSettings> jwtSettings)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _claimsFactory = claimsFactory;
        _refreshTokenService = refreshTokenService;
        _currentUser = currentUser;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = request.Dto.Email.Trim().ToLowerInvariant();

        var user = await _context.AppUsers
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return Result<AuthResponseDto>.Failure("Unauthorized.InvalidCredentials", "Invalid email or password.");
        }

        if (user.IsLocked)
        {
            return Result<AuthResponseDto>.Failure("Unauthorized.AccountLocked", "Account is temporarily locked.");
        }

        if (!VerifyPassword(request.Dto.Password, user.PasswordHash))
        {
            user.RecordFailedLogin();
            await _context.SaveChangesAsync(cancellationToken);
            return Result<AuthResponseDto>.Failure("Unauthorized.InvalidCredentials", "Invalid email or password.");
        }

        user.RecordSuccessfulLogin();
        await _context.SaveChangesAsync(cancellationToken);

        return await IssueTokensAsync(user, cancellationToken);
    }

    public Task<Result<AuthResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result<AuthResponseDto>.Failure(
            "NotImplemented",
            "Refresh token flow is not configured yet."));
    }

    public Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success());
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.Id is null || _currentUser.Id == Guid.Empty)
        {
            return Result.Failure("Unauthorized", "User is not authenticated.");
        }

        var user = await _context.AppUsers
            .FirstOrDefaultAsync(u => u.Id == _currentUser.Id, cancellationToken);

        if (user is null)
        {
            return Result.Failure("NotFound", "User not found.");
        }

        if (!VerifyPassword(request.Dto.OldPassword, user.PasswordHash))
        {
            return Result.Failure("Unauthorized.InvalidCredentials", "Current password is incorrect.");
        }

        user.ChangePassword(_passwordHasher.HashPassword(request.Dto.NewPassword));
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success());
    }

    public Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Failure("NotImplemented", "Password reset is not configured yet."));
    }

    public Task<Result<AuthResponseDto>> Handle(SwitchTenantCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result<AuthResponseDto>.Failure(
            "NotImplemented",
            "Tenant switching is not configured yet."));
    }

    private async Task<Result<AuthResponseDto>> IssueTokensAsync(AppUser user, CancellationToken cancellationToken)
    {
        var roleNames = await (
            from userRole in _context.UserRoles
            join role in _context.Roles on userRole.RoleId equals role.Id
            where userRole.UserId == user.Id
            select role.Name
        ).ToListAsync(cancellationToken);

        var claims = _claimsFactory.CreateClaims(user, roleNames);
        var accessToken = _jwtTokenGenerator.GenerateToken(claims);
        var refreshToken = _refreshTokenService.GenerateRefreshToken();
        var expiresIn = _jwtSettings.ExpiryMinutes * 60;

        return Result<AuthResponseDto>.Success(new AuthResponseDto(accessToken, refreshToken, expiresIn));
    }

    private bool VerifyPassword(string password, string passwordHash)
    {
        if (passwordHash.StartsWith("$2", StringComparison.Ordinal))
        {
            return _passwordHasher.VerifyPassword(password, passwordHash);
        }

        return string.Equals(password, passwordHash, StringComparison.Ordinal);
    }
}
