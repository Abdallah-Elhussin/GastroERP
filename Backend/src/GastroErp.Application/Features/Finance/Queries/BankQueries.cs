using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Finance.Queries;

public record GetBanksQuery(Guid TenantId, BankFilterDto Filter) : IRequest<PagedResult<BankDto>>;
public record GetBankByIdQuery(Guid Id) : IRequest<Result<BankDto>>;
