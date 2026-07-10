namespace GastroErp.Application.Features.Onboarding.DTOs;

public record AdminAccountStepDto(
    string FullName,
    string Email,
    string Mobile,
    string Password,
    string ConfirmPassword);

public record CompanyAddressDto(
    string Country,
    string City,
    string? Region,
    string? District,
    string? Street,
    string? PostalCode,
    decimal? Latitude,
    decimal? Longitude);

public record CompanyDataStepDto(
    string NameAr,
    string NameEn,
    string TradeName,
    string? LogoUrl,
    string CommercialRegister,
    string TaxNumber,
    string? TaxCertificateUrl,
    string Phone,
    string Email,
    string? Website,
    CompanyAddressDto Address);

public record GeneralSettingsStepDto(
    string Language,
    string Currency,
    bool MultiCurrencyEnabled,
    IReadOnlyList<string>? AdditionalCurrencies,
    string Timezone,
    int FiscalYearStartMonth,
    string CalendarType);

public record MainBranchStepDto(
    string Name,
    string Phone,
    string? Email,
    string Address);

/// <summary>
/// Restaurant onboarding wizard payload (4 steps consolidated).
/// </summary>
public record RegisterCompanyDto(
    AdminAccountStepDto Admin,
    CompanyDataStepDto Company,
    GeneralSettingsStepDto Settings,
    MainBranchStepDto Branch);

public record OnboardingChecklistDto(
    bool CompanyCreated,
    bool AdministratorCreated,
    bool MainBranchCreated,
    bool WarehousesCreated,
    bool ChartOfAccountsInstalled,
    bool UnitsInstalled,
    bool VatInstalled,
    bool RolesInstalled,
    bool WorkflowsInstalled,
    bool DashboardsInstalled,
    bool TrialSubscriptionActivated);

public record RegisterCompanyResponseDto(
    Guid TenantId,
    Guid CompanyId,
    Guid BranchId,
    Guid UserId,
    string Token,
    string RefreshToken,
    int ExpiresIn,
    OnboardingChecklistDto Checklist,
    string RedirectUrl = "/dashboard");
