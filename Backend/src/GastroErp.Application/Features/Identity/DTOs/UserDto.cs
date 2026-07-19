namespace GastroErp.Application.Features.Identity.DTOs;

public record UserDto
{
    public Guid Id { get; init; }
    public int Number { get; init; }
    public string? Code { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public string? MobileNumber { get; init; }
    public string? AvatarUrl { get; init; }
    public string PreferredLanguage { get; init; } = "ar";
    public bool IsActive { get; init; }
    public bool IsEmailVerified { get; init; }
    public bool IsPosUser { get; init; }
    public bool MustChangePassword { get; init; }
    public DateTimeOffset? LastLoginAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public bool IsLocked { get; init; }
    public Guid? BranchId { get; init; }
    public string? BranchNameAr { get; init; }
    public Guid? RoleId { get; init; }
    public string? RoleName { get; init; }
    public string? RoleNameAr { get; init; }
}

public record CreateUserDto
{
    public string UserName { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string? LastName { get; init; }
    public string? Email { get; init; }
    public string? MobileNumber { get; init; }
    public string? PhoneNumber { get; init; }
    public Guid BranchId { get; init; }
    public Guid RoleId { get; init; }
    public string Password { get; init; } = string.Empty;
    public bool IsActive { get; init; } = true;
    public bool IsPosUser { get; init; }
    public bool MustChangePassword { get; init; } = true;
    public bool IsLocked { get; init; }
    public string? Code { get; init; }
    public string PreferredLanguage { get; init; } = "ar";
}

public record UpdateUserDto
{
    public string UserName { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string? LastName { get; init; }
    public string? Email { get; init; }
    public string? MobileNumber { get; init; }
    public string? PhoneNumber { get; init; }
    public Guid BranchId { get; init; }
    public Guid RoleId { get; init; }
    public string? Password { get; init; }
    public bool IsActive { get; init; } = true;
    public bool IsPosUser { get; init; }
    public bool MustChangePassword { get; init; }
    public bool IsLocked { get; init; }
    public string? Code { get; init; }
    public string? AvatarUrl { get; init; }
    public string PreferredLanguage { get; init; } = "ar";
}

public record ResetUserPasswordDto(string NewPassword);

public record UserLicenseStatusDto(
    int CurrentUsers,
    int MaxUsers,
    bool IsUnlimited,
    bool IsTrial,
    string Label);
