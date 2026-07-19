using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;

namespace GastroErp.Domain.Entities.Finance;

/// <summary>طريقة إقفال الأرباح والخسائر.</summary>
public enum ClosingMethod
{
    SingleSummary = 1,
    DirectToRetainedEarnings = 2,
    ByProfitCenter = 3,
    ByBranch = 4
}

/// <summary>
/// إعدادات الأستاذ العام حسب الشركة والفرع — منفصلة عن AccountingSettings (دليل الحسابات).
/// </summary>
public sealed class GeneralLedgerSetting : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public int Number { get; private set; }
    public Guid CompanyId { get; private set; }
    public Guid BranchId { get; private set; }

    public int VoucherNumberLength { get; private set; } = 8;
    public int DecimalPlaces { get; private set; } = 2;
    public bool ShowDateInReports { get; private set; } = true;

    public bool ShowPostingIndicator { get; private set; } = true;
    public bool AutoPostReceiptChecks { get; private set; }
    public bool AutoPostPaymentChecks { get; private set; }
    public bool UseBudgetPerCurrency { get; private set; }
    /// <summary>إظهار عمود الحساب التحليلي في تفاصيل السندات (اختياري للمطاعم).</summary>
    public bool UseAnalyticalAccounts { get; private set; }

    public bool AllowZeroEffectEntries { get; private set; }
    public bool RequireJournalType { get; private set; }
    public bool AllowManualTaxEntries { get; private set; }
    public bool RequireReferenceNumber { get; private set; }

    public ClosingMethod ClosingMethod { get; private set; } = ClosingMethod.SingleSummary;
    public bool IsSystem { get; private set; }

    private GeneralLedgerSetting() { }

    public static GeneralLedgerSetting Create(
        Guid tenantId,
        int number,
        Guid companyId,
        Guid branchId,
        int voucherNumberLength = 8,
        int decimalPlaces = 2,
        bool showDateInReports = true,
        bool showPostingIndicator = true,
        bool autoPostReceiptChecks = false,
        bool autoPostPaymentChecks = false,
        bool useBudgetPerCurrency = false,
        bool allowZeroEffectEntries = false,
        bool requireJournalType = false,
        bool allowManualTaxEntries = false,
        bool requireReferenceNumber = false,
        ClosingMethod closingMethod = ClosingMethod.SingleSummary,
        bool isSystem = false,
        bool useAnalyticalAccounts = false)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId required.", nameof(tenantId));
        if (companyId == Guid.Empty || branchId == Guid.Empty)
            throw new BusinessException(ErrorCodes.RequiredField);

        var setting = new GeneralLedgerSetting
        {
            TenantId = tenantId,
            Number = number,
            CompanyId = companyId,
            BranchId = branchId,
            IsSystem = isSystem
        };

        setting.Apply(
            voucherNumberLength, decimalPlaces, showDateInReports,
            showPostingIndicator, autoPostReceiptChecks, autoPostPaymentChecks, useBudgetPerCurrency,
            allowZeroEffectEntries, requireJournalType, allowManualTaxEntries, requireReferenceNumber,
            closingMethod, useAnalyticalAccounts);

        return setting;
    }

    public void Update(
        Guid companyId,
        Guid branchId,
        int voucherNumberLength,
        int decimalPlaces,
        bool showDateInReports,
        bool showPostingIndicator,
        bool autoPostReceiptChecks,
        bool autoPostPaymentChecks,
        bool useBudgetPerCurrency,
        bool allowZeroEffectEntries,
        bool requireJournalType,
        bool allowManualTaxEntries,
        bool requireReferenceNumber,
        ClosingMethod closingMethod,
        bool useAnalyticalAccounts)
    {
        if (companyId == Guid.Empty || branchId == Guid.Empty)
            throw new BusinessException(ErrorCodes.RequiredField);

        CompanyId = companyId;
        BranchId = branchId;
        Apply(
            voucherNumberLength, decimalPlaces, showDateInReports,
            showPostingIndicator, autoPostReceiptChecks, autoPostPaymentChecks, useBudgetPerCurrency,
            allowZeroEffectEntries, requireJournalType, allowManualTaxEntries, requireReferenceNumber,
            closingMethod, useAnalyticalAccounts);
    }

    public void EnsureCanDelete()
    {
        if (IsSystem)
            throw new BusinessException(ErrorCodes.GeneralLedgerSettingProtected);
    }

    private void Apply(
        int voucherNumberLength,
        int decimalPlaces,
        bool showDateInReports,
        bool showPostingIndicator,
        bool autoPostReceiptChecks,
        bool autoPostPaymentChecks,
        bool useBudgetPerCurrency,
        bool allowZeroEffectEntries,
        bool requireJournalType,
        bool allowManualTaxEntries,
        bool requireReferenceNumber,
        ClosingMethod closingMethod,
        bool useAnalyticalAccounts)
    {
        if (voucherNumberLength is < 4 or > 12)
            throw new BusinessException(ErrorCodes.GeneralLedgerSettingVoucherLengthInvalid);
        if (decimalPlaces is < 0 or > 4)
            throw new BusinessException(ErrorCodes.GeneralLedgerSettingDecimalPlacesInvalid);
        if (!Enum.IsDefined(closingMethod))
            throw new BusinessException(ErrorCodes.RequiredField);

        VoucherNumberLength = voucherNumberLength;
        DecimalPlaces = decimalPlaces;
        ShowDateInReports = showDateInReports;
        ShowPostingIndicator = showPostingIndicator;
        AutoPostReceiptChecks = autoPostReceiptChecks;
        AutoPostPaymentChecks = autoPostPaymentChecks;
        UseBudgetPerCurrency = useBudgetPerCurrency;
        UseAnalyticalAccounts = useAnalyticalAccounts;
        AllowZeroEffectEntries = allowZeroEffectEntries;
        RequireJournalType = requireJournalType;
        AllowManualTaxEntries = allowManualTaxEntries;
        RequireReferenceNumber = requireReferenceNumber;
        ClosingMethod = closingMethod;
    }
}
