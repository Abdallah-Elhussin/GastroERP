using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Interfaces.Authorization;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Identity.Services;
using GastroErp.Domain.Entities.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace GastroErp.Application.Features.Identity.Commands.Users;

public record UserPermissionOverrideDto(Guid PermissionId, PermissionEffect Effect);

public record UserPermissionsStateDto(
    Guid UserId,
    string UserName,
    string FullName,
    IReadOnlyList<Guid> RolePermissionIds,
    IReadOnlyList<UserPermissionOverrideDto> Overrides,
    IReadOnlyList<Guid> EffectivePermissionIds,
    IReadOnlyList<string> RoleNames);

public record GetUserPermissionsStateQuery(Guid UserId) : IRequest<Result<UserPermissionsStateDto>>;

public record ReplaceUserEffectivePermissionsCommand(Guid UserId, IReadOnlyList<Guid> DesiredPermissionIds)
    : IRequest<Result>;

public record ClearUserPermissionOverridesCommand(Guid UserId) : IRequest<Result>;

public sealed class GetUserPermissionsStateQueryHandler(
    IApplicationDbContext context,
    IEffectivePermissionService effectivePermissions)
    : IRequestHandler<GetUserPermissionsStateQuery, Result<UserPermissionsStateDto>>
{
    public async Task<Result<UserPermissionsStateDto>> Handle(
        GetUserPermissionsStateQuery request, CancellationToken cancellationToken)
    {
        var user = await context.AppUsers.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user is null) return Result<UserPermissionsStateDto>.Failure("UserNotFound", "User not found.");

        var roleNames = await (
            from ur in context.UserRoles.AsNoTracking()
            join r in context.Roles.AsNoTracking() on ur.RoleId equals r.Id
            where ur.UserId == user.Id
            select r.Name).ToListAsync(cancellationToken);

        var rolePermissionIds = await effectivePermissions.GetRolePermissionIdsAsync(user.Id, cancellationToken);
        var overrides = await effectivePermissions.GetUserOverridesAsync(user.Id, cancellationToken);
        var effectiveNames = await effectivePermissions.GetPermissionNamesAsync(user.Id, cancellationToken);

        IReadOnlyList<Guid> effectiveIds = effectiveNames.Count == 0
            ? []
            : await context.Permissions.AsNoTracking()
                .Where(p => effectiveNames.Contains(p.Name))
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);

        return Result<UserPermissionsStateDto>.Success(new UserPermissionsStateDto(
            user.Id,
            user.UserName,
            user.FullName,
            rolePermissionIds,
            overrides.Select(kv => new UserPermissionOverrideDto(kv.Key, (PermissionEffect)kv.Value)).ToList(),
            effectiveIds,
            roleNames));
    }
}

public sealed class ReplaceUserEffectivePermissionsCommandHandler(
    IApplicationDbContext context,
    IEffectivePermissionService effectivePermissions,
    IMemoryCache cache)
    : IRequestHandler<ReplaceUserEffectivePermissionsCommand, Result>
{
    public async Task<Result> Handle(ReplaceUserEffectivePermissionsCommand request, CancellationToken cancellationToken)
    {
        var user = await context.AppUsers.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user is null) return Result.Failure("UserNotFound", "User not found.");

        var roleIds = (await effectivePermissions.GetRolePermissionIdsAsync(user.Id, cancellationToken)).ToHashSet();
        var desiredRaw = request.DesiredPermissionIds.Where(id => id != Guid.Empty).ToHashSet();

        var desired = (await context.Permissions.AsNoTracking()
            .Where(p => desiredRaw.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync(cancellationToken)).ToHashSet();

        var existing = await context.UserPermissions
            .Where(up => up.UserId == user.Id)
            .ToListAsync(cancellationToken);
        context.UserPermissions.RemoveRange(existing);

        foreach (var id in desired.Except(roleIds))
            context.UserPermissions.Add(new UserPermission(user.TenantId, user.Id, id, PermissionEffect.Allow));

        foreach (var id in roleIds.Except(desired))
            context.UserPermissions.Add(new UserPermission(user.TenantId, user.Id, id, PermissionEffect.Deny));

        await context.SaveChangesAsync(cancellationToken);
        EffectivePermissionService.Invalidate(cache, user.Id);
        return Result.Success();
    }
}

public sealed class ClearUserPermissionOverridesCommandHandler(
    IApplicationDbContext context,
    IMemoryCache cache)
    : IRequestHandler<ClearUserPermissionOverridesCommand, Result>
{
    public async Task<Result> Handle(ClearUserPermissionOverridesCommand request, CancellationToken cancellationToken)
    {
        var existing = await context.UserPermissions
            .Where(up => up.UserId == request.UserId)
            .ToListAsync(cancellationToken);
        if (existing.Count == 0) return Result.Success();

        context.UserPermissions.RemoveRange(existing);
        await context.SaveChangesAsync(cancellationToken);
        EffectivePermissionService.Invalidate(cache, request.UserId);
        return Result.Success();
    }
}
