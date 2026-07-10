using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Sales.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Sales.Queries;

public record GetKdsBoardQuery(Guid TenantId, Guid? BranchId, Guid? StationId)
    : IRequest<Result<IReadOnlyList<KdsTicketViewDto>>>;
