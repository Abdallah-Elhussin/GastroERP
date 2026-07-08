namespace GastroErp.Application.Features.Identity.DTOs;

public record UserDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public string? AvatarUrl { get; init; }
    public string PreferredLanguage { get; init; } = "ar";
    public bool IsActive { get; init; }
    public bool IsEmailVerified { get; init; }
    public DateTimeOffset? LastLoginAt { get; init; }
    public bool IsLocked { get; init; }
}
