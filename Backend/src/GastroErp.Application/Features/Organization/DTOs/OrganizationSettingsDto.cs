using System;

namespace GastroErp.Application.Features.Organization.DTOs;

public class OrganizationSettingsDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? CommercialRegistration { get; set; }
    public string? TaxNumber { get; set; }
    public Guid? DefaultCurrencyId { get; set; }
    public Guid? DefaultLanguageId { get; set; }
    public Guid? DefaultTimezoneId { get; set; }
    public string? DateFormat { get; set; }
    public string? NumberFormat { get; set; }
    public string? LogoUrl { get; set; }
    public string? Theme { get; set; }
    public string? Address { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
}
