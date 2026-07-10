using GastroErp.Application.Common.Authorization;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Interfaces.Authentication;
using GastroErp.Application.Common.Interfaces.Platform;
using GastroErp.Application.Common.Interfaces.Security;
using GastroErp.Application.Common.Options;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Onboarding;
using GastroErp.Application.Features.Onboarding.DTOs;
using GastroErp.Domain.Entities.Identity;
using GastroErp.Domain.Entities.Inventory.Warehouse;
using GastroErp.Domain.Entities.Organization;
using GastroErp.Domain.Enums;
using GastroErp.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GastroErp.Persistence.Services;

public sealed class RestaurantOnboardingService : IRestaurantOnboardingService
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IClaimsFactory _claimsFactory;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IAuthSessionService _authSessionService;
    private readonly ITenantMasterDataSeedService _masterDataSeed;
    private readonly IIdentityPlatformSeedService _identitySeed;
    private readonly AuthJwtSettings _jwtSettings;
    private readonly ILogger<RestaurantOnboardingService> _logger;

    public RestaurantOnboardingService(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IClaimsFactory claimsFactory,
        IRefreshTokenService refreshTokenService,
        IAuthSessionService authSessionService,
        ITenantMasterDataSeedService masterDataSeed,
        IIdentityPlatformSeedService identitySeed,
        IOptions<AuthJwtSettings> jwtSettings,
        ILogger<RestaurantOnboardingService> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _claimsFactory = claimsFactory;
        _refreshTokenService = refreshTokenService;
        _authSessionService = authSessionService;
        _masterDataSeed = masterDataSeed;
        _identitySeed = identitySeed;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    public async Task<Result<RegisterCompanyResponseDto>> ProvisionAsync(
        RegisterCompanyDto wizard,
        CancellationToken cancellationToken = default)
    {
        var email = wizard.Admin.Email.Trim().ToLowerInvariant();

        if (await _context.AppUsers.AnyAsync(u => u.Email == email, cancellationToken))
        {
            return Result<RegisterCompanyResponseDto>.Failure("Conflict.EmailExists", "Email is already registered.");
        }

        var countryCode = wizard.Company.Address.Country.ToUpperInvariant();
        var currency = wizard.Settings.Currency.ToUpperInvariant();
        var language = wizard.Settings.Language.ToLowerInvariant();
        var timezone = RestaurantOnboardingCatalog.ResolveWindowsTimezone(wizard.Settings.Timezone);
        var (countryAr, countryEn) = RestaurantOnboardingCatalog.GetCountryNames(countryCode);

        var slug = CreateSlug(wizard.Company.TradeName);
        if (await _context.Tenants.AnyAsync(t => t.Slug == slug, cancellationToken))
        {
            slug = $"{slug}-{Guid.NewGuid().ToString("N")[..6]}";
        }

        var tenant = new Tenant(
            wizard.Company.NameAr,
            slug,
            currency,
            language,
            timezone,
            wizard.Company.NameEn);
        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync(cancellationToken);

        var vatNumber = VatNumber.TryCreate(wizard.Company.TaxNumber);
        var company = new Company(
            tenant.Id,
            wizard.Company.NameAr,
            wizard.Company.TaxNumber,
            wizard.Company.NameEn,
            vatNumber,
            wizard.Company.CommercialRegister);

        company.UpdateContactInfo(
            EmailAddress.TryCreate(wizard.Company.Email),
            PhoneNumber.TryCreate(RestaurantOnboardingCatalog.NormalizePhone(wizard.Company.Phone, countryCode)),
            wizard.Company.Website);

        company.UpdateAddress(new Address(
            wizard.Company.Address.Street,
            wizard.Company.Address.Street,
            wizard.Company.Address.City,
            wizard.Company.Address.City,
            wizard.Company.Address.Region,
            wizard.Company.Address.Region,
            wizard.Company.Address.PostalCode,
            countryAr,
            countryEn));

        company.SetFiscalYear((byte)wizard.Settings.FiscalYearStartMonth);
        company.SetDefaultCurrency(RestaurantOnboardingCatalog.StableId($"currency:{currency}"));

        if (!string.IsNullOrWhiteSpace(wizard.Company.LogoUrl))
        {
            tenant.UpdateBranding(wizard.Company.LogoUrl, null, null);
        }

        _context.Companies.Add(company);
        await _context.SaveChangesAsync(cancellationToken);

        var branch = new Branch(
            tenant.Id,
            company.Id,
            wizard.Branch.Name,
            BranchType.Restaurant,
            wizard.Branch.Name,
            "BR-001");
        branch.SetAsDefault();
        branch.UpdateContactInfo(wizard.Branch.Email, RestaurantOnboardingCatalog.NormalizePhone(wizard.Branch.Phone, countryCode));
        branch.UpdateAddress(new Address(
            wizard.Branch.Address,
            wizard.Branch.Address,
            wizard.Company.Address.City,
            wizard.Company.Address.City,
            wizard.Company.Address.Region,
            wizard.Company.Address.Region,
            wizard.Company.Address.PostalCode,
            countryAr,
            countryEn));

        if (wizard.Company.Address.Latitude is decimal lat && wizard.Company.Address.Longitude is decimal lng)
        {
            branch.SetGeoLocation(lat, lng);
        }

        _context.Branches.Add(branch);
        await _context.SaveChangesAsync(cancellationToken);

        await SeedWarehousesAsync(tenant.Id, branch.Id, cancellationToken);
        await SeedCurrenciesAsync(tenant.Id, wizard, cancellationToken);

        var additionalCodes = wizard.Settings.MultiCurrencyEnabled && wizard.Settings.AdditionalCurrencies is not null
            ? string.Join(',', wizard.Settings.AdditionalCurrencies.Select(c => c.ToUpperInvariant()))
            : null;

        var orgSettings = new OrganizationSettings(
            tenant.Id,
            wizard.Company.TradeName,
            wizard.Company.NameEn,
            wizard.Company.CommercialRegister,
            wizard.Company.TaxNumber);

        orgSettings.ConfigureOnboarding(
            wizard.Settings.MultiCurrencyEnabled,
            wizard.Settings.CalendarType,
            additionalCodes);

        orgSettings.UpdateLocalization(
            RestaurantOnboardingCatalog.StableId($"currency:{currency}"),
            RestaurantOnboardingCatalog.StableId($"language:{language}"),
            RestaurantOnboardingCatalog.StableId($"timezone:{timezone}"),
            wizard.Settings.CalendarType.Equals("Hijri", StringComparison.OrdinalIgnoreCase) ? "dd/MM/yyyy (Hijri)" : "dd/MM/yyyy",
            "#,##0.00");

        orgSettings.UpdateContactInfo(
            branch.Address.FullAddressAr,
            wizard.Company.Email,
            RestaurantOnboardingCatalog.NormalizePhone(wizard.Company.Phone, countryCode));

        if (!string.IsNullOrWhiteSpace(wizard.Company.LogoUrl))
        {
            orgSettings.UpdateAppearance(wizard.Company.LogoUrl, "default");
        }

        _context.OrganizationSettings.Add(orgSettings);

        await _identitySeed.EnsureGlobalPermissionsAsync(cancellationToken);
        var adminRole = await _identitySeed.EnsureRestaurantRolesAsync(tenant.Id, cancellationToken);

        var ownerParts = wizard.Admin.FullName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var firstName = ownerParts.Length > 0 ? ownerParts[0] : "Admin";
        var lastName = ownerParts.Length > 1 ? ownerParts[1] : "User";

        var adminUser = new AppUser(
            tenant.Id,
            email,
            _passwordHasher.HashPassword(wizard.Admin.Password),
            firstName,
            lastName,
            RestaurantOnboardingCatalog.NormalizePhone(wizard.Admin.Mobile, countryCode));

        adminUser.AssignRole(adminRole.Id);
        adminUser.VerifyEmail();
        _context.AppUsers.Add(adminUser);
        await _context.SaveChangesAsync(cancellationToken);

        if (!_context.UserRoles.Any(ur => ur.UserId == adminUser.Id && ur.RoleId == adminRole.Id))
        {
            _context.UserRoles.Add(new UserRole(adminUser.Id, adminRole.Id, tenant.Id));
        }

        var trialPlan = await EnsureTrialPlanAsync(cancellationToken);
        var subscription = new Subscription(
            tenant.Id,
            trialPlan.Id,
            BillingCycle.Monthly,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(30),
            trialPlan.MaxBranches,
            trialPlan.MaxUsers,
            trialPlan.MaxDevices,
            Money.Zero(trialPlan.MonthlyPrice.Currency),
            "Restaurant onboarding trial subscription");
        subscription.SetTrial();
        _context.Subscriptions.Add(subscription);

        await _context.SaveChangesAsync(cancellationToken);
        await _masterDataSeed.SeedAsync(tenant.Id, cancellationToken);

        var roleNames = new[] { adminRole.Name };
        var claims = _claimsFactory.CreateClaims(adminUser, roleNames);
        var accessToken = _jwtTokenGenerator.GenerateToken(claims);
        var refreshToken = _refreshTokenService.GenerateRefreshToken();
        await _authSessionService.CreateSessionAsync(
            tenant.Id,
            adminUser.Id,
            refreshToken,
            deviceId: "onboarding",
            deviceName: "Restaurant Setup Wizard",
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Restaurant onboarded. Tenant={TenantId}, Company={CompanyId}, Branch={BranchId}, User={UserId}",
            tenant.Id, company.Id, branch.Id, adminUser.Id);

        var checklist = new OnboardingChecklistDto(
            CompanyCreated: true,
            AdministratorCreated: true,
            MainBranchCreated: true,
            WarehousesCreated: true,
            ChartOfAccountsInstalled: true,
            UnitsInstalled: true,
            VatInstalled: RestaurantOnboardingCatalog.GetVatRate(countryCode) > 0,
            RolesInstalled: true,
            WorkflowsInstalled: true,
            DashboardsInstalled: true,
            TrialSubscriptionActivated: true);

        return Result<RegisterCompanyResponseDto>.Success(new RegisterCompanyResponseDto(
            tenant.Id,
            company.Id,
            branch.Id,
            adminUser.Id,
            accessToken,
            refreshToken,
            _jwtSettings.ExpiryMinutes * 60,
            checklist));
    }

    private async Task SeedWarehousesAsync(Guid tenantId, Guid branchId, CancellationToken cancellationToken)
    {
        if (await _context.Warehouses.AnyAsync(w => w.TenantId == tenantId, cancellationToken))
        {
            return;
        }

        var warehouses = new (string NameAr, string NameEn, string Code)[]
        {
            ("المستودع الرئيسي", "Main Warehouse", "WH-001"),
            ("مستودع المطبخ", "Kitchen Warehouse", "WH-KIT"),
            ("مخزن جاف", "Dry Storage", "WH-DRY"),
            ("مخزن تبريد", "Cold Storage", "WH-COLD"),
            ("الفريزر", "Freezer", "WH-FRZ")
        };

        foreach (var (nameAr, nameEn, code) in warehouses)
        {
            _context.Warehouses.Add(new Warehouse(tenantId, nameAr, nameEn, code, branchId));
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedCurrenciesAsync(Guid tenantId, RegisterCompanyDto wizard, CancellationToken cancellationToken)
    {
        if (await _context.TenantCurrencies.AnyAsync(c => c.TenantId == tenantId, cancellationToken))
        {
            return;
        }

        var primary = wizard.Settings.Currency.ToUpperInvariant();
        _context.TenantCurrencies.Add(new TenantCurrency(tenantId, primary, isPrimary: true));

        if (wizard.Settings.MultiCurrencyEnabled && wizard.Settings.AdditionalCurrencies is not null)
        {
            foreach (var code in wizard.Settings.AdditionalCurrencies
                         .Select(c => c.ToUpperInvariant())
                         .Where(c => !string.Equals(c, primary, StringComparison.Ordinal))
                         .Distinct(StringComparer.Ordinal))
            {
                _context.TenantCurrencies.Add(new TenantCurrency(tenantId, code, isPrimary: false, exchangeRate: 1m));
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<SubscriptionPlan> EnsureTrialPlanAsync(CancellationToken cancellationToken)
    {
        var plan = await _context.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Name == "Trial", cancellationToken);

        if (plan is not null)
        {
            return plan;
        }

        plan = new SubscriptionPlan(
            "Trial",
            "تجريبي",
            SubscriptionPlanType.Starter,
            Money.Zero("SAR"),
            Money.Zero("SAR"),
            maxBranches: 3,
            maxUsers: 10,
            maxDevices: 5,
            maxProducts: 500,
            description: "Default trial plan",
            sortOrder: 0);

        _context.SubscriptionPlans.Add(plan);
        await _context.SaveChangesAsync(cancellationToken);
        return plan;
    }

    private static string CreateSlug(string companyName)
    {
        var slug = new string(companyName.Trim().ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '-')
            .ToArray());

        while (slug.Contains("--", StringComparison.Ordinal))
        {
            slug = slug.Replace("--", "-", StringComparison.Ordinal);
        }

        return slug.Trim('-');
    }
}
