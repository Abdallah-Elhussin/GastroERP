using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Ai.DTOs;

public record IntelligenceFilterDto(
    Guid? BranchId = null,
    FraudSeverity? MinSeverity = null,
    CustomerSegmentType? Segment = null,
    ChurnRiskLevel? MinChurnRisk = null,
    ProductRecommendationType? RecommendationType = null,
    int Page = 1,
    int PageSize = 50);

public record RefreshIntelligenceDto(
    Guid? BranchId = null,
    int LookbackDays = 30,
    bool NotifyOnCritical = true);

public record RefreshIntelligenceResultDto(string Module, int ItemsProcessed, DateTimeOffset CompletedAt);

public record FraudAlertDto(
    Guid Id, FraudType AlertType, decimal RiskScore, FraudSeverity Severity,
    string Source, FraudAlertStatus Status, string? ReferenceType, Guid? ReferenceId,
    Guid? BranchId, string DetailsJson, DateTimeOffset CreatedAt);

public record FraudAnalysisResultDto(
    IReadOnlyList<FraudAlertDto> Alerts, int TotalAlerts, int CriticalCount, DateTimeOffset AnalyzedAt);

public record CustomerSegmentDto(
    Guid Id, Guid CustomerId, string CustomerName, CustomerSegmentType Segment,
    decimal Score, string MetricsJson, DateTimeOffset UpdatedAt);

public record SegmentationResultDto(
    IReadOnlyList<CustomerSegmentDto> Segments, IReadOnlyDictionary<CustomerSegmentType, int> Distribution,
    DateTimeOffset RefreshedAt);

public record ChurnPredictionDto(
    Guid Id, Guid CustomerId, string CustomerName, decimal ChurnProbability,
    ChurnRiskLevel RiskLevel, string Recommendation, string MetricsJson, DateTimeOffset GeneratedAt);

public record ChurnAnalysisResultDto(
    IReadOnlyList<ChurnPredictionDto> Predictions, int HighRiskCount, DateTimeOffset GeneratedAt);

public record RelatedProductDto(Guid ProductId, string ProductName, decimal Confidence);

public record ProductRecommendationDto(
    Guid Id, Guid ProductId, string ProductName, ProductRecommendationType RecommendationType,
    decimal Confidence, IReadOnlyList<RelatedProductDto> RelatedProducts,
    Guid? TargetCustomerId, Guid? BranchId, DateTimeOffset CreatedAt);

public record ProductRecommendationResultDto(
    IReadOnlyList<ProductRecommendationDto> Recommendations, DateTimeOffset GeneratedAt);

public record IntelligenceDashboardDto(
    int OpenFraudAlerts,
    int CriticalFraudAlerts,
    int HighRiskCustomers,
    int CustomersAtRisk,
    int VipCustomers,
    int ActiveRecommendations,
    decimal AverageChurnProbability,
    DateTimeOffset GeneratedAt);

public record IntelligenceMonitoringDto(
    IReadOnlyList<string> RegisteredJobs,
    DateTimeOffset? LastFraudRun,
    DateTimeOffset? LastSegmentationRun,
    DateTimeOffset? LastChurnRun,
    DateTimeOffset? LastRecommendationRun,
    bool EnginesHealthy);
