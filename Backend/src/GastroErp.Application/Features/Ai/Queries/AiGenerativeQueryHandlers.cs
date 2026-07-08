using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Ai.DTOs;
using GastroErp.Application.Features.Ai.Services;
using MediatR;

namespace GastroErp.Application.Features.Ai.Queries;

public sealed class GetDashboardInsightsQueryHandler : IRequestHandler<GetDashboardInsightsQuery, Result<AiDashboardInsightDto>>
{
    private readonly IAiDashboardInsightsService _service;
    public GetDashboardInsightsQueryHandler(IAiDashboardInsightsService service) => _service = service;
    public async Task<Result<AiDashboardInsightDto>> Handle(GetDashboardInsightsQuery request, CancellationToken ct)
        => Result<AiDashboardInsightDto>.Success(await _service.GetDashboardInsightsAsync(request.TenantId, request.Filter, ct));
}
