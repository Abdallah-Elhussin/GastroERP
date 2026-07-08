using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Ai.Commands;
using GastroErp.Application.Features.Ai.DTOs;
using GastroErp.Application.Features.Ai.Services;
using MediatR;

namespace GastroErp.Application.Features.Ai.Commands;

public sealed class RefreshForecastsCommandHandler : IRequestHandler<RefreshForecastsCommand, Result<RefreshForecastsResultDto>>
{
    private readonly IAiForecastOrchestrator _orchestrator;

    public RefreshForecastsCommandHandler(IAiForecastOrchestrator orchestrator) => _orchestrator = orchestrator;

    public async Task<Result<RefreshForecastsResultDto>> Handle(RefreshForecastsCommand request, CancellationToken cancellationToken)
        => Result<RefreshForecastsResultDto>.Success(
            await _orchestrator.RefreshAllAsync(request.TenantId, request.Dto, cancellationToken));
}
