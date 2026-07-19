using GastroErp.Domain.Common;

namespace GastroErp.Domain.Entities.Identity;

/// <summary>أثر صلاحية خاصة بالمستخدم (تتجاوز صلاحيات الدور).</summary>
public enum PermissionEffect : byte
{
    Allow = 1,
    Deny = 2
}

/// <summary>
/// صلاحية خاصة بمستخدم — تُضاف (Allow) أو تُستثنى (Deny) فوق صلاحيات الأدوار.
/// الأولوية: User Deny → User Allow → Role → Default deny.
/// </summary>
public sealed class UserPermission : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid PermissionId { get; private set; }
    public PermissionEffect Effect { get; private set; }

    private UserPermission()
    {
    }

    public UserPermission(Guid tenantId, Guid userId, Guid permissionId, PermissionEffect effect)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException(nameof(tenantId));
        if (userId == Guid.Empty) throw new ArgumentException(nameof(userId));
        if (permissionId == Guid.Empty) throw new ArgumentException(nameof(permissionId));

        TenantId = tenantId;
        UserId = userId;
        PermissionId = permissionId;
        Effect = effect;
    }

    public void SetEffect(PermissionEffect effect) => Effect = effect;
}
