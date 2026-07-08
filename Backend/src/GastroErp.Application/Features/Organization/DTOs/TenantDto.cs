using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Organization.DTOs;

public record TenantDto(
    Guid Id,
    string NameAr,
    string? NameEn,
    string Slug,
    TenantStatus Status,
    string DefaultCurrency,
    string DefaultLanguage,
    string DefaultTimezone,
    string? LogoUrl,
    string? PrimaryColor,
    string? SecondaryColor,
    DateTime CreatedAt
);
