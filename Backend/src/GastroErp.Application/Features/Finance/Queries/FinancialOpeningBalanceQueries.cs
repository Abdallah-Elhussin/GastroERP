using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Finance.Queries;

public record GetFinancialOpeningBalancesQuery(Guid TenantId, FinancialOpeningBalanceFilterDto Filter)
    : IRequest<PagedResult<FinancialOpeningBalanceDto>>;

public record GetFinancialOpeningBalanceByIdQuery(Guid Id)
    : IRequest<Result<FinancialOpeningBalanceDto>>;
