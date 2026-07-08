using GastroErp.Domain.Common;

namespace GastroErp.Domain.Entities.Identity;

/// <summary>
/// Role — الدور الوظيفي (Aggregate Root)
/// يحدد مجموعة الصلاحيات الممنوحة لأصحاب هذا الدور.
/// الأدوار النظامية (IsSystem=true) لا يمكن حذفها أو تعديل صلاحياتها.
/// </summary>
public sealed class Role : AuditableBaseEntity
{
    public Guid? TenantId { get; private set; }
    public string Name { get; private set; }
    public string? NameAr { get; private set; }
    public string? Description { get; private set; }
    public bool IsSystem { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<RolePermission> _permissions = [];
    public IReadOnlyCollection<RolePermission> Permissions => _permissions.AsReadOnly();

    private Role()
    {
        Name = string.Empty;
    }

    /// <summary>إنشاء دور مخصص للمستأجر</summary>
    public Role(Guid tenantId, string name, string? nameAr = null, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Role name cannot be empty.", nameof(name));
        TenantId = tenantId;
        Name = name;
        NameAr = nameAr;
        Description = description;
        IsSystem = false;
        IsActive = true;
    }

    /// <summary>إنشاء دور نظامي (للـ Seed Data)</summary>
    public static Role CreateSystemRole(Guid id, string name, string? nameAr = null)
    {
        var role = new Role
        {
            Name = name,
            NameAr = nameAr,
            IsSystem = true,
            IsActive = true
        };
        return role;
    }

    public void GrantPermission(Guid permissionId)
    {
        if (IsSystem)
            throw new InvalidOperationException("Permissions on system roles cannot be modified.");
        if (_permissions.Any(p => p.PermissionId == permissionId)) return;
        _permissions.Add(new RolePermission(Id, permissionId));
    }

    public void RevokePermission(Guid permissionId)
    {
        if (IsSystem)
            throw new InvalidOperationException("Permissions on system roles cannot be modified.");
        var perm = _permissions.FirstOrDefault(p => p.PermissionId == permissionId);
        if (perm is not null) _permissions.Remove(perm);
    }

    public void UpdateName(string name, string? nameAr)
    {
        if (IsSystem) throw new InvalidOperationException("System role names cannot be changed.");
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty.", nameof(name));
        Name = name;
        NameAr = nameAr;
    }

    public void Deactivate()
    {
        if (IsSystem) throw new InvalidOperationException("System roles cannot be deactivated.");
        IsActive = false;
    }
}

/// <summary>RolePermission — ربط الدور بالصلاحية</summary>
public sealed class RolePermission
{
    public Guid RoleId { get; private set; }
    public Guid PermissionId { get; private set; }

    private RolePermission() { }

    public RolePermission(Guid roleId, Guid permissionId)
    {
        RoleId = roleId;
        PermissionId = permissionId;
    }
}

/// <summary>
/// Permission — الصلاحية
/// تمثل عملية محددة يمكن السماح بها أو منعها (مثل: orders.create).
/// الصلاحيات نظامية وثابتة، لا يمكن للمستخدم إنشاؤها.
/// </summary>
public sealed class Permission
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Module { get; private set; }
    public string Name { get; private set; }
    public string DisplayName { get; private set; }
    public string? DisplayNameAr { get; private set; }
    public string? Description { get; private set; }

    private Permission()
    {
        Module = string.Empty;
        Name = string.Empty;
        DisplayName = string.Empty;
    }

    public Permission(string module, string name, string displayName,
                      string? displayNameAr = null, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(module)) throw new ArgumentException("Module cannot be empty.", nameof(module));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("Display name cannot be empty.", nameof(displayName));

        Module = module;
        Name = name.ToLowerInvariant();
        DisplayName = displayName;
        DisplayNameAr = displayNameAr;
        Description = description;
    }
}
