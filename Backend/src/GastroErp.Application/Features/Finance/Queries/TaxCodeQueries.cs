using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Finance.Queries;

public record GetTaxCodesQuery(Guid TenantId, TaxCodeFilterDto Filter) : IRequest<PagedResult<TaxCodeDto>>;
public record GetTaxCodeByIdQuery(Guid Id) : IRequest<Result<TaxCodeDto>>;
