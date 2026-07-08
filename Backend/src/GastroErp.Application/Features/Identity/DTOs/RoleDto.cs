namespace GastroErp.Application.Features.Identity.DTOs;

public record RoleDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? NameAr { get; init; }
    public string? Description { get; init; }
    public bool IsSystem { get; init; }
    public bool IsActive { get; init; }
}
