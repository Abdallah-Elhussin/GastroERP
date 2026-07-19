using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Interfaces.Authorization;
using GastroErp.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace GastroErp.Application.Features.Identity.Services;

public sealed class EffectivePermissionService(
    IApplicationDbContext context,
    IMemoryCache cache) : IEffectivePermissionService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(2);

    public async Task<IReadOnlyList<string>> GetPermissionNamesAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty) return [];

        var cacheKey = $"eff-perms:{userId:N}";
        if (cache.TryGetValue(cacheKey, out IReadOnlyList<string>? cached) && cached is not null)
            return cached;

        var roleIds = await context.UserRoles.AsNoTracking()
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync(cancellationToken);

        var rolePermissionIds = roleIds.Count == 0
            ? new HashSet<Guid>()
            : (await context.RolePermissions.AsNoTracking()
                .Where(rp => roleIds.Contains(rp.RoleId))
                .Select(rp => rp.PermissionId)
                .ToListAsync(cancellationToken)).ToHashSet();

        var overrides = await context.UserPermissions.AsNoTracking()
            .Where(up => up.UserId == userId)
            .Select(up => new { up.PermissionId, up.Effect })
            .ToListAsync(cancellationToken);

        foreach (var o in overrides.Where(x => x.Effect == PermissionEffect.Allow))
            rolePermissionIds.Add(o.PermissionId);

        foreach (var o in overrides.Where(x => x.Effect == PermissionEffect.Deny))
            rolePermissionIds.Remove(o.PermissionId);

        if (rolePermissionIds.Count == 0)
        {
            cache.Set(cacheKey, Array.Empty<string>(), CacheDuration);
            return [];
        }

        var names = await context.Permissions.AsNoTracking()
            .Where(p => rolePermissionIds.Contains(p.Id))
            .Select(p => p.Name)
            .Distinct()
            .OrderBy(n => n)
            .ToListAsync(cancellationToken);

        cache.Set(cacheKey, (IReadOnlyList<string>)names, CacheDuration);
        return names;
    }

    public async Task<IReadOnlyList<Guid>> GetRolePermissionIdsAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        var roleIds = await context.UserRoles.AsNoTracking()
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync(cancellationToken);

        if (roleIds.Count == 0) return [];

        return await context.RolePermissions.AsNoTracking()
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.PermissionId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, byte>> GetUserOverridesAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        var rows = await context.UserPermissions.AsNoTracking()
            .Where(up => up.UserId == userId)
            .Select(up => new { up.PermissionId, Effect = (byte)up.Effect })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(x => x.PermissionId, x => x.Effect);
    }

    public static void Invalidate(IMemoryCache cache, Guid userId)
        => cache.Remove($"eff-perms:{userId:N}");
}
