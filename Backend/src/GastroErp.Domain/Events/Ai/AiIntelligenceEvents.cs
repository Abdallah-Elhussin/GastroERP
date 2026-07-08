using GastroErp.Domain.Common;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Events.Ai;

public sealed record FraudDetectedEvent(
    Guid AlertId, Guid TenantId, FraudType AlertType, FraudSeverity Severity, decimal RiskScore)
    : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record CustomerSegmentUpdatedEvent(
    Guid SegmentId, Guid TenantId, Guid CustomerId, CustomerSegmentType Segment)
    : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record ChurnPredictedEvent(
    Guid PredictionId, Guid TenantId, Guid CustomerId, decimal ChurnProbability, ChurnRiskLevel RiskLevel)
    : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record RecommendationGeneratedEvent(
    Guid RecommendationId, Guid TenantId, Guid ProductId, ProductRecommendationType RecommendationType)
    : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
