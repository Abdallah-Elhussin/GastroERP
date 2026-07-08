using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Ai.Commands;
using GastroErp.Application.Features.Ai.DTOs;
using GastroErp.Application.Features.Ai.Services;
using MediatR;

namespace GastroErp.Application.Features.Ai.Commands;

public sealed class RefreshFraudAnalysisCommandHandler
    : IRequestHandler<RefreshFraudAnalysisCommand, Result<FraudAnalysisResultDto>>
{
    private readonly IFraudDetectionService _service;
    public RefreshFraudAnalysisCommandHandler(IFraudDetectionService service) => _service = service;
    public async Task<Result<FraudAnalysisResultDto>> Handle(RefreshFraudAnalysisCommand request, CancellationToken ct)
        => Result<FraudAnalysisResultDto>.Success(await _service.AnalyzeAsync(request.TenantId, request.Dto, ct));
}

public sealed class RefreshSegmentsCommandHandler
    : IRequestHandler<RefreshSegmentsCommand, Result<SegmentationResultDto>>
{
    private readonly ICustomerSegmentationService _service;
    public RefreshSegmentsCommandHandler(ICustomerSegmentationService service) => _service = service;
    public async Task<Result<SegmentationResultDto>> Handle(RefreshSegmentsCommand request, CancellationToken ct)
        => Result<SegmentationResultDto>.Success(await _service.RefreshAsync(request.TenantId, request.Dto, ct));
}

public sealed class RefreshChurnCommandHandler
    : IRequestHandler<RefreshChurnCommand, Result<ChurnAnalysisResultDto>>
{
    private readonly IChurnPredictionService _service;
    public RefreshChurnCommandHandler(IChurnPredictionService service) => _service = service;
    public async Task<Result<ChurnAnalysisResultDto>> Handle(RefreshChurnCommand request, CancellationToken ct)
        => Result<ChurnAnalysisResultDto>.Success(await _service.RefreshAsync(request.TenantId, request.Dto, ct));
}

public sealed class RefreshIntelligenceRecommendationsCommandHandler
    : IRequestHandler<RefreshIntelligenceRecommendationsCommand, Result<ProductRecommendationResultDto>>
{
    private readonly IRecommendationEngineService _service;
    public RefreshIntelligenceRecommendationsCommandHandler(IRecommendationEngineService service) => _service = service;
    public async Task<Result<ProductRecommendationResultDto>> Handle(
        RefreshIntelligenceRecommendationsCommand request, CancellationToken ct)
        => Result<ProductRecommendationResultDto>.Success(await _service.RefreshAsync(request.TenantId, request.Dto, ct));
}
