namespace GastroErp.Application.Features.Identity.DTOs;

public record CreateUserDto
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public string PreferredLanguage { get; init; } = "ar";
}
