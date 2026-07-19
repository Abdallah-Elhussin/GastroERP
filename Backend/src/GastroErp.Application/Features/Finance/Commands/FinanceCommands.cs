using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Finance.Commands;

// Chart of Accounts
public record CreateAccountCommand(Guid TenantId, CreateAccountDto Dto) : IRequest<Result<AccountDto>>;
public record UpdateAccountCommand(Guid Id, UpdateAccountDto Dto) : IRequest<Result>;
public record ReparentAccountCommand(Guid Id, ReparentAccountDto Dto) : IRequest<Result>;
public record RenumberAccountCommand(Guid Id, RenumberAccountDto Dto) : IRequest<Result>;
public record ActivateAccountCommand(Guid Id) : IRequest<Result>;
public record DeactivateAccountCommand(Guid Id) : IRequest<Result>;
public record DeleteAccountCommand(Guid Id) : IRequest<Result>;
public record ImportAccountsCommand(Guid TenantId, IReadOnlyList<AccountImportRowDto> Rows, bool Commit) : IRequest<Result<AccountImportPreviewDto>>;

// Accounting Settings
public record UpsertAccountingSettingsCommand(Guid TenantId, UpdateAccountingSettingsDto Dto) : IRequest<Result<AccountingSettingsDto>>;

// Journal
public record CreateJournalCommand(Guid TenantId, Guid UserId, CreateJournalDto Dto) : IRequest<Result<JournalDto>>;
public record UpdateJournalCommand(Guid Id, UpdateJournalDto Dto) : IRequest<Result<JournalDto>>;
public record ApproveJournalCommand(Guid Id) : IRequest<Result<JournalDto>>;
public record PostJournalCommand(Guid Id, Guid UserId) : IRequest<Result<JournalDto>>;
public record ReverseJournalCommand(Guid Id, Guid UserId) : IRequest<Result<JournalDto>>;
public record DeleteJournalCommand(Guid Id) : IRequest<Result>;

// Fiscal Period
public record CreateFiscalPeriodCommand(Guid TenantId, CreateFiscalPeriodDto Dto) : IRequest<Result<FiscalPeriodDto>>;
public record UpdateFiscalPeriodCommand(Guid Id, UpdateFiscalPeriodDto Dto) : IRequest<Result<FiscalPeriodDto>>;
public record DeleteFiscalPeriodCommand(Guid Id) : IRequest<Result>;
public record GenerateFiscalPeriodDetailsCommand(Guid Id) : IRequest<Result<FiscalPeriodDto>>;
public record CloseFiscalPeriodCommand(Guid Id) : IRequest<Result>;
public record LockFiscalPeriodCommand(Guid Id) : IRequest<Result>;
public record ReopenFiscalPeriodCommand(Guid Id) : IRequest<Result>;

// Cost Center
public record CreateCostCenterCommand(Guid TenantId, Guid? CurrentBranchId, CreateCostCenterDto Dto) : IRequest<Result<CostCenterDto>>;
public record UpdateCostCenterCommand(Guid Id, UpdateCostCenterDto Dto) : IRequest<Result<CostCenterDto>>;
public record ActivateCostCenterCommand(Guid Id) : IRequest<Result>;
public record DeactivateCostCenterCommand(Guid Id) : IRequest<Result>;
public record DeleteCostCenterCommand(Guid Id) : IRequest<Result>;

// Currency
public record CreateCurrencyCommand(Guid TenantId, string? UserName, CreateCurrencyDto Dto) : IRequest<Result<CurrencyDto>>;
public record UpdateCurrencyCommand(Guid Id, string? UserName, UpdateCurrencyDto Dto) : IRequest<Result<CurrencyDto>>;
public record ActivateCurrencyCommand(Guid Id) : IRequest<Result>;
public record DeactivateCurrencyCommand(Guid Id) : IRequest<Result>;
public record DeleteCurrencyCommand(Guid Id) : IRequest<Result>;
public record SetCompanyCurrencyCommand(Guid TenantId, Guid Id, string? UserName) : IRequest<Result<CurrencyDto>>;
public record CreateCurrencyExchangeRateCommand(Guid TenantId, string? UserName, CreateCurrencyExchangeRateDto Dto) : IRequest<Result<CurrencyExchangeRateDto>>;
public record UpdateCurrencyExchangeRateCommand(Guid Id, string? UserName, UpdateCurrencyExchangeRateDto Dto) : IRequest<Result<CurrencyExchangeRateDto>>;
public record ActivateCurrencyExchangeRateCommand(Guid Id) : IRequest<Result>;
public record DeactivateCurrencyExchangeRateCommand(Guid Id) : IRequest<Result>;
public record DeleteCurrencyExchangeRateCommand(Guid Id) : IRequest<Result>;
