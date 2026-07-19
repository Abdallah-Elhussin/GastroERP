using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.DTOs;
using GastroErp.Domain.Entities.Finance;
using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Finance.Services;

public interface IJournalNumberGenerator
{
    Task<string> GenerateAsync(Guid tenantId, CancellationToken ct = default);
}

public interface IFiscalPeriodService
{
    Task<FiscalPeriod> GetOrEnsureOpenPeriodAsync(Guid tenantId, DateOnly postingDate, CancellationToken ct = default);
    Task<Result> ValidatePeriodAcceptsPostingsAsync(Guid fiscalPeriodId, CancellationToken ct = default);
}

public interface IFinancialValidationService
{
    Task<Result> ValidateJournalLinesAsync(
        Guid tenantId, IReadOnlyList<JournalLineDto> lines, CancellationToken ct = default,
        bool requireBalance = true);
    Task<Result> ValidateAccountCanPostAsync(Guid accountId, CancellationToken ct = default);
}

public interface IJournalPostingService
{
    Task<Result<JournalEntry>> CreateDraftAsync(
        Guid tenantId, CreateJournalDto dto, CancellationToken ct = default);
    Task<Result<JournalEntry>> CreateAndPostAsync(
        Guid tenantId, Guid userId, CreateJournalDto dto, CancellationToken ct = default);
    Task<Result<JournalEntry>> PostExistingAsync(Guid journalId, Guid userId, CancellationToken ct = default);
    Task<Result<JournalEntry>> ReverseAsync(Guid journalId, Guid userId, CancellationToken ct = default);
}

public interface IAutoPostingService
{
    Task<Result> PostSalesOrderCompletedAsync(Guid salesOrderId, Guid userId, CancellationToken ct = default);
    Task<Result> PostPaymentCompletedAsync(Guid paymentId, Guid userId, CancellationToken ct = default);
    Task<Result> PostPayrollRunAsync(Guid payrollRunId, Guid userId, CancellationToken ct = default);
}

public interface IAccountBalanceService
{
    Task<decimal> GetAccountBalanceAsync(Guid accountId, DateOnly? asOfDate = null, CancellationToken ct = default);
    Task<GeneralLedgerResultDto> GetGeneralLedgerAsync(Guid tenantId, GeneralLedgerFilterDto filter, CancellationToken ct = default);
}

public interface ITrialBalanceService
{
    Task<IReadOnlyList<TrialBalanceLineDto>> GetTrialBalanceAsync(Guid tenantId, TrialBalanceFilterDto filter, CancellationToken ct = default);
    Task<BalanceVerificationDto> VerifyBalanceAsync(Guid tenantId, TrialBalanceFilterDto filter, CancellationToken ct = default);
}

public static class StandardAccountCodes
{
    public const string Cash = "1100";
    public const string AccountsReceivable = "1200";
    public const string VatPayable = "2100";
    public const string SalesRevenue = "4000";
    public const string SalaryExpense = "5100";
    public const string SalariesPayable = "2200";
}
