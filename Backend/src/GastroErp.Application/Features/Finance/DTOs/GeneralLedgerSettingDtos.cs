using GastroErp.Domain.Entities.Finance;

namespace GastroErp.Application.Features.Finance.DTOs;

public record UpsertGeneralLedgerSettingDto(
    Guid CompanyId,
    Guid BranchId,
    int VoucherNumberLength = 8,
    int DecimalPlaces = 2,
    bool ShowDateInReports = true,
    bool ShowPostingIndicator = true,
    bool AutoPostReceiptChecks = false,
    bool AutoPostPaymentChecks = false,
    bool UseBudgetPerCurrency = false,
    bool UseAnalyticalAccounts = false,
    bool AllowZeroEffectEntries = false,
    bool RequireJournalType = false,
    bool AllowManualTaxEntries = false,
    bool RequireReferenceNumber = false,
    ClosingMethod ClosingMethod = ClosingMethod.SingleSummary);

public record GeneralLedgerSettingDto(
    Guid Id,
    int Number,
    Guid CompanyId,
    string? CompanyNameAr,
    Guid BranchId,
    string? BranchNameAr,
    int VoucherNumberLength,
    int DecimalPlaces,
    bool ShowDateInReports,
    bool ShowPostingIndicator,
    bool AutoPostReceiptChecks,
    bool AutoPostPaymentChecks,
    bool UseBudgetPerCurrency,
    bool UseAnalyticalAccounts,
    bool AllowZeroEffectEntries,
    bool RequireJournalType,
    bool AllowManualTaxEntries,
    bool RequireReferenceNumber,
    ClosingMethod ClosingMethod,
    string ClosingMethodCode,
    bool IsSystem,
    DateTimeOffset CreatedAt,
    string? CreatedBy,
    DateTimeOffset? UpdatedAt,
    string? UpdatedBy);

public record GeneralLedgerSettingFilterDto(
    Guid? CompanyId = null,
    Guid? BranchId = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 200);
