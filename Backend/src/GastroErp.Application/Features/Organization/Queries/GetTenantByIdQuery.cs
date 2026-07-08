using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Organization.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Organization.Queries;

public record GetTenantByIdQuery(Guid Id) : IRequest<Result<TenantDto>>;
