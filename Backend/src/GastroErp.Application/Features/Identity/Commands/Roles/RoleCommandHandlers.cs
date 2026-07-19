using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Identity.Services;
using GastroErp.Domain.Entities.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace GastroErp.Application.Features.Identity.Commands.Roles;

public class RoleCommandHandlers :
    IRequestHandler<CreateRoleCommand, Result<Guid>>,
    IRequestHandler<UpdateRoleCommand, Result>,
    IRequestHandler<DeleteRoleCommand, Result>,
    IRequestHandler<CopyRoleCommand, Result<Guid>>,
    IRequestHandler<AssignPermissionsCommand, Result>,
    IRequestHandler<RemovePermissionsCommand, Result>,
    IRequestHandler<ReplaceRolePermissionsCommand, Result>,
    IRequestHandler<AssignRoleToUserCommand, Result>,
    IRequestHandler<RemoveRoleFromUserCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUserService;
    private readonly IMemoryCache _cache;

    public RoleCommandHandlers(
        IApplicationDbContext context,
        ICurrentUser currentUserService,
        IMemoryCache cache)
    {
        _context = context;
        _currentUserService = currentUserService;
        _cache = cache;
    }

    public async Task<Result<Guid>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId;
        if (tenantId == Guid.Empty) return Result<Guid>.Failure("Tenant ID is required.");

        var exists = await _context.Roles.AnyAsync(r => r.Name == request.Dto.Name && r.TenantId == tenantId, cancellationToken);
        if (exists) return Result<Guid>.Failure("Role with this name already exists.");

        var role = new Role(tenantId, request.Dto.Name, request.Dto.NameAr, request.Dto.Description);

        _context.Roles.Add(role);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(role.Id);
    }

    public async Task<Result> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (role == null) return Result.Failure("Role not found.");

        if (role.IsSystem)
        {
            role.UpdateDescription(request.Dto.Description);
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }

        var exists = await _context.Roles.AnyAsync(
            r => r.Name == request.Dto.Name && r.TenantId == role.TenantId && r.Id != role.Id,
            cancellationToken);
        if (exists) return Result.Failure("Role with this name already exists.");

        role.UpdateName(request.Dto.Name, request.Dto.NameAr);
        role.UpdateDescription(request.Dto.Description);

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (role == null) return Result.Failure("Role not found.");
        if (role.IsSystem) return Result.Failure("System roles cannot be deleted.");

        var inUse = await _context.UserRoles.AnyAsync(ur => ur.RoleId == role.Id, cancellationToken);
        if (inUse) return Result.Failure("Role is assigned to users and cannot be deleted.");

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<Guid>> Handle(CopyRoleCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId;
        if (tenantId == Guid.Empty) return Result<Guid>.Failure("Tenant ID is required.");

        var source = await _context.Roles.Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == request.SourceRoleId, cancellationToken);
        if (source is null) return Result<Guid>.Failure("Source role not found.");

        var name = string.IsNullOrWhiteSpace(request.Name)
            ? $"{source.Name} Copy"
            : request.Name.Trim();
        var nameAr = string.IsNullOrWhiteSpace(request.NameAr)
            ? (string.IsNullOrWhiteSpace(source.NameAr) ? null : $"{source.NameAr} (نسخة)")
            : request.NameAr.Trim();

        var exists = await _context.Roles.AnyAsync(r => r.Name == name && r.TenantId == tenantId, cancellationToken);
        if (exists) return Result<Guid>.Failure("Role with this name already exists.");

        var copy = new Role(tenantId, name, nameAr, source.Description);
        copy.SyncPermissions(source.Permissions.Select(p => p.PermissionId).ToList());

        _context.Roles.Add(copy);
        await _context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(copy.Id);
    }

    public async Task<Result> Handle(AssignPermissionsCommand request, CancellationToken cancellationToken)
    {
        var role = await _context.Roles.Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (role == null) return Result.Failure("Role not found.");

        foreach (var permId in request.PermissionIds)
            role.GrantPermission(permId);

        await _context.SaveChangesAsync(cancellationToken);
        await InvalidateUsersForRoleAsync(request.Id, cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(RemovePermissionsCommand request, CancellationToken cancellationToken)
    {
        var role = await _context.Roles.Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (role == null) return Result.Failure("Role not found.");

        foreach (var permId in request.PermissionIds)
            role.RevokePermission(permId);

        await _context.SaveChangesAsync(cancellationToken);
        await InvalidateUsersForRoleAsync(request.Id, cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(ReplaceRolePermissionsCommand request, CancellationToken cancellationToken)
    {
        var role = await _context.Roles.Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (role == null) return Result.Failure("Role not found.");

        var validIds = await _context.Permissions.AsNoTracking()
            .Where(p => request.PermissionIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        role.SyncPermissions(validIds);
        await _context.SaveChangesAsync(cancellationToken);
        await InvalidateUsersForRoleAsync(request.Id, cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(AssignRoleToUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.AppUsers.Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null) return Result.Failure("User not found.");

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);
        if (role == null) return Result.Failure("Role not found.");

        user.AssignRole(request.RoleId);
        await _context.SaveChangesAsync(cancellationToken);
        EffectivePermissionService.Invalidate(_cache, request.UserId);
        return Result.Success();
    }

    public async Task<Result> Handle(RemoveRoleFromUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.AppUsers.Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null) return Result.Failure("User not found.");

        user.RemoveRole(request.RoleId);
        await _context.SaveChangesAsync(cancellationToken);
        EffectivePermissionService.Invalidate(_cache, request.UserId);
        return Result.Success();
    }

    private async Task InvalidateUsersForRoleAsync(Guid roleId, CancellationToken cancellationToken)
    {
        var userIds = await _context.UserRoles.AsNoTracking()
            .Where(ur => ur.RoleId == roleId)
            .Select(ur => ur.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var userId in userIds)
            EffectivePermissionService.Invalidate(_cache, userId);
    }
}
