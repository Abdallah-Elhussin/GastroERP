using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Finance.DTOs;

public record FinancialOpeningBalanceLineDto(
    Guid? Id,
    Guid ChartOfAccountId,
    string? AccountNumber,
    string? AccountNameAr,
    Guid? CostCenterId,
    string? CostCenterNameAr,
    decimal Debit,
    decimal Credit,
    string Currency,
    string? Description);

public record UpsertFinancialOpeningBalanceDto(
    Guid CompanyId,
    DateOnly OpeningDate,
    Guid FiscalPeriodId,
    Guid? BranchId = null,
    string? Description = null,
    Guid? EquityAccountId = null,
    IReadOnlyList<FinancialOpeningBalanceLineDto>? Lines = null);

public record FinancialOpeningBalanceDto(
    Guid Id,
    int Number,
    string DocumentNumber,
    Guid CompanyId,
    string? CompanyNameAr,
    Guid? BranchId,
    string? BranchNameAr,
    DateOnly OpeningDate,
    Guid FiscalPeriodId,
    int? FiscalYear,
    string? Description,
    FinancialOpeningBalanceStatus Status,
    Guid? EquityAccountId,
    Guid? JournalEntryId,
    string? JournalEntryNumber,
    int LinesCount,
    decimal TotalDebit,
    decimal TotalCredit,
    DateTimeOffset CreatedAt,
    DateTimeOffset? PostedAt,
    IReadOnlyList<FinancialOpeningBalanceLineDto> Lines);

public record FinancialOpeningBalanceFilterDto(
    Guid? CompanyId = null,
    Guid? BranchId = null,
    Guid? FiscalPeriodId = null,
    FinancialOpeningBalanceStatus? Status = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 200);
