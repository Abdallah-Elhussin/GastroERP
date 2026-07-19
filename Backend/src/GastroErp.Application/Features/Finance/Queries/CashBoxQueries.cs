using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Finance.Queries;

public record GetCashBoxesQuery(Guid TenantId, CashBoxFilterDto Filter) : IRequest<PagedResult<CashBoxDto>>;
public record GetCashBoxByIdQuery(Guid Id) : IRequest<Result<CashBoxDto>>;
