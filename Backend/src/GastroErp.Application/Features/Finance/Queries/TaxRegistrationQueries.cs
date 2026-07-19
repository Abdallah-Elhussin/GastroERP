using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Finance.Queries;

public record GetTaxRegistrationsQuery(Guid TenantId, TaxRegistrationFilterDto Filter)
    : IRequest<PagedResult<TaxRegistrationProfileDto>>;
public record GetTaxRegistrationByIdQuery(Guid Id) : IRequest<Result<TaxRegistrationProfileDto>>;
