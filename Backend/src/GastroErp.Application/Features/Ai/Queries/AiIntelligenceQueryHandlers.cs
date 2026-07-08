using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Ai.DTOs;
using GastroErp.Application.Features.Ai.Queries;
using GastroErp.Application.Features.Ai.Services;
using MediatR;

namespace GastroErp.Application.Features.Ai.Queries;

public sealed class GetFraudAlertsQueryHandler
    : IRequestHandler<GetFraudAlertsQuery, Result<IReadOnlyList<FraudAlertDto>>>
{
    private readonly IFraudDetectionService _service;
    public GetFraudAlertsQueryHandler(IFraudDetectionService service) => _service = service;
    public async Task<Result<IReadOnlyList<FraudAlertDto>>> Handle(GetFraudAlertsQuery request, CancellationToken ct)
        => Result<IReadOnlyList<FraudAlertDto>>.Success(await _service.GetAlertsAsync(request.TenantId, request.Filter, ct));
}

public sealed class GetCustomerSegmentsQueryHandler
    : IRequestHandler<GetCustomerSegmentsQuery, Result<IReadOnlyList<CustomerSegmentDto>>>
{
    private readonly ICustomerSegmentationService _service;
    public GetCustomerSegmentsQueryHandler(ICustomerSegmentationService service) => _service = service;
    public async Task<Result<IReadOnlyList<CustomerSegmentDto>>> Handle(GetCustomerSegmentsQuery request, CancellationToken ct)
        => Result<IReadOnlyList<CustomerSegmentDto>>.Success(await _service.GetSegmentsAsync(request.TenantId, request.Filter, ct));
}

public sealed class GetChurnPredictionsQueryHandler
    : IRequestHandler<GetChurnPredictionsQuery, Result<IReadOnlyList<ChurnPredictionDto>>>
{
    private readonly IChurnPredictionService _service;
    public GetChurnPredictionsQueryHandler(IChurnPredictionService service) => _service = service;
    public async Task<Result<IReadOnlyList<ChurnPredictionDto>>> Handle(GetChurnPredictionsQuery request, CancellationToken ct)
        => Result<IReadOnlyList<ChurnPredictionDto>>.Success(await _service.GetPredictionsAsync(request.TenantId, request.Filter, ct));
}

public sealed class GetProductRecommendationsQueryHandler
    : IRequestHandler<GetProductRecommendationsQuery, Result<IReadOnlyList<ProductRecommendationDto>>>
{
    private readonly IRecommendationEngineService _service;
    public GetProductRecommendationsQueryHandler(IRecommendationEngineService service) => _service = service;
    public async Task<Result<IReadOnlyList<ProductRecommendationDto>>> Handle(GetProductRecommendationsQuery request, CancellationToken ct)
        => Result<IReadOnlyList<ProductRecommendationDto>>.Success(await _service.GetRecommendationsAsync(request.TenantId, request.Filter, ct));
}

public sealed class GetIntelligenceDashboardQueryHandler
    : IRequestHandler<GetIntelligenceDashboardQuery, Result<IntelligenceDashboardDto>>
{
    private readonly IIntelligenceDashboardService _service;
    public GetIntelligenceDashboardQueryHandler(IIntelligenceDashboardService service) => _service = service;
    public async Task<Result<IntelligenceDashboardDto>> Handle(GetIntelligenceDashboardQuery request, CancellationToken ct)
        => Result<IntelligenceDashboardDto>.Success(await _service.GetDashboardAsync(request.TenantId, request.BranchId, ct));
}

public sealed class GetIntelligenceMonitoringQueryHandler
    : IRequestHandler<GetIntelligenceMonitoringQuery, Result<IntelligenceMonitoringDto>>
{
    private readonly IIntelligenceDashboardService _service;
    public GetIntelligenceMonitoringQueryHandler(IIntelligenceDashboardService service) => _service = service;
    public async Task<Result<IntelligenceMonitoringDto>> Handle(GetIntelligenceMonitoringQuery request, CancellationToken ct)
        => Result<IntelligenceMonitoringDto>.Success(await _service.GetMonitoringAsync(ct));
}
