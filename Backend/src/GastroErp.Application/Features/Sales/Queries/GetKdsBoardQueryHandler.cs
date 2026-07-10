using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Sales.DTOs;
using GastroErp.Application.Features.Sales.Services;
using MediatR;

namespace GastroErp.Application.Features.Sales.Queries;

public sealed class GetKdsBoardQueryHandler(IKdsBoardProjectionService boardProjection)
    : IRequestHandler<GetKdsBoardQuery, Result<IReadOnlyList<KdsTicketViewDto>>>
{
    public async Task<Result<IReadOnlyList<KdsTicketViewDto>>> Handle(
        GetKdsBoardQuery request,
        CancellationToken cancellationToken)
    {
        var board = await boardProjection.GetActiveBoardAsync(
            request.TenantId,
            request.BranchId,
            request.StationId,
            cancellationToken);

        return Result<IReadOnlyList<KdsTicketViewDto>>.Success(board);
    }
}
