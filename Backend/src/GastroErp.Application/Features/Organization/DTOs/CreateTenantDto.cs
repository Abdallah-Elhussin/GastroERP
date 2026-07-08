namespace GastroErp.Application.Features.Organization.DTOs;

public record CreateTenantDto(
    string NameAr,
    string? NameEn,
    string Slug,
    string DefaultCurrency = "SAR",
    string DefaultLanguage = "ar",
    string DefaultTimezone = "Arab Standard Time"
);
