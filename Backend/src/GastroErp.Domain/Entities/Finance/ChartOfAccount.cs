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
    public AccountType AccountType { get; private set; }
    public AccountCategory AccountCategory { get; private set; }
    public string Currency { get; private set; }
    public bool IsPostingAllowed { get; private set; }
    public bool IsSummaryAccount { get; private set; }
    public bool IsActive { get; private set; }
    public int SortOrder { get; private set; }

    private ChartOfAccount()
    {
        AccountNumber = string.Empty;
        NameAr = string.Empty;
        Currency = "SAR";
    }

    public static ChartOfAccount Create(
        Guid tenantId, string accountNumber, string nameAr, AccountType accountType,
        AccountCategory category, bool isPostingAllowed = true, bool isSummaryAccount = false,
        Guid? parentAccountId = null, string? nameEn = null, string currency = "SAR", int sortOrder = 0)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(accountNumber)) throw new BusinessException(ErrorCodes.AccountNumberRequired);
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);

        var account = new ChartOfAccount
        {
            TenantId = tenantId,
            AccountNumber = accountNumber.Trim(),
            NameAr = nameAr,
            NameEn = nameEn,
            ParentAccountId = parentAccountId,
            AccountType = accountType,
            AccountCategory = category,
            Currency = currency.ToUpperInvariant(),
            IsPostingAllowed = isPostingAllowed && !isSummaryAccount,
            IsSummaryAccount = isSummaryAccount,
            IsActive = true,
            SortOrder = sortOrder
        };

        account.RaiseDomainEvent(new AccountCreatedEvent(account.Id, tenantId, accountNumber, nameAr));
        return account;
    }

    public void Update(string nameAr, string? nameEn, AccountCategory category, bool isSummaryAccount, int sortOrder)
    {
        NameAr = nameAr;
        NameEn = nameEn;
        AccountCategory = category;
        IsSummaryAccount = isSummaryAccount;
        IsPostingAllowed = !isSummaryAccount && IsActive;
        SortOrder = sortOrder;
    }

    public void Activate() { IsActive = true; if (!IsSummaryAccount) IsPostingAllowed = true; }
    public void Deactivate() { IsActive = false; IsPostingAllowed = false; }

    public void EnsureCanPost()
    {
        if (!IsActive) throw new BusinessException(ErrorCodes.AccountInactive);
        if (!IsPostingAllowed || IsSummaryAccount) throw new BusinessException(ErrorCodes.AccountPostingNotAllowed);
    }
}
