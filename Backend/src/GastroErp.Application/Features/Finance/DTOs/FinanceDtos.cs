using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Finance.DTOs;

// ─── Chart of Accounts ────────────────────────────────────────────────────────

public record CreateAccountDto(
    string AccountNumber, string NameAr, AccountType AccountType, AccountCategory AccountCategory,
    bool IsSummaryAccount = false, Guid? ParentAccountId = null, string? NameEn = null,
    string Currency = "SAR", int SortOrder = 0);

public record UpdateAccountDto(
    string NameAr, string? NameEn, AccountCategory AccountCategory,
    bool IsSummaryAccount, int SortOrder);

public record AccountDto(
    Guid Id, string AccountNumber, string NameAr, string? NameEn,
    Guid? ParentAccountId, AccountType AccountType, AccountCategory AccountCategory,
    string Currency, bool IsPostingAllowed, bool IsSummaryAccount, bool IsActive, int SortOrder);

public record AccountTreeNodeDto(
    Guid Id, string AccountNumber, string NameAr, string? NameEn,
    AccountType AccountType, AccountCategory AccountCategory,
    bool IsActive, int SortOrder, IReadOnlyList<AccountTreeNodeDto> Children);

public record AccountFilterDto(
    AccountType? AccountType = null, bool? IsActive = null,
    string? Search = null, int Page = 1, int PageSize = 50);

// ─── Journal ────────────────────────────────────────────────────────────────────

public record JournalLineDto(
    Guid? Id, Guid ChartOfAccountId, Guid? CostCenterId,
    decimal Debit, decimal Credit, string? Description);

public record CreateJournalDto(
    DateOnly PostingDate, string Description, PostingSource SourceModule,
    Guid? BranchId = null, Guid? CompanyId = null, string? Reference = null,
    Guid? SourceDocumentId = null, IReadOnlyList<JournalLineDto>? Lines = null);

public record JournalDto(
    Guid Id, string EntryNumber, DateOnly PostingDate, Guid FiscalPeriodId,
    string Description, string? Reference, PostingSource SourceModule,
    Guid? SourceDocumentId, JournalStatus Status, DateTimeOffset? PostedAt,
    decimal TotalDebit, decimal TotalCredit);

public record JournalDetailDto(
    Guid Id, string EntryNumber, DateOnly PostingDate, Guid FiscalPeriodId,
    string Description, string? Reference, PostingSource SourceModule,
    Guid? SourceDocumentId, JournalStatus Status, DateTimeOffset? PostedAt,
    IReadOnlyList<JournalLineDetailDto> Lines);

public record JournalLineDetailDto(
    Guid Id, int LineNumber, Guid ChartOfAccountId, string AccountNumber, string AccountName,
    Guid? CostCenterId, decimal Debit, decimal Credit, string Currency, string? Description);

public record JournalFilterDto(
    JournalStatus? Status = null, PostingSource? SourceModule = null,
    DateOnly? FromDate = null, DateOnly? ToDate = null,
    int Page = 1, int PageSize = 50);

// ─── Fiscal Period ──────────────────────────────────────────────────────────────

public record CreateFiscalPeriodDto(
    int FiscalYear, string Name, DateOnly StartDate, DateOnly EndDate);

public record FiscalPeriodDto(
    Guid Id, int FiscalYear, string Name, DateOnly StartDate, DateOnly EndDate, FiscalPeriodStatus Status);

// ─── Cost Center ────────────────────────────────────────────────────────────────

public record CreateCostCenterDto(
    Guid BranchId, string Code, string NameAr, Guid? DepartmentId = null, string? NameEn = null);

public record UpdateCostCenterDto(string NameAr, string? NameEn, Guid? DepartmentId);

public record CostCenterDto(
    Guid Id, Guid BranchId, Guid? DepartmentId, string Code,
    string NameAr, string? NameEn, CostCenterStatus Status);

public record CostCenterFilterDto(
    Guid? BranchId = null, CostCenterStatus? Status = null,
    string? Search = null, int Page = 1, int PageSize = 50);

// ─── Reports ────────────────────────────────────────────────────────────────────

public record TrialBalanceFilterDto(
    DateOnly? AsOfDate = null, Guid? FiscalPeriodId = null);

public record TrialBalanceLineDto(
    Guid AccountId, string AccountNumber, string AccountName,
    AccountType AccountType, decimal DebitBalance, decimal CreditBalance);

public record GeneralLedgerFilterDto(
    Guid AccountId, DateOnly? FromDate = null, DateOnly? ToDate = null,
    int Page = 1, int PageSize = 100);

public record GeneralLedgerLineDto(
    DateOnly PostingDate, string EntryNumber, string Description,
    decimal Debit, decimal Credit, decimal RunningBalance);

public record AccountStatementFilterDto(
    Guid AccountId, DateOnly FromDate, DateOnly ToDate);

public record JournalRegisterFilterDto(
    DateOnly? FromDate = null, DateOnly? ToDate = null,
    PostingSource? SourceModule = null, int Page = 1, int PageSize = 50);

public record BalanceVerificationDto(
    decimal TotalDebit, decimal TotalCredit, bool IsBalanced, int PostedJournalCount);
