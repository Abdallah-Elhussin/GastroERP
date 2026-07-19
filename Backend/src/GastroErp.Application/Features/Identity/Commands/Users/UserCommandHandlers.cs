using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Interfaces.Security;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Identity.DTOs;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Entities.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Identity.Commands.Users;

public class UserCommandHandlers :
    IRequestHandler<CreateUserCommand, Result<Guid>>,
    IRequestHandler<UpdateUserCommand, Result>,
    IRequestHandler<DeleteUserCommand, Result>,
    IRequestHandler<RestoreUserCommand, Result>,
    IRequestHandler<ActivateUserCommand, Result>,
    IRequestHandler<DeactivateUserCommand, Result>,
    IRequestHandler<LockUserCommand, Result>,
    IRequestHandler<UnlockUserCommand, Result>,
    IRequestHandler<AdminResetUserPasswordCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<UserCommandHandlers> _logger;

    public UserCommandHandlers(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        IPasswordHasher passwordHasher,
        ILogger<UserCommandHandlers> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUser.TenantId;
        if (tenantId == Guid.Empty)
            return Result<Guid>.Failure(ErrorCodes.RequiredField, "Tenant ID is required.");

        var dto = request.Dto;
        var userName = dto.UserName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(userName))
            return Result<Guid>.Failure(ErrorCodes.RequiredField, "Please enter the username.");

        var firstName = dto.FirstName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(firstName))
            return Result<Guid>.Failure(ErrorCodes.RequiredField, "Please enter the first name.");

        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 8)
            return Result<Guid>.Failure(ErrorCodes.RequiredField, "Password must be at least 8 characters.");

        if (dto.BranchId == Guid.Empty)
            return Result<Guid>.Failure(ErrorCodes.RequiredField, "Please select a branch.");

        if (dto.RoleId == Guid.Empty)
            return Result<Guid>.Failure(ErrorCodes.RequiredField, "Please select a role.");

        var email = string.IsNullOrWhiteSpace(dto.Email)
            ? $"{userName.ToLowerInvariant()}@users.local"
            : dto.Email.Trim().ToLowerInvariant();

        if (await _context.AppUsers.AnyAsync(
                u => u.TenantId == tenantId && u.UserName == userName, cancellationToken))
            return Result<Guid>.Failure(ErrorCodes.UserNameDuplicate, "Username is already in use.");

        if (await _context.AppUsers.AnyAsync(
                u => u.TenantId == tenantId && u.Email == email, cancellationToken))
            return Result<Guid>.Failure(ErrorCodes.UserEmailDuplicate, "Email is already in use.");

        var branchOk = await _context.Branches.AnyAsync(
            b => b.Id == dto.BranchId && b.TenantId == tenantId, cancellationToken);
        if (!branchOk)
            return Result<Guid>.Failure(ErrorCodes.OrgBranchNotFound, "Branch not found.");

        var roleOk = await _context.Roles.AnyAsync(
            r => r.Id == dto.RoleId && r.IsActive && (r.IsSystem || r.TenantId == null || r.TenantId == tenantId),
            cancellationToken);
        if (!roleOk)
            return Result<Guid>.Failure(ErrorCodes.RequiredField, "Role not found.");

        var code = dto.Code?.Trim();
        if (string.IsNullOrWhiteSpace(code))
        {
            var maxCode = await _context.AppUsers.AsNoTracking()
                .Where(u => u.TenantId == tenantId && u.Code != null)
                .Select(u => u.Code)
                .ToListAsync(cancellationToken);
            var next = maxCode
                .Select(c => int.TryParse(c, out var n) ? n : 0)
                .DefaultIfEmpty(0)
                .Max() + 1;
            code = next.ToString();
        }

        var user = new AppUser(
            tenantId,
            userName,
            email,
            _passwordHasher.HashPassword(dto.Password),
            firstName,
            dto.LastName,
            dto.PhoneNumber,
            dto.MobileNumber,
            code,
            dto.PreferredLanguage,
            dto.IsPosUser,
            dto.MustChangePassword);

        if (!dto.IsActive)
            user.Deactivate();

        if (dto.IsLocked)
            user.Lock(DateTimeOffset.UtcNow.AddYears(100));

        _context.AppUsers.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        _context.UserRoles.Add(new UserRole(user.Id, dto.RoleId, tenantId));
        _context.UserBranches.Add(new UserBranch(user.Id, dto.BranchId, tenantId, isDefault: true));
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User created {UserId} by {Actor}", user.Id, _currentUser.Id);
        return Result<Guid>.Success(user.Id);
    }

    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
        if (user is null)
            return Result.Failure(ErrorCodes.UserNotFound, "User not found.");

        var dto = request.Dto;
        var userName = dto.UserName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(userName))
            return Result.Failure(ErrorCodes.RequiredField, "Please enter the username.");

        var firstName = dto.FirstName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(firstName))
            return Result.Failure(ErrorCodes.RequiredField, "Please enter the first name.");

        if (dto.BranchId == Guid.Empty)
            return Result.Failure(ErrorCodes.RequiredField, "Please select a branch.");

        if (dto.RoleId == Guid.Empty)
            return Result.Failure(ErrorCodes.RequiredField, "Please select a role.");

        if (await _context.AppUsers.AnyAsync(
                u => u.TenantId == user.TenantId && u.UserName == userName && u.Id != user.Id,
                cancellationToken))
            return Result.Failure(ErrorCodes.UserNameDuplicate, "Username is already in use.");

        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            var email = dto.Email.Trim().ToLowerInvariant();
            if (await _context.AppUsers.AnyAsync(
                    u => u.TenantId == user.TenantId && u.Email == email && u.Id != user.Id,
                    cancellationToken))
                return Result.Failure(ErrorCodes.UserEmailDuplicate, "Email is already in use.");
            user.UpdateEmail(email);
        }

        user.UpdateUserName(userName);
        user.SetCode(dto.Code);
        user.UpdateProfile(firstName, dto.LastName, dto.PhoneNumber, dto.MobileNumber, dto.AvatarUrl);
        user.SetPreferredLanguage(dto.PreferredLanguage);
        user.SetPosUser(dto.IsPosUser);
        user.SetMustChangePassword(dto.MustChangePassword);

        if (dto.IsActive) user.Activate();
        else user.Deactivate();

        if (dto.IsLocked && !user.IsLocked)
            user.Lock(DateTimeOffset.UtcNow.AddYears(100));
        else if (!dto.IsLocked && user.IsLocked)
            user.Unlock();

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            if (dto.Password.Length < 8)
                return Result.Failure(ErrorCodes.RequiredField, "Password must be at least 8 characters.");
            user.ChangePassword(_passwordHasher.HashPassword(dto.Password));
            if (dto.MustChangePassword)
                user.ForcePasswordChange();
        }

        await ReplacePrimaryRoleAsync(user.Id, user.TenantId, dto.RoleId, cancellationToken);
        await ReplaceDefaultBranchAsync(user.Id, user.TenantId, dto.BranchId, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("User updated {UserId}", user.Id);
        return Result.Success();
    }

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
        if (user is null)
            return Result.Failure(ErrorCodes.UserNotFound, "User not found.");

        user.SoftDelete(_currentUser.Id?.ToString());
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("User deleted {UserId}", user.Id);
        return Result.Success();
    }

    public async Task<Result> Handle(RestoreUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.AppUsers.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == request.Id && u.IsDeleted, cancellationToken);
        if (user is null)
            return Result.Failure(ErrorCodes.UserNotFound, "Deleted user not found.");

        user.Restore();
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
        if (user is null)
            return Result.Failure(ErrorCodes.UserNotFound, "User not found.");

        user.Activate();
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
        if (user is null)
            return Result.Failure(ErrorCodes.UserNotFound, "User not found.");

        user.Deactivate();
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(LockUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
        if (user is null)
            return Result.Failure(ErrorCodes.UserNotFound, "User not found.");

        var until = request.Until == default
            ? DateTimeOffset.UtcNow.AddYears(100)
            : request.Until;
        user.Lock(until);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("User locked {UserId} until {Until}", user.Id, until);
        return Result.Success();
    }

    public async Task<Result> Handle(UnlockUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
        if (user is null)
            return Result.Failure(ErrorCodes.UserNotFound, "User not found.");

        user.Unlock();
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("User unlocked {UserId}", user.Id);
        return Result.Success();
    }

    public async Task<Result> Handle(AdminResetUserPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
        if (user is null)
            return Result.Failure(ErrorCodes.UserNotFound, "User not found.");

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
            return Result.Failure(ErrorCodes.RequiredField, "Password must be at least 8 characters.");

        user.ChangePassword(_passwordHasher.HashPassword(request.NewPassword));
        user.ForcePasswordChange();
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Password reset for user {UserId}", user.Id);
        return Result.Success();
    }

    private async Task ReplacePrimaryRoleAsync(
        Guid userId,
        Guid tenantId,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        var existing = await _context.UserRoles.Where(r => r.UserId == userId).ToListAsync(cancellationToken);
        _context.UserRoles.RemoveRange(existing);
        _context.UserRoles.Add(new UserRole(userId, roleId, tenantId));
    }

    private async Task ReplaceDefaultBranchAsync(
        Guid userId,
        Guid tenantId,
        Guid branchId,
        CancellationToken cancellationToken)
    {
        var existing = await _context.UserBranches.Where(b => b.UserId == userId).ToListAsync(cancellationToken);
        _context.UserBranches.RemoveRange(existing);
        _context.UserBranches.Add(new UserBranch(userId, branchId, tenantId, isDefault: true));
    }
}
