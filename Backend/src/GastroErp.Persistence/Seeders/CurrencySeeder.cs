using GastroErp.Application.Common.Interfaces;
using GastroErp.Domain.Entities.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Persistence.Seeders;

/// <summary>عملات افتراضية للمطاعم في السعودية ودول الخليج.</summary>
public sealed class CurrencySeeder : IDataSeeder
{
    private readonly ILogger<CurrencySeeder> _logger;
    public CurrencySeeder(ILogger<CurrencySeeder> logger) => _logger = logger;
    public int Order => 21;

    public async Task SeedAsync(Guid tenantId, IApplicationDbContext context, CancellationToken ct = default)
    {
        if (await context.Currencies.AnyAsync(c => c.TenantId == tenantId, ct))
            return;

        var n = 1;
        var rateNo = 1;
        void Add(
            string code,
            string ar,
            string en,
            string symbol,
            decimal rate,
            bool company,
            string? subAr,
            string? subEn,
            byte decimals = 2)
        {
            var currency = Currency.Create(
                tenantId, n, code, ar, en, rate, company, symbol, decimals,
                subAr, subEn, sortOrder: n, isSystem: true, rateUpdatedBy: "seed");
            context.Currencies.Add(currency);

            if (!company)
            {
                context.CurrencyExchangeRates.Add(CurrencyExchangeRate.Create(
                    tenantId, currency.Id, rateNo++, rate,
                    new DateOnly(2026, 1, 1), endDate: null, isActive: true, changeReason: "Seed"));
            }

            n++;
        }

        Add("SAR", "الريال السعودي", "Saudi Riyal", "﷼", 1m, true, "هللة", "Halala");
        Add("USD", "الدولار الأمريكي", "US Dollar", "$", 3.75m, false, "سنت", "Cent");
        Add("EUR", "اليورو", "Euro", "€", 4.10m, false, "سنت", "Cent");
        Add("AED", "الدرهم الإماراتي", "UAE Dirham", "د.إ", 1.02m, false, "فلس", "Fils");
        Add("KWD", "الدينار الكويتي", "Kuwaiti Dinar", "د.ك", 12.25m, false, "فلس", "Fils", 3);
        Add("QAR", "الريال القطري", "Qatari Riyal", "ر.ق", 1.03m, false, "درهم", "Dirham");
        Add("OMR", "الريال العماني", "Omani Rial", "ر.ع.", 9.75m, false, "بيسة", "Baisa", 3);
        Add("BHD", "الدينار البحريني", "Bahraini Dinar", "د.ب", 9.95m, false, "فلس", "Fils", 3);

        await context.SaveChangesAsync(ct);
        _logger.LogInformation("Currencies seeded for tenant {TenantId}", tenantId);
    }
}
