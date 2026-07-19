using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Finance;

namespace GastroErp.Domain.Entities.Finance;

/// <summary>Currency — عملة النظام (Aggregate Root) لدعم تعدد العملات.</summary>
public sealed class Currency : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public int Number { get; private set; }
    public string Code { get; private set; }
    public string NameAr { get; private set; }
    public string NameEn { get; private set; }
    public string? Symbol { get; private set; }
    public byte DecimalPlaces { get; private set; }
    public string? SubUnitNameAr { get; private set; }
    public string? SubUnitNameEn { get; private set; }
    /// <summary>سعر صرف وحدة واحدة مقابل عملة الشركة.</summary>
    public decimal CurrentExchangeRate { get; private set; }
    public bool IsCompanyCurrency { get; private set; }
    public CurrencyStatus Status { get; private set; }
    public bool IsSystem { get; private set; }
    public int SortOrder { get; private set; }
    public DateTimeOffset? LastExchangeRateAt { get; private set; }
    public string? LastExchangeRateBy { get; private set; }

    private Currency()
    {
        Code = string.Empty;
        NameAr = string.Empty;
        NameEn = string.Empty;
    }

    public static Currency Create(
        Guid tenantId,
        int number,
        string code,
        string nameAr,
        string nameEn,
        decimal exchangeRate,
        bool isCompanyCurrency = false,
        string? symbol = null,
        byte decimalPlaces = 2,
        string? subUnitNameAr = null,
        string? subUnitNameEn = null,
        int sortOrder = 0,
        bool isSystem = false,
        string? rateUpdatedBy = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId required.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(code) || code.Trim().Length != 3)
            throw new BusinessException(ErrorCodes.CurrencyCodeInvalid);
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        if (string.IsNullOrWhiteSpace(nameEn)) throw new BusinessException(ErrorCodes.RequiredField);
        ValidateDecimalPlaces(decimalPlaces);

        var rate = isCompanyCurrency ? 1m : exchangeRate;
        if (rate <= 0) throw new BusinessException(ErrorCodes.CurrencyExchangeRateInvalid);

        var now = DateTimeOffset.UtcNow;
        var currency = new Currency
        {
            TenantId = tenantId,
            Number = number,
            Code = code.Trim().ToUpperInvariant(),
            NameAr = nameAr.Trim(),
            NameEn = nameEn.Trim(),
            Symbol = string.IsNullOrWhiteSpace(symbol) ? null : symbol.Trim(),
            DecimalPlaces = decimalPlaces,
            SubUnitNameAr = NormalizeOptional(subUnitNameAr),
            SubUnitNameEn = NormalizeOptional(subUnitNameEn),
            CurrentExchangeRate = rate,
            IsCompanyCurrency = isCompanyCurrency,
            Status = CurrencyStatus.Active,
            IsSystem = isSystem,
            SortOrder = sortOrder,
            LastExchangeRateAt = now,
            LastExchangeRateBy = rateUpdatedBy
        };

        currency.RaiseDomainEvent(new CurrencyCreatedEvent(currency.Id, tenantId, currency.Code));
        return currency;
    }

    public void Update(
        string nameAr,
        string nameEn,
        string? symbol,
        byte decimalPlaces,
        string? subUnitNameAr,
        string? subUnitNameEn,
        int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        if (string.IsNullOrWhiteSpace(nameEn)) throw new BusinessException(ErrorCodes.RequiredField);
        ValidateDecimalPlaces(decimalPlaces);

        NameAr = nameAr.Trim();
        NameEn = nameEn.Trim();
        Symbol = string.IsNullOrWhiteSpace(symbol) ? null : symbol.Trim();
        DecimalPlaces = decimalPlaces;
        SubUnitNameAr = NormalizeOptional(subUnitNameAr);
        SubUnitNameEn = NormalizeOptional(subUnitNameEn);
        SortOrder = sortOrder;
    }

    public void SetExchangeRate(decimal rate, string? updatedBy)
    {
        if (IsCompanyCurrency)
        {
            CurrentExchangeRate = 1m;
        }
        else
        {
            if (rate <= 0) throw new BusinessException(ErrorCodes.CurrencyExchangeRateInvalid);
            CurrentExchangeRate = rate;
        }

        LastExchangeRateAt = DateTimeOffset.UtcNow;
        LastExchangeRateBy = updatedBy;
    }

    public void MarkAsCompanyCurrency()
    {
        IsCompanyCurrency = true;
        CurrentExchangeRate = 1m;
        LastExchangeRateAt = DateTimeOffset.UtcNow;
    }

    public void ClearCompanyCurrencyFlag() => IsCompanyCurrency = false;

    public void Activate() => Status = CurrencyStatus.Active;
    public void Deactivate()
    {
        if (IsCompanyCurrency)
            throw new BusinessException(ErrorCodes.CurrencyCompanyCannotDeactivate);
        Status = CurrencyStatus.Inactive;
    }

    public void EnsureCanDelete()
    {
        if (IsSystem)
            throw new BusinessException(ErrorCodes.CurrencyProtected);
        if (IsCompanyCurrency)
            throw new BusinessException(ErrorCodes.CurrencyCompanyCannotDelete);
    }

    public bool IsActive => Status == CurrencyStatus.Active;
    public bool IsForeignCurrency => !IsCompanyCurrency;

    private static void ValidateDecimalPlaces(byte decimalPlaces)
    {
        if (decimalPlaces is not (0 or 2 or 3 or 4))
            throw new BusinessException(ErrorCodes.CurrencyDecimalPlacesInvalid);
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
