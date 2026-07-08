namespace GastroErp.Application.Features.Organization.DTOs;

public record CompanyDto(
    Guid Id,
    Guid TenantId,
    string NameAr,
    string? NameEn,
    string TaxNumber,
    string? VatNumber,
    string? CommercialRegister,
    string? Website,
    string? LogoUrl,
    string? Email,
    string? PhoneNumber,
    bool IsActive,
    byte FiscalYearStartMonth,
    DateTime CreatedAt
);

public record CreateCompanyDto(
    Guid TenantId,
    string NameAr,
    string? NameEn,
    string TaxNumber,
    string? VatNumber = null,
    string? CommercialRegister = null,
    string? Website = null,
    byte FiscalYearStartMonth = 1
);

public record UpdateCompanyDto(
    string? Website,
    string? LogoUrl,
    string? Email,
    string? PhoneNumber
);
