using GastroErp.Application.Common.Interfaces;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Finance.Services;

public sealed class ExchangeRateService(IApplicationDbContext context) : IExchangeRateService
{
    public async Task<decimal> GetRateAsync(
        Guid tenantId, Guid currencyId, DateOnly asOfDate, CancellationToken cancellationToken = default)
    {
        var rate = await TryGetRateAsync(tenantId, currencyId, asOfDate, cancellationToken);
        if (rate is null)
            throw new BusinessException(ErrorCodes.CurrencyExchangeRateNotFound);
        return rate.Value;
    }

    public async Task<decimal> GetRateByCodeAsync(
        Guid tenantId, string currencyCode, DateOnly asOfDate, CancellationToken cancellationToken = default)
    {
        var code = currencyCode.Trim().ToUpperInvariant();
        var currency = await context.Currencies.AsNoTracking()
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Code == code, cancellationToken);
        if (currency is null)
            throw new BusinessException(ErrorCodes.CurrencyNotFound);
        if (currency.IsCompanyCurrency)
            return 1m;
        return await GetRateAsync(tenantId, currency.Id, asOfDate, cancellationToken);
    }

    public async Task<decimal?> TryGetRateAsync(
        Guid tenantId, Guid currencyId, DateOnly asOfDate, CancellationToken cancellationToken = default)
    {
        var currency = await context.Currencies.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == currencyId && c.TenantId == tenantId, cancellationToken);
        if (currency is null)
            return null;
        if (currency.IsCompanyCurrency)
            return 1m;

        var match = await context.CurrencyExchangeRates.AsNoTracking()
            .Where(r =>
                r.TenantId == tenantId &&
                r.CurrencyId == currencyId &&
                r.IsActive &&
                r.StartDate <= asOfDate &&
                (r.EndDate == null || r.EndDate >= asOfDate))
            .OrderByDescending(r => r.StartDate)
            .FirstOrDefaultAsync(cancellationToken);

        return match?.Rate;
    }
}
