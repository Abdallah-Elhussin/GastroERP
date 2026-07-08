using GastroErp.Application.Common.Authorization;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Persistence.Seeders;

public sealed class IdentityPlatformSeedService : IIdentityPlatformSeedService
{
    private static readonly (string Name, string? NameAr, string Description)[] StandardRoles =
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

    public async Task<Role> EnsureTenantRolesAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        await EnsureGlobalPermissionsAsync(cancellationToken);

        var allPermissionIds = await _context.Permissions
            .AsNoTracking()
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        Role? administrator = null;

        foreach (var (name, nameAr, description) in StandardRoles)
        {
            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Name == name, cancellationToken);

            if (role is null)
            {
                role = new Role(tenantId, name, nameAr, description);
                _context.Roles.Add(role);
                await _context.SaveChangesAsync(cancellationToken);
            }

            if (string.Equals(name, "Administrator", StringComparison.Ordinal))
            {
                administrator = role;
                await EnsureRolePermissionsAsync(role.Id, allPermissionIds, cancellationToken);
            }
            else if (string.Equals(name, "Viewer", StringComparison.Ordinal))
            {
                var viewPermissionIds = await _context.Permissions
                    .AsNoTracking()
                    .Where(p => p.Name.EndsWith(".View") || p.Name.EndsWith(".Use"))
                    .Select(p => p.Id)
                    .ToListAsync(cancellationToken);
                await EnsureRolePermissionsAsync(role.Id, viewPermissionIds, cancellationToken);
            }

            if (string.Equals(name, "Administrator", StringComparison.Ordinal))
            {
                administrator = role;
            }
        }

        return administrator ?? throw new InvalidOperationException("Administrator role was not created.");
    }

    private async Task EnsureRolePermissionsAsync(Guid roleId, IReadOnlyCollection<Guid> permissionIds, CancellationToken cancellationToken)
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
