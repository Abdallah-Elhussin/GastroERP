using GastroErp.Domain.Common;

namespace GastroErp.Domain.Entities.Identity;

public sealed class PermissionCategory
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string? NameAr { get; private set; }
    public int SortOrder { get; private set; }

    private PermissionCategory()
    {
        Name = string.Empty;
    }

    public PermissionCategory(Guid id, string name, string? nameAr = null, int sortOrder = 0)
    {
        if (id == Guid.Empty) throw new ArgumentException("Id cannot be empty.", nameof(id));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty.", nameof(name));

        Id = id;
        Name = name;
        NameAr = nameAr;
        SortOrder = sortOrder;
    }
}

public sealed class PermissionGroup
{
    public Guid Id { get; private set; }
    public Guid CategoryId { get; private set; }
    public string Name { get; private set; }
    public string? NameAr { get; private set; }
    public int SortOrder { get; private set; }

    private PermissionGroup()
    {
        Name = string.Empty;
    }

    public PermissionGroup(Guid id, Guid categoryId, string name, string? nameAr = null, int sortOrder = 0)
    {
        if (id == Guid.Empty) throw new ArgumentException("Id cannot be empty.", nameof(id));
        if (categoryId == Guid.Empty) throw new ArgumentException("CategoryId cannot be empty.", nameof(categoryId));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty.", nameof(name));

        Id = id;
        CategoryId = categoryId;
        Name = name;
        NameAr = nameAr;
        SortOrder = sortOrder;
    }
}

public sealed class UserSession : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public string DeviceId { get; private set; }
    public string? DeviceName { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public DateTimeOffset LastSeenAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }

    private UserSession()
    {
        DeviceId = string.Empty;
    }

    public UserSession(
        Guid tenantId,
        Guid userId,
        string deviceId,
        DateTimeOffset expiresAt,
        string? deviceName = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (userId == Guid.Empty) throw new ArgumentException("UserId cannot be empty.", nameof(userId));
        if (string.IsNullOrWhiteSpace(deviceId)) throw new ArgumentException("DeviceId cannot be empty.", nameof(deviceId));

        TenantId = tenantId;
        UserId = userId;
        DeviceId = deviceId;
        ExpiresAt = expiresAt;
        DeviceName = deviceName;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        LastSeenAt = DateTimeOffset.UtcNow;
    }

    public void Touch() => LastSeenAt = DateTimeOffset.UtcNow;

    public void Revoke()
    {
        if (IsRevoked) return;
        IsRevoked = true;
        RevokedAt = DateTimeOffset.UtcNow;
    }

    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;
}

public sealed class RefreshTokenEntity
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid SessionId { get; private set; }
    public string TokenHash { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public string? ReplacedByTokenHash { get; private set; }

    private RefreshTokenEntity()
    {
        TokenHash = string.Empty;
    }

    public RefreshTokenEntity(Guid tenantId, Guid userId, Guid sessionId, string tokenHash, DateTimeOffset expiresAt)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (userId == Guid.Empty) throw new ArgumentException("UserId cannot be empty.", nameof(userId));
        if (sessionId == Guid.Empty) throw new ArgumentException("SessionId cannot be empty.", nameof(sessionId));
        if (string.IsNullOrWhiteSpace(tokenHash)) throw new ArgumentException("Token hash cannot be empty.", nameof(tokenHash));

        TenantId = tenantId;
        UserId = userId;
        SessionId = sessionId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Revoke(string? replacedByTokenHash = null)
    {
        if (IsRevoked) return;
        IsRevoked = true;
        RevokedAt = DateTimeOffset.UtcNow;
        ReplacedByTokenHash = replacedByTokenHash;
    }

    public bool IsActive => !IsRevoked && DateTimeOffset.UtcNow < ExpiresAt;
}
