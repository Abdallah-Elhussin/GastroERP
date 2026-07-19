using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.DTOs;
using GastroErp.Domain.Enums;
using MediatR;

namespace GastroErp.Application.Features.Finance.Queries;

public record GetAccountsQuery(Guid TenantId, AccountFilterDto Filter) : IRequest<PagedResult<AccountDto>>;
public record GetAccountByIdQuery(Guid Id) : IRequest<Result<AccountDto>>;
public record GetAccountTreeQuery(
    Guid TenantId,
    AccountType? AccountType = null,
    bool IncludeInactive = true,
    string? Search = null) : IRequest<Result<IReadOnlyList<AccountTreeNodeDto>>>;
public record ExportAccountsQuery(Guid TenantId) : IRequest<Result<IReadOnlyList<AccountExportRowDto>>>;
public record GetAccountingSettingsQuery(Guid TenantId) : IRequest<Result<AccountingSettingsDto>>;

public record GetJournalsQuery(Guid TenantId, JournalFilterDto Filter) : IRequest<PagedResult<JournalDto>>;
public record GetJournalByIdQuery(Guid Id) : IRequest<Result<JournalDetailDto>>;

public record GetFiscalPeriodsQuery(
    Guid TenantId,
    string? SearchTerm = null,
    FiscalPeriodStatus? Status = null) : IRequest<Result<IReadOnlyList<FiscalPeriodDto>>>;
public record GetFiscalPeriodByIdQuery(Guid Id) : IRequest<Result<FiscalPeriodDto>>;

public record GetCostCentersQuery(Guid TenantId, CostCenterFilterDto Filter) : IRequest<PagedResult<CostCenterDto>>;
public record GetCostCenterByIdQuery(Guid Id) : IRequest<Result<CostCenterDto>>;

public record GetCurrenciesQuery(Guid TenantId, CurrencyFilterDto Filter) : IRequest<PagedResult<CurrencyDto>>;
public record GetCurrencyByIdQuery(Guid Id) : IRequest<Result<CurrencyDto>>;
public record GetCurrencyExchangeRatesQuery(Guid TenantId, CurrencyExchangeRateFilterDto Filter) : IRequest<PagedResult<CurrencyExchangeRateDto>>;

public record GetTrialBalanceQuery(Guid TenantId, TrialBalanceFilterDto Filter) : IRequest<Result<IReadOnlyList<TrialBalanceLineDto>>>;
public record GetGeneralLedgerQuery(Guid TenantId, GeneralLedgerFilterDto Filter) : IRequest<Result<GeneralLedgerResultDto>>;
public record GetAccountStatementQuery(AccountStatementFilterDto Filter) : IRequest<Result<IReadOnlyList<GeneralLedgerLineDto>>>;
public record GetJournalRegisterQuery(Guid TenantId, JournalRegisterFilterDto Filter) : IRequest<PagedResult<JournalDto>>;
public record GetBalanceVerificationQuery(Guid TenantId, TrialBalanceFilterDto Filter) : IRequest<Result<BalanceVerificationDto>>;
