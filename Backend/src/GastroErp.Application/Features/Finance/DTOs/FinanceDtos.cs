using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Finance.DTOs;

// ─── Chart of Accounts ────────────────────────────────────────────────────────

public record CreateAccountDto(
    string AccountNumber, string NameAr, AccountType AccountType, AccountCategory AccountCategory,
    bool IsSummaryAccount = false, Guid? ParentAccountId = null, string? NameEn = null,
    string Currency = "SAR", int SortOrder = 0, string? Notes = null, bool IsSystemAccount = false,
    Guid? AccountClassificationId = null);

public record UpdateAccountDto(
    string NameAr, string? NameEn, AccountCategory AccountCategory,
    bool IsSummaryAccount, int SortOrder, string Currency = "SAR", string? Notes = null,
    Guid? AccountClassificationId = null);

public record ReparentAccountDto(Guid? NewParentAccountId);

public record RenumberAccountDto(string NewAccountNumber);

public record AccountDto(
    Guid Id, string AccountNumber, string NameAr, string? NameEn,
    Guid? ParentAccountId, AccountType AccountType, AccountCategory AccountCategory,
    string Currency, bool IsPostingAllowed, bool IsSummaryAccount, bool IsActive, int SortOrder,
    bool IsSystemAccount = false, string? Notes = null, Guid? AccountClassificationId = null);

public record AccountTreeNodeDto(
    Guid Id, string AccountNumber, string NameAr, string? NameEn,
    Guid? ParentAccountId, AccountType AccountType, AccountCategory AccountCategory,
    string Currency, bool IsPostingAllowed, bool IsSummaryAccount, bool IsSystemAccount,
    bool IsActive, int SortOrder, string? Notes, Guid? AccountClassificationId,
    IReadOnlyList<AccountTreeNodeDto> Children);

public record AccountFilterDto(
    AccountType? AccountType = null, bool? IsActive = null,
    string? Search = null, int Page = 1, int PageSize = 50);

public record AccountImportRowDto(
    string AccountNumber, string NameAr, string? NameEn, string? ParentAccountNumber,
    AccountType AccountType, AccountCategory AccountCategory, string Currency,
    bool IsSummaryAccount, int SortOrder, string? Notes);

public record AccountImportPreviewDto(
    int TotalRows, int ValidRows, int InvalidRows,
    IReadOnlyList<string> Errors, IReadOnlyList<AccountImportRowDto> Rows);

public record AccountExportRowDto(
    string AccountNumber, string NameAr, string? NameEn, string? ParentAccountNumber,
    int Level, AccountType AccountType, AccountCategory AccountCategory,
    string Currency, bool IsSummaryAccount, bool IsSystemAccount, bool IsActive, int SortOrder, string? Notes);

// ─── Accounting Settings ──────────────────────────────────────────────────────

public record AccountingSettingsDto(
    Guid Id, Guid TenantId, Guid? CompanyId,
    int AccountNumberMaxLength, int MaxTreeLevels, string LevelLengthsCsv, string LevelSeparator,
    Guid? CashAccountId, Guid? BankAccountId, Guid? InventoryAccountId, Guid? CogsAccountId,
    Guid? SalesRevenueAccountId, Guid? PurchaseAccountId, Guid? AccountsReceivableAccountId,
    Guid? AccountsPayableAccountId, Guid? VatInputAccountId, Guid? VatOutputAccountId,
    Guid? DiscountAccountId, Guid? RoundOffAccountId, Guid? OpeningBalanceAccountId,
    Guid? RetainedEarningsAccountId, Guid? PayrollExpenseAccountId, Guid? PayrollLiabilityAccountId,
    Guid? ProductionVarianceAccountId, Guid? InventoryAdjustmentAccountId, Guid? WasteAccountId,
    Guid? DeliveryRevenueAccountId, Guid? DeliveryExpenseAccountId, Guid? KitchenConsumptionAccountId,
    Guid? CustomerAdvancesAccountId, Guid? SupplierAdvancesAccountId, Guid? ExchangeDifferenceAccountId,
    Guid? GrniAccountId, Guid? FixedAssetAccountId,
    bool AutoPostSales, bool AutoPostPurchases, bool AutoPostGoodsReceipt, bool AutoPostGoodsIssue,
    bool AutoPostStockTransfer, bool AutoPostWaste, bool AutoPostProduction, bool AutoPostPayroll);

public record UpdateAccountingSettingsDto(
    int AccountNumberMaxLength, int MaxTreeLevels, string LevelLengthsCsv, string LevelSeparator,
    Guid? CashAccountId, Guid? BankAccountId, Guid? InventoryAccountId, Guid? CogsAccountId,
    Guid? SalesRevenueAccountId, Guid? PurchaseAccountId, Guid? AccountsReceivableAccountId,
    Guid? AccountsPayableAccountId, Guid? VatInputAccountId, Guid? VatOutputAccountId,
    Guid? DiscountAccountId, Guid? RoundOffAccountId, Guid? OpeningBalanceAccountId,
    Guid? RetainedEarningsAccountId, Guid? PayrollExpenseAccountId, Guid? PayrollLiabilityAccountId,
    Guid? ProductionVarianceAccountId, Guid? InventoryAdjustmentAccountId, Guid? WasteAccountId,
    Guid? DeliveryRevenueAccountId, Guid? DeliveryExpenseAccountId, Guid? KitchenConsumptionAccountId,
    Guid? CustomerAdvancesAccountId, Guid? SupplierAdvancesAccountId, Guid? ExchangeDifferenceAccountId,
    Guid? GrniAccountId, Guid? FixedAssetAccountId,
    bool AutoPostSales, bool AutoPostPurchases, bool AutoPostGoodsReceipt, bool AutoPostGoodsIssue,
    bool AutoPostStockTransfer, bool AutoPostWaste, bool AutoPostProduction, bool AutoPostPayroll);

// ─── Journal ────────────────────────────────────────────────────────────────────

public record JournalLineDto(
    Guid? Id, Guid ChartOfAccountId, Guid? CostCenterId,
    decimal Debit, decimal Credit, string? Description,
    string Currency = "SAR",
    decimal ExchangeRate = 1m,
    Guid? AnalyticalAccountId = null);

public record CreateJournalDto(
    DateOnly PostingDate, string Description, PostingSource SourceModule,
    Guid? BranchId = null, Guid? CompanyId = null, string? Reference = null,
    Guid? SourceDocumentId = null, IReadOnlyList<JournalLineDto>? Lines = null,
    JournalVoucherType? VoucherType = null,
    Guid? FiscalPeriodId = null);

public record UpdateJournalDto(
    DateOnly PostingDate,
    string Description,
    Guid? BranchId = null,
    Guid? CompanyId = null,
    string? Reference = null,
    IReadOnlyList<JournalLineDto>? Lines = null,
    JournalVoucherType? VoucherType = null,
    Guid? FiscalPeriodId = null);

public record JournalDto(
    Guid Id, string EntryNumber, DateOnly PostingDate, Guid FiscalPeriodId,
    string Description, string? Reference, PostingSource SourceModule,
    Guid? SourceDocumentId, JournalStatus Status, DateTimeOffset? PostedAt,
    decimal TotalDebit, decimal TotalCredit,
    JournalVoucherType VoucherType = JournalVoucherType.Ordinary,
    Guid? CompanyId = null,
    Guid? BranchId = null,
    string? CreatedBy = null,
    DateTimeOffset? CreatedAt = null);

public record JournalDetailDto(
    Guid Id, string EntryNumber, DateOnly PostingDate, Guid FiscalPeriodId,
    string Description, string? Reference, PostingSource SourceModule,
    Guid? SourceDocumentId, JournalStatus Status, DateTimeOffset? PostedAt,
    Guid? CompanyId, Guid? BranchId,
    IReadOnlyList<JournalLineDetailDto> Lines,
    JournalVoucherType VoucherType = JournalVoucherType.Ordinary);

public record JournalLineDetailDto(
    Guid Id, int LineNumber, Guid ChartOfAccountId, string AccountNumber, string AccountName,
    Guid? CostCenterId, decimal Debit, decimal Credit, string Currency, string? Description,
    decimal ExchangeRate = 1m,
    Guid? AnalyticalAccountId = null);

public record JournalFilterDto(
    JournalStatus? Status = null, PostingSource? SourceModule = null,
    DateOnly? FromDate = null, DateOnly? ToDate = null,
    string? Search = null,
    Guid? CompanyId = null,
    Guid? BranchId = null,
    Guid? FiscalPeriodId = null,
    int? FiscalYear = null,
    JournalVoucherType? VoucherType = null,
    string? EntryNumber = null,
    int Page = 1, int PageSize = 50);


// ─── Fiscal Period ──────────────────────────────────────────────────────────────

public record CreateFiscalPeriodDto(
    int FiscalYear,
    byte StartMonth = 1,
    string? Notes = null,
    FiscalPeriodPolicy PeriodPolicy = FiscalPeriodPolicy.Monthly,
    bool GenerateDetails = true);

public record UpdateFiscalPeriodDto(
    byte StartMonth,
    string? Notes = null,
    IReadOnlyList<UpdateFiscalPeriodDetailDto>? Details = null);

public record UpdateFiscalPeriodDetailDto(
    Guid? Id,
    int PeriodNumber,
    string NameAr,
    string NameEn,
    DateOnly StartDate,
    DateOnly EndDate,
    FiscalPeriodStatus Status);

public record FiscalPeriodDetailDto(
    Guid Id,
    int PeriodNumber,
    string NameAr,
    string NameEn,
    DateOnly StartDate,
    DateOnly EndDate,
    FiscalPeriodStatus Status);

public record FiscalPeriodDto(
    Guid Id,
    int FiscalYear,
    byte StartMonth,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    string? Notes,
    FiscalPeriodPolicy PeriodPolicy,
    string PeriodPolicyCode,
    FiscalPeriodStatus Status,
    string StatusCode,
    IReadOnlyList<FiscalPeriodDetailDto> Details,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

// ─── Cost Center ────────────────────────────────────────────────────────────────

public record CreateCostCenterDto(
    string NameAr,
    Guid? BranchId = null,
    string? Code = null,
    string? NameEn = null,
    Guid? DepartmentId = null,
    Guid? ParentCostCenterId = null,
    CostCenterType CostCenterType = CostCenterType.Operational,
    string? Description = null,
    int SortOrder = 0,
    bool UseInPurchases = true,
    bool UseInInventory = true,
    bool UseInProduction = true,
    bool UseInSales = true,
    bool UseInPayroll = true,
    bool UseInAssets = true,
    bool UseInMaintenance = true,
    bool UseInJournals = true,
    IReadOnlyList<Guid>? AllowedAccountIds = null);

public record UpdateCostCenterDto(
    string NameAr,
    string? NameEn = null,
    Guid? DepartmentId = null,
    Guid? ParentCostCenterId = null,
    CostCenterType CostCenterType = CostCenterType.Operational,
    string? Description = null,
    int SortOrder = 0,
    bool UseInPurchases = true,
    bool UseInInventory = true,
    bool UseInProduction = true,
    bool UseInSales = true,
    bool UseInPayroll = true,
    bool UseInAssets = true,
    bool UseInMaintenance = true,
    bool UseInJournals = true,
    IReadOnlyList<Guid>? AllowedAccountIds = null);

public record CostCenterDto(
    Guid Id,
    int Number,
    Guid BranchId,
    Guid? DepartmentId,
    Guid? ParentCostCenterId,
    string Code,
    string NameAr,
    string? NameEn,
    string? Description,
    CostCenterType CostCenterType,
    string CostCenterTypeCode,
    CostCenterStatus Status,
    bool IsActive,
    bool IsSystem,
    int SortOrder,
    int LinkedAccountsCount,
    bool UseInPurchases,
    bool UseInInventory,
    bool UseInProduction,
    bool UseInSales,
    bool UseInPayroll,
    bool UseInAssets,
    bool UseInMaintenance,
    bool UseInJournals,
    IReadOnlyList<Guid> AllowedAccountIds);

public record CostCenterFilterDto(
    Guid? BranchId = null,
    CostCenterStatus? Status = null,
    CostCenterType? CostCenterType = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 200);

// ─── Currency ───────────────────────────────────────────────────────────────────

public record CreateCurrencyDto(
    string Code,
    string NameAr,
    string NameEn,
    decimal CurrentExchangeRate = 1m,
    string? Symbol = null,
    byte DecimalPlaces = 2,
    string? SubUnitNameAr = null,
    string? SubUnitNameEn = null,
    bool IsCompanyCurrency = false,
    bool IsActive = true,
    int SortOrder = 0);

public record UpdateCurrencyDto(
    string NameAr,
    string NameEn,
    string? Symbol = null,
    byte DecimalPlaces = 2,
    string? SubUnitNameAr = null,
    string? SubUnitNameEn = null,
    decimal? CurrentExchangeRate = null,
    bool IsActive = true,
    int SortOrder = 0);

public record CurrencyDto(
    Guid Id,
    int Number,
    string Code,
    string NameAr,
    string NameEn,
    string? Symbol,
    byte DecimalPlaces,
    string? SubUnitNameAr,
    string? SubUnitNameEn,
    decimal CurrentExchangeRate,
    bool IsCompanyCurrency,
    bool IsForeignCurrency,
    CurrencyStatus Status,
    bool IsActive,
    bool IsSystem,
    int SortOrder,
    DateTimeOffset? LastExchangeRateAt,
    string? LastExchangeRateBy);

public record CurrencyFilterDto(
    CurrencyStatus? Status = null,
    bool? IsCompanyCurrency = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 200);

public record CreateCurrencyExchangeRateDto(
    Guid CurrencyId,
    decimal Rate,
    DateOnly StartDate,
    DateOnly? EndDate = null,
    bool IsActive = true,
    string? ChangeReason = null,
    bool AutoClosePreviousOpen = true);

public record UpdateCurrencyExchangeRateDto(
    decimal Rate,
    DateOnly StartDate,
    DateOnly? EndDate = null,
    bool IsActive = true,
    string? ChangeReason = null);

public record CurrencyExchangeRateDto(
    Guid Id,
    int Number,
    Guid CurrencyId,
    string CurrencyCode,
    string CurrencyNameAr,
    decimal Rate,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsActive,
    bool IsOpen,
    string? ChangeReason,
    string? CreatedBy,
    DateTimeOffset CreatedAt,
    string? UpdatedBy,
    DateTimeOffset? UpdatedAt);

public record CurrencyExchangeRateFilterDto(
    Guid? CurrencyId = null,
    DateOnly? AsOfDate = null,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    bool? ActiveOnly = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 200);

// ─── Reports ────────────────────────────────────────────────────────────────────

public record TrialBalanceFilterDto(
    DateOnly? AsOfDate = null, Guid? FiscalPeriodId = null);

public record TrialBalanceLineDto(
    Guid AccountId, string AccountNumber, string AccountName,
    AccountType AccountType, decimal DebitBalance, decimal CreditBalance);

public record GeneralLedgerFilterDto(
    Guid? AccountId = null,
    Guid? CompanyId = null,
    Guid? BranchId = null,
    Guid? FiscalPeriodId = null,
    int? FiscalYear = null,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null,
    Guid? CostCenterId = null,
    Guid? ParentAccountId = null,
    AccountType? AccountType = null,
    string? Currency = null,
    PostingSource? SourceModule = null,
    string? DocumentNumber = null,
    string? Search = null,
    Guid? PostedBy = null,
    bool IncludeOpeningBalance = true,
    int Page = 1,
    int PageSize = 50);

/// <summary>Posted ledger movement line for inquiry / account statement.</summary>
public record GeneralLedgerLineDto(
    DateOnly PostingDate,
    string EntryNumber,
    string Description,
    decimal Debit,
    decimal Credit,
    decimal RunningBalance,
    Guid? JournalEntryId = null,
    PostingSource? SourceModule = null,
    Guid? SourceDocumentId = null,
    string? Reference = null,
    Guid? CostCenterId = null,
    string? CostCenterNameAr = null,
    string? CostCenterNameEn = null,
    Guid? ChartOfAccountId = null,
    string? AccountNumber = null,
    string? AccountNameAr = null,
    bool IsOpeningBalance = false);

/// <summary>Paged general ledger inquiry with period summary.</summary>
public record GeneralLedgerResultDto(
    decimal OpeningBalance,
    decimal TotalDebit,
    decimal TotalCredit,
    decimal ClosingBalance,
    int TotalCount,
    int Page,
    int PageSize,
    IReadOnlyList<GeneralLedgerLineDto> Lines);

public record AccountStatementFilterDto(
    Guid AccountId, DateOnly FromDate, DateOnly ToDate);

public record JournalRegisterFilterDto(
    DateOnly? FromDate = null, DateOnly? ToDate = null,
    PostingSource? SourceModule = null, int Page = 1, int PageSize = 50);

public record BalanceVerificationDto(
    decimal TotalDebit, decimal TotalCredit, bool IsBalanced, int PostedJournalCount);
