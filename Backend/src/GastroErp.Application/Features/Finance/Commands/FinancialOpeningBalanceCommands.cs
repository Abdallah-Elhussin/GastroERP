using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Finance.Commands;

public record CreateFinancialOpeningBalanceCommand(Guid TenantId, UpsertFinancialOpeningBalanceDto Dto)
    : IRequest<Result<FinancialOpeningBalanceDto>>;

public record UpdateFinancialOpeningBalanceCommand(Guid Id, UpsertFinancialOpeningBalanceDto Dto)
    : IRequest<Result<FinancialOpeningBalanceDto>>;

public record PostFinancialOpeningBalanceCommand(Guid Id, Guid UserId)
    : IRequest<Result<FinancialOpeningBalanceDto>>;

public record ReverseFinancialOpeningBalanceCommand(Guid Id, Guid UserId)
    : IRequest<Result<FinancialOpeningBalanceDto>>;

public record DeleteFinancialOpeningBalanceCommand(Guid Id) : IRequest<Result>;
