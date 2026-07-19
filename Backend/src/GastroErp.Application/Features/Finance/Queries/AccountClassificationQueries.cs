using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Finance.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Finance.Queries;

public record GetAccountClassificationsQuery(Guid TenantId, AccountClassificationFilterDto Filter)
    : IRequest<Result<IReadOnlyList<AccountClassificationDto>>>;

public record GetAccountClassificationByIdQuery(Guid Id) : IRequest<Result<AccountClassificationDto>>;

public record GetAccountMainClassificationsQuery(Guid TenantId)
    : IRequest<Result<IReadOnlyList<AccountMainClassificationDto>>>;
