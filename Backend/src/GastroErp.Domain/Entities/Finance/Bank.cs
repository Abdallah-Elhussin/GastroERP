using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;

namespace GastroErp.Domain.Entities.Finance;

/// <summary>
/// بنك تشغيلي مرتبط بحساب دليل محاسبي — ليس بديلاً عن دليل الحسابات.
/// </summary>
public sealed class Bank : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public int Number { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public string? Code { get; private set; }
    public string? SwiftCode { get; private set; }
    public string? DefaultIban { get; private set; }
    public Guid CompanyId { get; private set; }
    public Guid BranchId { get; private set; }
    public Guid ChartOfAccountId { get; private set; }
    public Guid BaseCurrencyId { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateOnly? DeactivatedAt { get; private set; }
    public string? DeactivationReason { get; private set; }
    public bool IsSystem { get; private set; }
    public int SortOrder { get; private set; }

    private readonly List<BankAccountDetail> _accounts = [];
    public IReadOnlyCollection<BankAccountDetail> Accounts => _accounts.AsReadOnly();

    private Bank()
    {
        NameAr = string.Empty;
    }

    public static Bank Create(
        Guid tenantId,
        int number,
        string nameAr,
        Guid companyId,
        Guid branchId,
        Guid chartOfAccountId,
        Guid baseCurrencyId,
        string? nameEn = null,
        string? code = null,
        string? swiftCode = null,
        string? defaultIban = null,
        int sortOrder = 0,
        bool isSystem = false)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId required.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        if (companyId == Guid.Empty || branchId == Guid.Empty)
            throw new BusinessException(ErrorCodes.RequiredField);
        if (chartOfAccountId == Guid.Empty || baseCurrencyId == Guid.Empty)
            throw new BusinessException(ErrorCodes.RequiredField);

        return new Bank
        {
            TenantId = tenantId,
            Number = number,
            NameAr = nameAr.Trim(),
            NameEn = Normalize(nameEn),
            Code = NormalizeUpper(code),
            SwiftCode = NormalizeUpper(swiftCode),
            DefaultIban = NormalizeUpper(defaultIban),
            CompanyId = companyId,
            BranchId = branchId,
            ChartOfAccountId = chartOfAccountId,
            BaseCurrencyId = baseCurrencyId,
            SortOrder = sortOrder,
            IsSystem = isSystem,
            IsActive = true
        };
    }

    public void Update(
        string nameAr,
        string? nameEn,
        string? code,
        string? swiftCode,
        string? defaultIban,
        Guid companyId,
        Guid branchId,
        Guid baseCurrencyId,
        int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        if (companyId == Guid.Empty || branchId == Guid.Empty || baseCurrencyId == Guid.Empty)
            throw new BusinessException(ErrorCodes.RequiredField);

        NameAr = nameAr.Trim();
        NameEn = Normalize(nameEn);
        Code = NormalizeUpper(code);
        SwiftCode = NormalizeUpper(swiftCode);
        DefaultIban = NormalizeUpper(defaultIban);
        CompanyId = companyId;
        BranchId = branchId;
        BaseCurrencyId = baseCurrencyId;
        SortOrder = sortOrder;
    }

    public void ChangeChartOfAccount(Guid chartOfAccountId)
    {
        if (chartOfAccountId == Guid.Empty) throw new BusinessException(ErrorCodes.RequiredField);
        ChartOfAccountId = chartOfAccountId;
    }

    public void Activate()
    {
        IsActive = true;
        DeactivatedAt = null;
        DeactivationReason = null;
    }

    public void Deactivate(DateOnly? deactivatedAt, string? reason)
    {
        IsActive = false;
        DeactivatedAt = deactivatedAt ?? DateOnly.FromDateTime(DateTime.UtcNow);
        DeactivationReason = Normalize(reason);
    }

    public void ReplaceAccounts(IEnumerable<BankAccountDetail> accounts)
    {
        _accounts.Clear();
        var list = accounts.ToList();
        if (list.Count == 0) return;

        var defaults = list.Count(a => a.IsDefault && a.IsActive);
        if (defaults > 1)
            throw new BusinessException(ErrorCodes.BankAccountDefaultInvalid);

        foreach (var account in list)
            _accounts.Add(account);
    }

    public void EnsureCanDelete()
    {
        if (IsSystem)
            throw new BusinessException(ErrorCodes.BankProtected);
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeUpper(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();
}

/// <summary>حساب بنكي فرعي بعملة ورقم حساب وحدود.</summary>
public sealed class BankAccountDetail
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid BankId { get; private set; }
    public Guid CurrencyId { get; private set; }
    public string AccountNumber { get; private set; } = string.Empty;
    public string? Iban { get; private set; }
    public decimal? MinBalance { get; private set; }
    public decimal? MaxBalance { get; private set; }
    public decimal? MinTransaction { get; private set; }
    public decimal? MaxTransaction { get; private set; }
    public decimal? DailyTransferLimit { get; private set; }
    public bool AllowExceedLimits { get; private set; }
    public bool AllowWithdraw { get; private set; } = true;
    public bool AllowDeposit { get; private set; } = true;
    public bool AllowTransfer { get; private set; } = true;
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int SortOrder { get; private set; }

    private BankAccountDetail() { }

    public static BankAccountDetail Create(
        Guid bankId,
        Guid currencyId,
        string accountNumber,
        string? iban = null,
        decimal? minBalance = null,
        decimal? maxBalance = null,
        decimal? minTransaction = null,
        decimal? maxTransaction = null,
        decimal? dailyTransferLimit = null,
        bool allowExceedLimits = false,
        bool allowWithdraw = true,
        bool allowDeposit = true,
        bool allowTransfer = true,
        bool isDefault = false,
        bool isActive = true,
        int sortOrder = 0)
    {
        if (currencyId == Guid.Empty) throw new BusinessException(ErrorCodes.RequiredField);
        if (string.IsNullOrWhiteSpace(accountNumber)) throw new BusinessException(ErrorCodes.RequiredField);
        if (minBalance is decimal minB && maxBalance is decimal maxB && minB > maxB)
            throw new BusinessException(ErrorCodes.BankAccountLimitInvalid);
        if (minTransaction is decimal minT && maxTransaction is decimal maxT && minT > maxT)
            throw new BusinessException(ErrorCodes.BankAccountLimitInvalid);

        return new BankAccountDetail
        {
            BankId = bankId,
            CurrencyId = currencyId,
            AccountNumber = accountNumber.Trim(),
            Iban = string.IsNullOrWhiteSpace(iban) ? null : iban.Trim().ToUpperInvariant(),
            MinBalance = minBalance,
            MaxBalance = maxBalance,
            MinTransaction = minTransaction,
            MaxTransaction = maxTransaction,
            DailyTransferLimit = dailyTransferLimit,
            AllowExceedLimits = allowExceedLimits,
            AllowWithdraw = allowWithdraw,
            AllowDeposit = allowDeposit,
            AllowTransfer = allowTransfer,
            IsDefault = isDefault,
            IsActive = isActive,
            SortOrder = sortOrder
        };
    }
}
