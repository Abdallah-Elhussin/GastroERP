using GastroErp.Domain.Common;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Organization;

/// <summary>
/// Tenant-scoped currency configuration for multi-currency support.
/// </summary>
public sealed class TenantCurrency : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public string CurrencyCode { get; private set; }
    public bool IsPrimary { get; private set; }
    public decimal ExchangeRate { get; private set; }
    public bool IsActive { get; private set; }

    private TenantCurrency() => CurrencyCode = "SAR";

    public TenantCurrency(Guid tenantId, string currencyCode, bool isPrimary, decimal exchangeRate = 1m)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(currencyCode) || currencyCode.Length != 3)
            throw new ArgumentException("Currency code must be a 3-letter ISO code.", nameof(currencyCode));
        if (exchangeRate <= 0)
            throw new ArgumentOutOfRangeException(nameof(exchangeRate), "Exchange rate must be positive.");

        TenantId = tenantId;
        CurrencyCode = currencyCode.ToUpperInvariant();
        IsPrimary = isPrimary;
        ExchangeRate = exchangeRate;
        IsActive = true;
    }
}

/// <summary>
/// Tenant-scoped POS payment method configuration.
/// </summary>
public sealed class TenantPaymentMethod : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public PaymentMethodType MethodType { get; private set; }
    public string DisplayNameAr { get; private set; }
    public string DisplayNameEn { get; private set; }
    public bool IsActive { get; private set; }
    public int SortOrder { get; private set; }

    private TenantPaymentMethod()
    {
        DisplayNameAr = string.Empty;
        DisplayNameEn = string.Empty;
    }

    public TenantPaymentMethod(
        Guid tenantId,
        PaymentMethodType methodType,
        string displayNameAr,
        string displayNameEn,
        int sortOrder = 0)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(displayNameAr))
            throw new ArgumentException("Arabic display name cannot be empty.", nameof(displayNameAr));
        if (string.IsNullOrWhiteSpace(displayNameEn))
            throw new ArgumentException("English display name cannot be empty.", nameof(displayNameEn));

        TenantId = tenantId;
        MethodType = methodType;
        DisplayNameAr = displayNameAr;
        DisplayNameEn = displayNameEn;
        SortOrder = sortOrder;
        IsActive = true;
    }
}
