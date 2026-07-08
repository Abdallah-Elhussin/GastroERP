using GastroErp.Application.Common.Interfaces.Ai;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GastroErp.Infrastructure.Health;

public sealed class AiIntelligenceHealthCheck : IHealthCheck
{
    private readonly IFraudAnalysisEngine _fraud;
    private readonly ICustomerSegmentationEngine _segmentation;
    private readonly IChurnPredictionEngine _churn;
    private readonly IProductRecommendationEngine _recommendations;

    public AiIntelligenceHealthCheck(
        IFraudAnalysisEngine fraud, ICustomerSegmentationEngine segmentation,
        IChurnPredictionEngine churn, IProductRecommendationEngine recommendations)
        => (_fraud, _segmentation, _churn, _recommendations) = (fraud, segmentation, churn, recommendations);

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var enginesReady = _fraud is not null && _segmentation is not null && _churn is not null && _recommendations is not null;
        return Task.FromResult(enginesReady
            ? HealthCheckResult.Healthy("AI intelligence engines registered")
            : HealthCheckResult.Unhealthy("AI intelligence engines not available"));
    }
}
