using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Finance;

namespace GastroErp.Domain.Entities.Finance;

/// <summary>ChartOfAccount — دليل الحسابات (Aggregate Root)</summary>
public sealed class ChartOfAccount : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public string AccountNumber { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public Guid? ParentAccountId { get; private set; }
    public Guid? AccountClassificationId { get; private set; }
    public AccountType AccountType { get; private set; }
    public AccountCategory AccountCategory { get; private set; }
    public string Currency { get; private set; }
    public bool IsPostingAllowed { get; private set; }
    public bool IsSummaryAccount { get; private set; }
    public bool IsSystemAccount { get; private set; }
    public bool IsActive { get; private set; }
    public int SortOrder { get; private set; }
    public string? Notes { get; private set; }

    private ChartOfAccount()
    {
        AccountNumber = string.Empty;
        NameAr = string.Empty;
        Currency = "SAR";
    }

    public static ChartOfAccount Create(
        Guid tenantId, string accountNumber, string nameAr, AccountType accountType,
        AccountCategory category, bool isPostingAllowed = true, bool isSummaryAccount = false,
        Guid? parentAccountId = null, string? nameEn = null, string currency = "SAR", int sortOrder = 0,
        string? notes = null, bool isSystemAccount = false, Guid? accountClassificationId = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(accountNumber)) throw new BusinessException(ErrorCodes.AccountNumberRequired);
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);

        var account = new ChartOfAccount
        {
            TenantId = tenantId,
            AccountNumber = accountNumber.Trim(),
            NameAr = nameAr.Trim(),
            NameEn = string.IsNullOrWhiteSpace(nameEn) ? null : nameEn.Trim(),
            ParentAccountId = parentAccountId,
            AccountClassificationId = accountClassificationId,
            AccountType = accountType,
            AccountCategory = category,
            Currency = currency.ToUpperInvariant(),
            IsPostingAllowed = isPostingAllowed && !isSummaryAccount,
            IsSummaryAccount = isSummaryAccount,
            IsSystemAccount = isSystemAccount,
            IsActive = true,
            SortOrder = sortOrder,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim()
        };

        account.RaiseDomainEvent(new AccountCreatedEvent(account.Id, tenantId, accountNumber, nameAr));
        return account;
    }

    public void Update(
        string nameAr,
        string? nameEn,
        AccountCategory category,
        bool isSummaryAccount,
        int sortOrder,
        string currency,
        string? notes,
        bool allowCategoryChange,
        Guid? accountClassificationId = null)
    {
        if (IsSystemAccount && category != AccountCategory)
            throw new BusinessException(ErrorCodes.AccountSystemProtected);

        if (!allowCategoryChange && category != AccountCategory)
            throw new BusinessException(ErrorCodes.AccountHasTransactions);

        NameAr = nameAr.Trim();
        NameEn = string.IsNullOrWhiteSpace(nameEn) ? null : nameEn.Trim();
        if (allowCategoryChange && !IsSystemAccount)
            AccountCategory = category;
        IsSummaryAccount = isSummaryAccount;
        IsPostingAllowed = !isSummaryAccount && IsActive;
        SortOrder = sortOrder;
        Currency = string.IsNullOrWhiteSpace(currency) ? Currency : currency.Trim().ToUpperInvariant();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        AccountClassificationId = accountClassificationId;
    }

    public void SetClassification(Guid? accountClassificationId)
        => AccountClassificationId = accountClassificationId;

    public void MarkAsSystemAccount(bool isSystem = true) => IsSystemAccount = isSystem;

    public void Reparent(Guid? newParentAccountId)
    {
        if (newParentAccountId == Id)
            throw new BusinessException(ErrorCodes.AccountInvalidParent);

        ParentAccountId = newParentAccountId;
    }

    public void SetSortOrder(int sortOrder) => SortOrder = sortOrder;

    public void Renumber(string newAccountNumber)
    {
        if (IsSystemAccount)
            throw new BusinessException(ErrorCodes.AccountSystemProtected, "System account number cannot be changed.");
        if (string.IsNullOrWhiteSpace(newAccountNumber))
            throw new BusinessException(ErrorCodes.AccountNumberRequired);

        AccountNumber = newAccountNumber.Trim();
    }

    public void Activate()
    {
        IsActive = true;
        if (!IsSummaryAccount) IsPostingAllowed = true;
    }

    public void Deactivate()
    {
        IsActive = false;
        IsPostingAllowed = false;
    }

    public void EnsureCanPost()
    {
        if (!IsActive) throw new BusinessException(ErrorCodes.AccountInactive);
        if (!IsPostingAllowed || IsSummaryAccount) throw new BusinessException(ErrorCodes.AccountPostingNotAllowed);
    }

    public void EnsureCanDelete()
    {
        if (IsSystemAccount)
            throw new BusinessException(ErrorCodes.AccountSystemProtected, "System accounts cannot be deleted.");
    }
}
