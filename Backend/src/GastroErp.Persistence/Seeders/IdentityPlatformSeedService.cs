using GastroErp.Application.Common.Authorization;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Persistence.Seeders;

public sealed class IdentityPlatformSeedService : IIdentityPlatformSeedService
{
    private static readonly (string Name, string? NameAr, string Description)[] RestaurantRoles =
    [
        ("Administrator", "مدير النظام", "Full platform access"),
        ("Branch Manager", "مدير فرع", "Branch and operations manager"),
        ("Cashier", "أمين صندوق", "POS cashier"),
        ("Waiter", "نادل", "Waiter and table service"),
        ("Kitchen", "مطبخ", "Kitchen operations"),
        ("Inventory", "مخزون", "Inventory operations"),
        ("Accountant", "محاسب", "Finance and accounting"),
        ("HR", "موارد بشرية", "HR operations")
    ];

    private static readonly (string Name, string? NameAr, string Description)[] LegacyRoles =
    [
        ("Administrator", "مدير النظام", "Full platform access"),
        ("Manager", "مدير", "Branch and operations manager"),
        ("Cashier", "أمين صندوق", "POS cashier"),
        ("Accountant", "محاسب", "Finance and accounting"),
        ("HrManager", "مدير موارد بشرية", "HR operations"),
        ("KitchenManager", "مدير مطبخ", "Kitchen operations"),
        ("InventoryManager", "مدير مخزون", "Inventory operations"),
        ("Viewer", "مشاهد", "Read-only access")
    ];

    private readonly IApplicationDbContext _context;
    private readonly ILogger<IdentityPlatformSeedService> _logger;

    public IdentityPlatformSeedService(IApplicationDbContext context, ILogger<IdentityPlatformSeedService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task EnsureGlobalPermissionsAsync(CancellationToken cancellationToken = default)
    {
        if (await _context.Permissions.AnyAsync(cancellationToken))
        {
            return;
        }

        foreach (var definition in PermissionCatalog.GetAll())
        {
            var permission = new Permission(
                Permission.CreateStableId(definition.Name),
                definition.Module,
                definition.Name,
                definition.DisplayName);

            _context.Permissions.Add(permission);
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded {Count} permissions", PermissionCatalog.GetAll().Count);
    }

    public Task<Role> EnsureTenantRolesAsync(Guid tenantId, CancellationToken cancellationToken = default)
        => EnsureRolesAsync(tenantId, LegacyRoles, useRestaurantMatrix: false, cancellationToken);

    public Task<Role> EnsureRestaurantRolesAsync(Guid tenantId, CancellationToken cancellationToken = default)
        => EnsureRolesAsync(tenantId, RestaurantRoles, useRestaurantMatrix: true, cancellationToken);

    private async Task<Role> EnsureRolesAsync(
        Guid tenantId,
        IReadOnlyList<(string Name, string? NameAr, string Description)> roles,
        bool useRestaurantMatrix,
        CancellationToken cancellationToken)
    {
        await EnsureGlobalPermissionsAsync(cancellationToken);

        var allPermissions = await _context.Permissions
            .AsNoTracking()
            .Select(p => new { p.Id, p.Name })
            .ToListAsync(cancellationToken);

        Role? administrator = null;

        foreach (var (name, nameAr, description) in roles)
        {
            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Name == name, cancellationToken);

            if (role is null)
            {
                role = new Role(tenantId, name, nameAr, description);
                _context.Roles.Add(role);
                await _context.SaveChangesAsync(cancellationToken);
            }

            var permissionIds = useRestaurantMatrix
                ? allPermissions
                    .Where(p => RolePermissionTemplates.MatchesRole(name, p.Name))
                    .Select(p => p.Id)
                    .ToList()
                : GetLegacyPermissionIds(name, allPermissions.Select(p => (p.Id, p.Name)).ToList());

            await EnsureRolePermissionsAsync(role.Id, permissionIds, cancellationToken);

            if (string.Equals(name, "Administrator", StringComparison.Ordinal))
            {
                administrator = role;
            }
        }

        return administrator ?? throw new InvalidOperationException("Administrator role was not created.");
    }

    private static List<Guid> GetLegacyPermissionIds(
        string roleName,
        IReadOnlyList<(Guid Id, string Name)> allPermissions)
    {
        if (string.Equals(roleName, "Administrator", StringComparison.Ordinal))
        {
            return allPermissions.Select(p => p.Id).ToList();
        }

        if (string.Equals(roleName, "Viewer", StringComparison.Ordinal))
        {
            return allPermissions
                .Where(p => p.Name.EndsWith(".View", StringComparison.Ordinal)
                            || p.Name.EndsWith(".Use", StringComparison.Ordinal))
                .Select(p => p.Id)
                .ToList();
        }

        return [];
    }

    private async Task EnsureRolePermissionsAsync(
        Guid roleId,
        IReadOnlyCollection<Guid> permissionIds,
        CancellationToken cancellationToken)
    {
        var existing = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.PermissionId)
            .ToListAsync(cancellationToken);

        foreach (var permissionId in permissionIds.Except(existing))
        {
            _context.RolePermissions.Add(new RolePermission(roleId, permissionId));
        }

        if (permissionIds.Except(existing).Any())
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
