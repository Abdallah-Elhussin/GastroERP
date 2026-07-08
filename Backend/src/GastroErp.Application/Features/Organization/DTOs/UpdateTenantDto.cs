namespace GastroErp.Application.Features.Organization.DTOs;

public record UpdateTenantDto(
    string DefaultCurrency,
    string DefaultLanguage,
    string DefaultTimezone,
    string? LogoUrl,
    string? PrimaryColor,
    string? SecondaryColor
);
