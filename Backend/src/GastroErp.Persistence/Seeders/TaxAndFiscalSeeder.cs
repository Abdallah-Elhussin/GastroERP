using GastroErp.Application.Common.Interfaces;
using GastroErp.Domain.Entities.Finance;
using GastroErp.Domain.Entities.Invoicing;
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
        if (!await context.TaxRates.AnyAsync(t => t.TenantId == tenantId, ct))
        {
            context.TaxRates.Add(TaxRate.Create(
                tenantId, "VAT15", "ضريبة القيمة المضافة 15%", TaxType.VAT,
                TaxCalculationMethod.Percentage, 15m, isInclusive: false,
                nameEn: "VAT 15%", description: "ZATCA standard VAT rate"));
            await context.SaveChangesAsync(ct);
        }

        var year = DateTime.UtcNow.Year;
        if (!await context.FiscalPeriods.AnyAsync(p => p.TenantId == tenantId && p.FiscalYear == year, ct))
        {
            context.FiscalPeriods.Add(FiscalPeriod.Create(
                tenantId, year, $"السنة المالية {year}",
                new DateOnly(year, 1, 1), new DateOnly(year, 12, 31)));
            await context.SaveChangesAsync(ct);
        }

        _logger.LogInformation("Tax and fiscal data seeded for tenant {TenantId}", tenantId);
    }
}
