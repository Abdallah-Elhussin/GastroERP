using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;

namespace GastroErp.Domain.Entities.Finance;

/// <summary>Where a tax code may apply in commercial documents.</summary>
public enum TaxAppliesTo
{
    Sales = 1,
    Purchases = 2,
    Both = 3
}

/// <summary>VAT / GST calculation category (ZATCA-aligned).</summary>
public enum TaxCodeCalculationMethod
{
    Standard = 1,
    Exempt = 2,
    ZeroRated = 3
}

/// <summary>
/// Finance tax code master: code, bilingual names, GL postings, and dated rate history.
/// Distinct from <see cref="TaxRegistrationProfile"/> (ZATCA identity) and Invoicing TaxRate.
/// </summary>
public sealed class TaxCode : AuditableBaseEntity, ITenantEntity
{
    private readonly List<TaxCodeRate> _rates = [];

    public Guid TenantId { get; private set; }
    public int Number { get; private set; }
    public Guid CompanyId { get; private set; }
    public Guid? BranchId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string NameAr { get; private set; } = string.Empty;
    public string? NameEn { get; private set; }
    public TaxAppliesTo AppliesTo { get; private set; } = TaxAppliesTo.Both;
    public TaxCodeCalculationMethod CalculationMethod { get; private set; } = TaxCodeCalculationMethod.Standard;
    public Guid? SalesAccountId { get; private set; }
    public Guid? PurchaseAccountId { get; private set; }
    public bool PriceIncludesTax { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool HasBeenUsed { get; private set; }

    public IReadOnlyCollection<TaxCodeRate> Rates => _rates.AsReadOnly();

    private TaxCode() { }

    public static TaxCode Create(
        Guid tenantId,
        int number,
        Guid companyId,
        string code,
        string nameAr,
        string? nameEn = null,
        Guid? branchId = null,
        TaxAppliesTo appliesTo = TaxAppliesTo.Both,
        TaxCodeCalculationMethod calculationMethod = TaxCodeCalculationMethod.Standard,
        Guid? salesAccountId = null,
        Guid? purchaseAccountId = null,
        bool priceIncludesTax = false,
        bool isActive = true)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId is required.", nameof(tenantId));
        if (companyId == Guid.Empty) throw new BusinessException(ErrorCodes.RequiredField, "Company is required.");
        if (number < 1) throw new ArgumentOutOfRangeException(nameof(number));

        var entity = new TaxCode
        {
            TenantId = tenantId,
            Number = number,
            CompanyId = companyId,
            BranchId = branchId,
            AppliesTo = appliesTo,
            CalculationMethod = calculationMethod,
            SalesAccountId = salesAccountId,
            PurchaseAccountId = purchaseAccountId,
            PriceIncludesTax = priceIncludesTax,
            IsActive = isActive
        };
        entity.SetCode(code);
        entity.SetNames(nameAr, nameEn);
        return entity;
    }

    public void Update(
        Guid? branchId,
        string code,
        string nameAr,
        string? nameEn,
        TaxAppliesTo appliesTo,
        TaxCodeCalculationMethod calculationMethod,
        Guid? salesAccountId,
        Guid? purchaseAccountId,
        bool priceIncludesTax,
        bool isActive)
    {
        BranchId = branchId;
        SetCode(code);
        SetNames(nameAr, nameEn);
        AppliesTo = appliesTo;
        CalculationMethod = calculationMethod;
        SalesAccountId = salesAccountId;
        PurchaseAccountId = purchaseAccountId;
        PriceIncludesTax = priceIncludesTax;
        IsActive = isActive;
        NormalizeRatesForMethod();
    }

    public void SetActive(bool isActive) => IsActive = isActive;

    public void MarkAsUsed() => HasBeenUsed = true;

    public void EnsureCanDelete()
    {
        if (HasBeenUsed)
            throw new BusinessException(ErrorCodes.TaxCodeInUse,
                "Tax code is used in transactions and cannot be deleted. Deactivate it instead.");
    }

    public TaxCodeRate AddRate(DateOnly fromDate, DateOnly? toDate, decimal rate)
    {
        rate = NormalizeRate(rate);
        ValidatePeriod(fromDate, toDate, null);
        var row = TaxCodeRate.Create(Id, fromDate, toDate, rate);
        _rates.Add(row);
        return row;
    }

    public void UpdateRate(Guid rateId, DateOnly fromDate, DateOnly? toDate, decimal rate)
    {
        var row = _rates.FirstOrDefault(r => r.Id == rateId)
            ?? throw new BusinessException(ErrorCodes.TaxCodeRateNotFound, "Tax rate period not found.");
        rate = NormalizeRate(rate);
        ValidatePeriod(fromDate, toDate, rateId);
        row.Update(fromDate, toDate, rate);
    }

    public void RemoveRate(Guid rateId)
    {
        var row = _rates.FirstOrDefault(r => r.Id == rateId)
            ?? throw new BusinessException(ErrorCodes.TaxCodeRateNotFound, "Tax rate period not found.");
        _rates.Remove(row);
    }

    public void ReplaceRates(IEnumerable<(DateOnly FromDate, DateOnly? ToDate, decimal Rate)> rates)
    {
        _rates.Clear();
        foreach (var (from, to, rate) in rates.OrderBy(r => r.FromDate))
            AddRate(from, to, rate);
    }

    public decimal? CurrentRate(DateOnly? asOf = null)
    {
        var day = asOf ?? DateOnly.FromDateTime(DateTime.UtcNow);
        return _rates
            .Where(r => r.FromDate <= day && (r.ToDate == null || r.ToDate >= day))
            .OrderByDescending(r => r.FromDate)
            .Select(r => (decimal?)r.Rate)
            .FirstOrDefault();
    }

    private void SetCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new BusinessException(ErrorCodes.RequiredField, "Tax code is required.");
        var value = code.Trim().ToUpperInvariant();
        if (value.Length > 20)
            throw new BusinessException(ErrorCodes.RequiredField, "Tax code max length is 20.");
        Code = value;
    }

    private void SetNames(string nameAr, string? nameEn)
    {
        if (string.IsNullOrWhiteSpace(nameAr))
            throw new BusinessException(ErrorCodes.RequiredField, "Arabic name is required.");
        NameAr = nameAr.Trim();
        if (NameAr.Length > 150)
            throw new BusinessException(ErrorCodes.RequiredField, "Arabic name max length is 150.");
        NameEn = string.IsNullOrWhiteSpace(nameEn) ? null : nameEn.Trim();
        if (NameEn is { Length: > 150 })
            throw new BusinessException(ErrorCodes.RequiredField, "English name max length is 150.");
    }

    private decimal NormalizeRate(decimal rate)
    {
        if (CalculationMethod is TaxCodeCalculationMethod.Exempt or TaxCodeCalculationMethod.ZeroRated)
            return 0m;
        if (rate < 0 || rate > 100)
            throw new BusinessException(ErrorCodes.TaxCodeRateInvalid, "Tax rate must be between 0 and 100.");
        return Math.Round(rate, 2, MidpointRounding.AwayFromZero);
    }

    private void NormalizeRatesForMethod()
    {
        if (CalculationMethod is not (TaxCodeCalculationMethod.Exempt or TaxCodeCalculationMethod.ZeroRated))
            return;
        foreach (var rate in _rates.Where(r => r.Rate != 0))
            rate.Update(rate.FromDate, rate.ToDate, 0m);
    }

    private void ValidatePeriod(DateOnly fromDate, DateOnly? toDate, Guid? excludeId)
    {
        if (toDate.HasValue && toDate.Value < fromDate)
            throw new BusinessException(ErrorCodes.TaxCodeRateInvalid, "To date cannot be before from date.");

        foreach (var other in _rates.Where(r => excludeId == null || r.Id != excludeId))
        {
            var otherEnd = other.ToDate ?? DateOnly.MaxValue;
            var newEnd = toDate ?? DateOnly.MaxValue;
            if (fromDate <= otherEnd && other.FromDate <= newEnd)
                throw new BusinessException(ErrorCodes.TaxCodeRateOverlap,
                    "Overlapping tax rate periods are not allowed for the same tax code.");
        }
    }
}

/// <summary>Dated tax rate line for a <see cref="TaxCode"/> (historical / ZATCA rate changes).</summary>
public sealed class TaxCodeRate
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid TaxCodeId { get; private set; }
    public DateOnly FromDate { get; private set; }
    public DateOnly? ToDate { get; private set; }
    public decimal Rate { get; private set; }

    private TaxCodeRate() { }

    internal static TaxCodeRate Create(Guid taxCodeId, DateOnly fromDate, DateOnly? toDate, decimal rate)
    {
        return new TaxCodeRate
        {
            TaxCodeId = taxCodeId,
            FromDate = fromDate,
            ToDate = toDate,
            Rate = rate
        };
    }

    internal void Update(DateOnly fromDate, DateOnly? toDate, decimal rate)
    {
        FromDate = fromDate;
        ToDate = toDate;
        Rate = rate;
    }
}
