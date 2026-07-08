namespace GastroErp.Application.Features.Identity.DTOs;

public record UpdateRoleDto
{
    public string Name { get; init; } = string.Empty;
    public string? NameAr { get; init; }
    public string? Description { get; init; }
}
