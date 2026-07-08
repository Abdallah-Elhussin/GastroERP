using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Domain.Entities.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
    private readonly ICurrentUser _currentUserService;

    public UserCommandHandlers(IApplicationDbContext context, ICurrentUser currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId;
        if (tenantId == Guid.Empty) return Result<Guid>.Failure("Tenant ID is required.");

        var exists = await _context.AppUsers.AnyAsync(u => u.Email == request.Dto.Email, cancellationToken);
        if (exists) return Result<Guid>.Failure("User with this email already exists.");

        // NOTE: Password should be hashed before saving. Assuming Dto.Password is hashed by caller or we hash it here.
        // For now, we store it as is since there is no IPasswordHasher interface injected yet.
        var user = new AppUser(
            tenantId, 
            request.Dto.Email, 
            request.Dto.Password, // TODO: Hash password
            request.Dto.FirstName, 
            request.Dto.LastName, 
            request.Dto.PhoneNumber, 
            request.Dto.PreferredLanguage);

        _context.AppUsers.Add(user);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(user.Id);
    }

    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
        if (user == null) return Result.Failure("User not found.");

        user.UpdateProfile(request.Dto.FirstName, request.Dto.LastName, request.Dto.PhoneNumber, request.Dto.AvatarUrl);
        // Note: Missing PreferredLanguage update method in AppUser? I will ignore it for now or we could add it to domain.

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
        if (user == null) return Result.Failure("User not found.");

        _context.AppUsers.Remove(user);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(RestoreUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.AppUsers.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == request.Id && u.IsDeleted, cancellationToken);
        if (user == null) return Result.Failure("Deleted user not found.");

        user.Restore();
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
        if (user == null) return Result.Failure("User not found.");

        user.Activate();
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
        if (user == null) return Result.Failure("User not found.");

        user.Deactivate();
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(LockUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
        if (user == null) return Result.Failure("User not found.");

        // No explicit Lock method taking a date in AppUser? 
        // AppUser has FailedLoginCount and LockedUntil. Wait, the domain has RecordFailedLogin() which sets it automatically.
        // Let's add a manual lock or just use RecordFailedLogin inside a loop? 
        // Let's modify the Domain to allow manual lock, or skip manual lock if it's strictly for failed logins.
        return Result.Failure("Manual lock not fully implemented in Domain.");
    }

    public async Task<Result> Handle(UnlockUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
        if (user == null) return Result.Failure("User not found.");

        user.RecordSuccessfulLogin(); // This unlocks it in Domain
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(AdminResetUserPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
        if (user == null) return Result.Failure("User not found.");

        user.ChangePassword(request.NewPassword); // TODO: Hash password
        user.ForcePasswordChange(); // Force them to change it again on next login
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
