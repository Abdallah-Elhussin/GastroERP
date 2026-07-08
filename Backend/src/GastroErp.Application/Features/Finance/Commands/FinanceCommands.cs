using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Finance.Commands;

// Chart of Accounts
public record CreateAccountCommand(Guid TenantId, CreateAccountDto Dto) : IRequest<Result<AccountDto>>;
public record UpdateAccountCommand(Guid Id, UpdateAccountDto Dto) : IRequest<Result>;
public record ActivateAccountCommand(Guid Id) : IRequest<Result>;
public record DeactivateAccountCommand(Guid Id) : IRequest<Result>;
public record DeleteAccountCommand(Guid Id) : IRequest<Result>;

// Journal
public record CreateJournalCommand(Guid TenantId, Guid UserId, CreateJournalDto Dto) : IRequest<Result<JournalDto>>;
public record PostJournalCommand(Guid Id, Guid UserId) : IRequest<Result<JournalDto>>;
public record ReverseJournalCommand(Guid Id, Guid UserId) : IRequest<Result<JournalDto>>;

// Fiscal Period
public record CreateFiscalPeriodCommand(Guid TenantId, CreateFiscalPeriodDto Dto) : IRequest<Result<FiscalPeriodDto>>;
public record CloseFiscalPeriodCommand(Guid Id) : IRequest<Result>;
public record LockFiscalPeriodCommand(Guid Id) : IRequest<Result>;
public record ReopenFiscalPeriodCommand(Guid Id) : IRequest<Result>;

// Cost Center
public record CreateCostCenterCommand(Guid TenantId, CreateCostCenterDto Dto) : IRequest<Result<CostCenterDto>>;
public record UpdateCostCenterCommand(Guid Id, UpdateCostCenterDto Dto) : IRequest<Result>;
public record ActivateCostCenterCommand(Guid Id) : IRequest<Result>;
public record DeactivateCostCenterCommand(Guid Id) : IRequest<Result>;
