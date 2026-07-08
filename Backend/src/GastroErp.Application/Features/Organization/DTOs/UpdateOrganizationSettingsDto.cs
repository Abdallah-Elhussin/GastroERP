using System;

namespace GastroErp.Application.Features.Organization.DTOs;

public record UpdateOrganizationSettingsDto(
    string CompanyName,
    string? LegalName,
    string? CommercialRegistration,
    string? TaxNumber,
    Guid? DefaultCurrencyId,
    Guid? DefaultLanguageId,
    Guid? DefaultTimezoneId,
    string? DateFormat,
    string? NumberFormat,
    string? LogoUrl,
    string? Theme,
    string? Address,
    string? ContactEmail,
    string? ContactPhone
);
