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
        if (_permissions.Any(p => p.PermissionId == permissionId)) return;
        _permissions.Add(new RolePermission(Id, permissionId));
    }

    public void RevokePermission(Guid permissionId)
    {
        var perm = _permissions.FirstOrDefault(p => p.PermissionId == permissionId);
        if (perm is not null) _permissions.Remove(perm);
    }

    /// <summary>مزامنة صلاحيات الدور مع المجموعة المطلوبة (إضافة/إزالة).</summary>
    public void SyncPermissions(IReadOnlyCollection<Guid> permissionIds)
    {
        var desired = permissionIds.Where(id => id != Guid.Empty).ToHashSet();
        var current = _permissions.Select(p => p.PermissionId).ToHashSet();

        foreach (var id in current.Except(desired))
            RevokePermission(id);

        foreach (var id in desired.Except(current))
            GrantPermission(id);
    }

    public void UpdateName(string name, string? nameAr)
    {
        if (IsSystem) throw new InvalidOperationException("System role names cannot be changed.");
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty.", nameof(name));
        Name = name;
        NameAr = nameAr;
    }

    public void UpdateDescription(string? description)
    {
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }

    public void Deactivate()
    {
        if (IsSystem) throw new InvalidOperationException("System roles cannot be deactivated.");
        IsActive = false;
    }

    public void Activate() => IsActive = true;
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
    public Guid Id { get; private set; }
    public Guid? CategoryId { get; private set; }
    public Guid? GroupId { get; private set; }
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

    public Permission(Guid id, string module, string name, string displayName,
                      string? displayNameAr = null, string? description = null,
                      Guid? categoryId = null, Guid? groupId = null)
    {
        if (id == Guid.Empty) throw new ArgumentException("Id cannot be empty.", nameof(id));
        if (string.IsNullOrWhiteSpace(module)) throw new ArgumentException("Module cannot be empty.", nameof(module));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("Display name cannot be empty.", nameof(displayName));

        Id = id;
        Module = module;
        Name = name;
        DisplayName = displayName;
        DisplayNameAr = displayNameAr;
        Description = description;
        CategoryId = categoryId;
        GroupId = groupId;
    }

    public Permission(string module, string name, string displayName,
                      string? displayNameAr = null, string? description = null)
        : this(CreateStableId(name), module, name, displayName, displayNameAr, description)
    {
    }

    public static Guid CreateStableId(string permissionName)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes($"gastroerp:perm:{permissionName.ToLowerInvariant()}"));
        var guidBytes = new byte[16];
        Array.Copy(bytes, guidBytes, 16);
        guidBytes[6] = (byte)((guidBytes[6] & 0x0F) | 0x50);
        guidBytes[8] = (byte)((guidBytes[8] & 0x3F) | 0x80);
        return new Guid(guidBytes);
    }
}
