namespace GastroErp.Application.Features.Identity.DTOs;

public record UpdateUserDto
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public string? AvatarUrl { get; init; }
    public string PreferredLanguage { get; init; } = "ar";
}
