using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Features.Onboarding;
using GastroErp.Domain.Entities.Invoicing;
using GastroErp.Domain.Entities.Finance;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Persistence.Seeders;

public sealed class TaxAndFiscalSeeder : IDataSeeder
{
    private readonly ILogger<TaxAndFiscalSeeder> _logger;

    public TaxAndFiscalSeeder(ILogger<TaxAndFiscalSeeder> logger) => _logger = logger;

    public int Order => 25;

    public async Task SeedAsync(Guid tenantId, IApplicationDbContext context, CancellationToken ct = default)
    {
        var company = await context.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.TenantId == tenantId, ct);

        var countryCode = company?.Address.CountryEn switch
        {
            "Saudi Arabia" => "SA",
            "United Arab Emirates" => "AE",
            "Kuwait" => "KW",
            "Qatar" => "QA",
            "Oman" => "OM",
            "Bahrain" => "BH",
            "Egypt" => "EG",
            "Sudan" => "SD",
            _ => "SA"
        };

        if (!await context.TaxRates.AnyAsync(t => t.TenantId == tenantId, ct))
        {
            var vatRate = RestaurantOnboardingCatalog.GetVatRate(countryCode);
            if (vatRate > 0)
            {
                context.TaxRates.Add(TaxRate.Create(
                    tenantId,
                    RestaurantOnboardingCatalog.GetVatCode(countryCode),
                    RestaurantOnboardingCatalog.GetVatNameAr(countryCode),
                    TaxType.VAT,
                    TaxCalculationMethod.Percentage,
                    vatRate,
                    isInclusive: false,
                    nameEn: RestaurantOnboardingCatalog.GetVatNameEn(countryCode),
                    description: $"Default VAT for {countryCode}"));
            }

            await context.SaveChangesAsync(ct);
        }

        var year = DateTime.UtcNow.Year;
        var startMonth = company?.FiscalYearStartMonth ?? (byte)1;

        if (!await context.FiscalPeriods.AnyAsync(p => p.TenantId == tenantId && p.FiscalYear == year, ct))
        {
            var period = FiscalPeriod.Create(tenantId, year, startMonth);
            period.GenerateMonthlyDetails();
            context.FiscalPeriods.Add(period);
            await context.SaveChangesAsync(ct);
        }

        _logger.LogInformation("Tax and fiscal data seeded for tenant {TenantId}", tenantId);
    }
}
