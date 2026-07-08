using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Interfaces.Authentication;
using GastroErp.Application.Common.Interfaces.Platform;
using GastroErp.Application.Common.Interfaces.Security;
using GastroErp.Application.Common.Options;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Onboarding.DTOs;
using GastroErp.Domain.Entities.Identity;
using GastroErp.Domain.Entities.Inventory.Warehouse;
using GastroErp.Domain.Entities.Organization;
using GastroErp.Domain.Enums;
using GastroErp.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GastroErp.Application.Features.Onboarding.Commands;

public sealed class RegisterCompanyCommandHandler : IRequestHandler<RegisterCompanyCommand, Result<RegisterCompanyResponseDto>>
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
    private readonly ILogger<RegisterCompanyCommandHandler> _logger;

    public RegisterCompanyCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IClaimsFactory claimsFactory,
        IRefreshTokenService refreshTokenService,
        IAuthSessionService authSessionService,
        ITenantMasterDataSeedService masterDataSeed,
        IIdentityPlatformSeedService identitySeed,
        IOptions<AuthJwtSettings> jwtSettings,
        ILogger<RegisterCompanyCommandHandler> logger)
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

    public async Task<Result<RegisterCompanyResponseDto>> Handle(RegisterCompanyCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var email = dto.Email.Trim().ToLowerInvariant();

        if (await _context.AppUsers.AnyAsync(u => u.Email == email, cancellationToken))
        {
            return Result<RegisterCompanyResponseDto>.Failure("Conflict.EmailExists", "Email is already registered.");
        }

        var slug = CreateSlug(dto.CompanyName);
        if (await _context.Tenants.AnyAsync(t => t.Slug == slug, cancellationToken))
        {
            slug = $"{slug}-{Guid.NewGuid().ToString("N")[..6]}";
        }

        var tenant = new Tenant(dto.CompanyName, slug, GetCurrency(dto.Country), "ar", "Arab Standard Time", dto.CompanyName);
        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync(cancellationToken);

        var company = new Company(tenant.Id, dto.CompanyName, GenerateTaxNumber(), dto.CompanyName);
        _context.Companies.Add(company);
        await _context.SaveChangesAsync(cancellationToken);

        var branch = new Branch(tenant.Id, company.Id, "الفرع الرئيسي", BranchType.Restaurant, "Main Branch", "BR-001");
        branch.SetAsDefault();
        _context.Branches.Add(branch);
        await _context.SaveChangesAsync(cancellationToken);

        var warehouse = new Warehouse(tenant.Id, "المستودع الرئيسي", "Main Warehouse", "WH-001", branch.Id);
        warehouse.AddZone("منطقة التبريد", "Cold Zone", "COLD");
        warehouse.AddZone("منطقة جافة", "Dry Zone", "DRY");
        _context.Warehouses.Add(warehouse);

        await _identitySeed.EnsureGlobalPermissionsAsync(cancellationToken);
        var adminRole = await _identitySeed.EnsureTenantRolesAsync(tenant.Id, cancellationToken);

        var ownerParts = dto.OwnerName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var firstName = ownerParts.Length > 0 ? ownerParts[0] : "Owner";
        var lastName = ownerParts.Length > 1 ? ownerParts[1] : "User";

        var adminUser = new AppUser(
            tenant.Id,
            email,
            _passwordHasher.HashPassword(dto.Password),
            firstName,
            lastName,
            dto.Phone);

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
            "Trial onboarding subscription");
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
            deviceName: "Company Registration",
            cancellationToken: cancellationToken);

        _logger.LogInformation("Company registered. Tenant={TenantId}, Company={CompanyId}, User={UserId}", tenant.Id, company.Id, adminUser.Id);

        return Result<RegisterCompanyResponseDto>.Success(new RegisterCompanyResponseDto(
            tenant.Id,
            company.Id,
            adminUser.Id,
            accessToken,
            refreshToken));
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

    private static string GetCurrency(string country) =>
        country.ToUpperInvariant() switch
        {
            "SA" => "SAR",
            "AE" => "AED",
            "EG" => "EGP",
            _ => "USD"
        };

    private static string GenerateTaxNumber() =>
        $"3{DateTimeOffset.UtcNow:yyyyMMddHHmmss}".PadRight(15, '0')[..15];
}
