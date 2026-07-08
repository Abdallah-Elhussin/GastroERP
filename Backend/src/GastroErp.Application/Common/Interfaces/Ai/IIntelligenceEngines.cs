using GastroErp.Domain.Enums;

namespace GastroErp.Application.Common.Interfaces.Ai;

public record FraudSignal(
    FraudType AlertType,
    decimal RiskScore,
    FraudSeverity Severity,
    string Source,
    string DetailsJson,
    string? ReferenceType = null,
    Guid? ReferenceId = null,
    Guid? BranchId = null);

public record SegmentAssignment(
    Guid CustomerId,
    CustomerSegmentType Segment,
    decimal Score,
    string MetricsJson);

public record ChurnScore(
    Guid CustomerId,
    decimal ChurnProbability,
    ChurnRiskLevel RiskLevel,
    string Recommendation,
    string MetricsJson);

public record ProductRecommendationSignal(
    Guid ProductId,
    ProductRecommendationType RecommendationType,
    decimal Confidence,
    IReadOnlyList<(Guid ProductId, string Name, decimal Confidence)> RelatedProducts,
    Guid? TargetCustomerId = null,
    Guid? BranchId = null);

public interface IFraudAnalysisEngine
{
    Task<IReadOnlyList<FraudSignal>> AnalyzeAsync(
        Guid tenantId, int lookbackDays, Guid? branchId = null, CancellationToken ct = default);
}

public interface ICustomerSegmentationEngine
{
    Task<IReadOnlyList<SegmentAssignment>> SegmentAsync(
        Guid tenantId, int lookbackDays, Guid? branchId = null, CancellationToken ct = default);
}

public interface IChurnPredictionEngine
{
    Task<IReadOnlyList<ChurnScore>> PredictAsync(
        Guid tenantId, int lookbackDays, Guid? branchId = null, CancellationToken ct = default);
}

public interface IProductRecommendationEngine
{
    Task<IReadOnlyList<ProductRecommendationSignal>> GenerateAsync(
        Guid tenantId, int lookbackDays, Guid? branchId = null, CancellationToken ct = default);
}
