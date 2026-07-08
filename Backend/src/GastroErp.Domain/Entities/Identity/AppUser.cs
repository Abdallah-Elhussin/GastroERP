using GastroErp.Domain.Common;
using GastroErp.Domain.Events.Organization;

namespace GastroErp.Domain.Entities.Identity;

/// <summary>
/// AppUser — المستخدم (Aggregate Root)
/// يمثل أي شخص يدخل إلى النظام. يحتوي على صلاحياته وفروعه المسموحة.
/// جميع عملياته تُسجَّل في سجل التدقيق (Audit Log).
/// </summary>
public sealed class AppUser : AuditableBaseEntity
{
    private const int MaxFailedAttempts = 5;
    private const int LockoutMinutes = 30;

    public Guid TenantId { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? AvatarUrl { get; private set; }
    public string? PinCode { get; private set; }
    public string PreferredLanguage { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsEmailVerified { get; private set; }
    public DateTimeOffset? LastLoginAt { get; private set; }
    public int FailedLoginCount { get; private set; }
    public DateTimeOffset? LockedUntil { get; private set; }
    public bool MustChangePassword { get; private set; }

    private readonly List<UserRole> _roles = [];
    public IReadOnlyCollection<UserRole> Roles => _roles.AsReadOnly();

    private readonly List<UserBranch> _branches = [];
    public IReadOnlyCollection<UserBranch> Branches => _branches.AsReadOnly();

    private AppUser()
    {
        Email = string.Empty;
        PasswordHash = string.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
        PreferredLanguage = "ar";
    }

    public AppUser(Guid tenantId, string email, string passwordHash,
                   string firstName, string lastName,
                   string? phoneNumber = null, string preferredLanguage = "ar")
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be empty.", nameof(email));
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));
        if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("First name cannot be empty.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("Last name cannot be empty.", nameof(lastName));

        TenantId = tenantId;
        Email = email.ToLowerInvariant();
        PasswordHash = passwordHash;
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        PreferredLanguage = preferredLanguage;
        IsActive = true;
        IsEmailVerified = false;
        MustChangePassword = false;

        RaiseDomainEvent(new UserCreatedEvent(Id, TenantId, Email));
    }

    public void RecordSuccessfulLogin()
    {
        FailedLoginCount = 0;
        LockedUntil = null;
        LastLoginAt = DateTimeOffset.UtcNow;
    }

    public void RecordFailedLogin()
    {
        FailedLoginCount++;
        if (FailedLoginCount >= MaxFailedAttempts)
        {
            var lockUntil = DateTimeOffset.UtcNow.AddMinutes(LockoutMinutes);
            LockedUntil = lockUntil;
            RaiseDomainEvent(new UserLockedEvent(Id, TenantId, lockUntil));
        }
    }

    public bool IsLocked => LockedUntil.HasValue && LockedUntil > DateTimeOffset.UtcNow;

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        RaiseDomainEvent(new UserDeactivatedEvent(Id, TenantId));
    }

    public void Activate() => IsActive = true;

    public void VerifyEmail() => IsEmailVerified = true;

    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Password hash cannot be empty.", nameof(newPasswordHash));
        PasswordHash = newPasswordHash;
        MustChangePassword = false;
        RaiseDomainEvent(new UserPasswordChangedEvent(Id, TenantId));
    }

    public void ForcePasswordChange()
    {
        MustChangePassword = true;
    }

    public void UpdateProfile(string firstName, string lastName, string? phone, string? avatarUrl)
    {
        if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("First name cannot be empty.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("Last name cannot be empty.", nameof(lastName));
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phone;
        AvatarUrl = avatarUrl;
    }

    public void AssignRole(Guid roleId)
    {
        if (_roles.Any(r => r.RoleId == roleId)) return;
        _roles.Add(new UserRole(Id, roleId, TenantId));
    }

    public void RemoveRole(Guid roleId)
    {
        var role = _roles.FirstOrDefault(r => r.RoleId == roleId);
        if (role is not null) _roles.Remove(role);
    }

    public void GrantBranchAccess(Guid branchId, bool isDefault = false)
    {
        if (_branches.Any(b => b.BranchId == branchId)) return;
        _branches.Add(new UserBranch(Id, branchId, TenantId, isDefault));
    }

    public void RevokeBranchAccess(Guid branchId)
    {
        var branch = _branches.FirstOrDefault(b => b.BranchId == branchId);
        if (branch is not null) _branches.Remove(branch);
    }

    public string FullName => $"{FirstName} {LastName}";
}

/// <summary>UserRole — ربط المستخدم بالدور</summary>
public sealed class UserRole
{
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }
    public Guid TenantId { get; private set; }
    public DateTimeOffset AssignedAt { get; private set; }
    public string? AssignedBy { get; private set; }

    private UserRole() { }

    public UserRole(Guid userId, Guid roleId, Guid tenantId, string? assignedBy = null)
    {
        UserId = userId;
        RoleId = roleId;
        TenantId = tenantId;
        AssignedAt = DateTimeOffset.UtcNow;
        AssignedBy = assignedBy;
    }
}

/// <summary>UserBranch — ربط المستخدم بالفرع</summary>
public sealed class UserBranch
{
    public Guid UserId { get; private set; }
    public Guid BranchId { get; private set; }
    public Guid TenantId { get; private set; }
    public bool IsDefault { get; private set; }
    public DateTimeOffset GrantedAt { get; private set; }
    public string? GrantedBy { get; private set; }

    private UserBranch() { }

    public UserBranch(Guid userId, Guid branchId, Guid tenantId, bool isDefault = false, string? grantedBy = null)
    {
        UserId = userId;
        BranchId = branchId;
        TenantId = tenantId;
        IsDefault = isDefault;
        GrantedAt = DateTimeOffset.UtcNow;
        GrantedBy = grantedBy;
    }

    public void SetAsDefault() => IsDefault = true;
}
