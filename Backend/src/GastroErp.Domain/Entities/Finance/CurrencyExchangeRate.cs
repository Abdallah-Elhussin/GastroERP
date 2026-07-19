using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;

namespace GastroErp.Domain.Entities.Finance;

/// <summary>
/// معامل تحويل العملة لفترة زمنية — لا يعرّف العملة، بل يحدد سعرها من StartDate حتى EndDate (أو مفتوح).
/// </summary>
public sealed class CurrencyExchangeRate : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid CurrencyId { get; private set; }
    public int Number { get; private set; }
    /// <summary>سعر وحدة واحدة من العملة مقابل عملة الشركة.</summary>
    public decimal Rate { get; private set; }
    public DateOnly StartDate { get; private set; }
    /// <summary>null = السجل مفتوح (السعر الحالي).</summary>
    public DateOnly? EndDate { get; private set; }
    public bool IsActive { get; private set; }
    public string? ChangeReason { get; private set; }

    private CurrencyExchangeRate() { }

    public static CurrencyExchangeRate Create(
        Guid tenantId,
        Guid currencyId,
        int number,
        decimal rate,
        DateOnly startDate,
        DateOnly? endDate = null,
        bool isActive = true,
        string? changeReason = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId required.", nameof(tenantId));
        if (currencyId == Guid.Empty) throw new ArgumentException("CurrencyId required.", nameof(currencyId));
        if (rate <= 0) throw new BusinessException(ErrorCodes.CurrencyExchangeRateInvalid);
        ValidatePeriod(startDate, endDate);

        return new CurrencyExchangeRate
        {
            TenantId = tenantId,
            CurrencyId = currencyId,
            Number = number,
            Rate = rate,
            StartDate = startDate,
            EndDate = endDate,
            IsActive = isActive,
            ChangeReason = NormalizeOptional(changeReason)
        };
    }

    public void Update(decimal rate, DateOnly startDate, DateOnly? endDate, string? changeReason)
    {
        if (rate <= 0) throw new BusinessException(ErrorCodes.CurrencyExchangeRateInvalid);
        ValidatePeriod(startDate, endDate);

        Rate = rate;
        StartDate = startDate;
        EndDate = endDate;
        ChangeReason = NormalizeOptional(changeReason);
    }

    public void Close(DateOnly endDate)
    {
        if (endDate < StartDate)
            throw new BusinessException(ErrorCodes.CurrencyExchangeRateInvalidPeriod);
        EndDate = endDate;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    public bool IsOpen => EndDate is null;

    public bool Covers(DateOnly date)
    {
        if (date < StartDate) return false;
        return EndDate is null || date <= EndDate;
    }

    public static bool PeriodsOverlap(DateOnly startA, DateOnly? endA, DateOnly startB, DateOnly? endB)
    {
        var endAExclusive = endA ?? DateOnly.MaxValue;
        var endBExclusive = endB ?? DateOnly.MaxValue;
        return startA <= endBExclusive && startB <= endAExclusive;
    }

    private static void ValidatePeriod(DateOnly startDate, DateOnly? endDate)
    {
        if (endDate is DateOnly end && end < startDate)
            throw new BusinessException(ErrorCodes.CurrencyExchangeRateInvalidPeriod);
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
