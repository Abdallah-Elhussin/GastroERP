using GastroErp.Domain.Common;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Ai;

namespace GastroErp.Domain.Entities.Ai;

/// <summary>Advisory fraud alert — read-only intelligence output</summary>
public sealed class FraudAlert : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public FraudType AlertType { get; private set; }
    public decimal RiskScore { get; private set; }
    public FraudSeverity Severity { get; private set; }
    public string Source { get; private set; }
    public FraudAlertStatus Status { get; private set; }
    public string? ReferenceType { get; private set; }
    public Guid? ReferenceId { get; private set; }
    public Guid? BranchId { get; private set; }
    public string DetailsJson { get; private set; }

    private FraudAlert()
    {
        Source = string.Empty;
        DetailsJson = "{}";
    }

    public static FraudAlert Create(
        Guid tenantId, FraudType alertType, decimal riskScore, FraudSeverity severity,
        string source, string detailsJson, string? referenceType = null, Guid? referenceId = null, Guid? branchId = null)
    {
        var alert = new FraudAlert
        {
            TenantId = tenantId,
            AlertType = alertType,
            RiskScore = Math.Clamp(riskScore, 0, 100),
            Severity = severity,
            Source = source,
            Status = FraudAlertStatus.Open,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            BranchId = branchId,
            DetailsJson = detailsJson
        };
        alert.RaiseDomainEvent(new FraudDetectedEvent(alert.Id, tenantId, alertType, severity, alert.RiskScore));
        return alert;
    }

    public void Acknowledge() => Status = FraudAlertStatus.Acknowledged;
    public void Resolve() => Status = FraudAlertStatus.Resolved;
    public void Dismiss() => Status = FraudAlertStatus.Dismissed;
}

/// <summary>Customer segment assignment (advisory)</summary>
public sealed class CustomerSegment : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid CustomerId { get; private set; }
    public CustomerSegmentType Segment { get; private set; }
    public decimal Score { get; private set; }
    public string MetricsJson { get; private set; }

    private CustomerSegment() => MetricsJson = "{}";

    public static CustomerSegment Create(
        Guid tenantId, Guid customerId, CustomerSegmentType segment, decimal score, string metricsJson)
    {
        var entity = new CustomerSegment
        {
            TenantId = tenantId,
            CustomerId = customerId,
            Segment = segment,
            Score = score,
            MetricsJson = metricsJson
        };
        entity.RaiseDomainEvent(new CustomerSegmentUpdatedEvent(entity.Id, tenantId, customerId, segment));
        return entity;
    }

    public void Update(CustomerSegmentType segment, decimal score, string metricsJson)
    {
        Segment = segment;
        Score = score;
        MetricsJson = metricsJson;
        RaiseDomainEvent(new CustomerSegmentUpdatedEvent(Id, TenantId, CustomerId, segment));
    }
}

/// <summary>Churn prediction for a customer (advisory)</summary>
public sealed class ChurnPrediction : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid CustomerId { get; private set; }
    public decimal ChurnProbability { get; private set; }
    public ChurnRiskLevel RiskLevel { get; private set; }
    public string Recommendation { get; private set; }
    public string MetricsJson { get; private set; }
    public DateTimeOffset GeneratedAt { get; private set; }

    private ChurnPrediction()
    {
        Recommendation = string.Empty;
        MetricsJson = "{}";
    }

    public static ChurnPrediction Create(
        Guid tenantId, Guid customerId, decimal churnProbability, ChurnRiskLevel riskLevel,
        string recommendation, string metricsJson)
    {
        var entity = new ChurnPrediction
        {
            TenantId = tenantId,
            CustomerId = customerId,
            ChurnProbability = Math.Clamp(churnProbability, 0, 100),
            RiskLevel = riskLevel,
            Recommendation = recommendation,
            MetricsJson = metricsJson,
            GeneratedAt = DateTimeOffset.UtcNow
        };
        entity.RaiseDomainEvent(new ChurnPredictedEvent(entity.Id, tenantId, customerId, churnProbability, riskLevel));
        return entity;
    }

    public void Refresh(decimal churnProbability, ChurnRiskLevel riskLevel, string recommendation, string metricsJson)
    {
        ChurnProbability = Math.Clamp(churnProbability, 0, 100);
        RiskLevel = riskLevel;
        Recommendation = recommendation;
        MetricsJson = metricsJson;
        GeneratedAt = DateTimeOffset.UtcNow;
        RaiseDomainEvent(new ChurnPredictedEvent(Id, TenantId, CustomerId, churnProbability, riskLevel));
    }
}

/// <summary>Product recommendation insight (upsell/cross-sell — advisory)</summary>
public sealed class ProductRecommendation : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid ProductId { get; private set; }
    public ProductRecommendationType RecommendationType { get; private set; }
    public decimal Confidence { get; private set; }
    public string RelatedProductsJson { get; private set; }
    public Guid? TargetCustomerId { get; private set; }
    public Guid? BranchId { get; private set; }

    private ProductRecommendation() => RelatedProductsJson = "[]";

    public static ProductRecommendation Create(
        Guid tenantId, Guid productId, ProductRecommendationType recommendationType,
        decimal confidence, string relatedProductsJson, Guid? targetCustomerId = null, Guid? branchId = null)
    {
        var entity = new ProductRecommendation
        {
            TenantId = tenantId,
            ProductId = productId,
            RecommendationType = recommendationType,
            Confidence = Math.Clamp(confidence, 0, 100),
            RelatedProductsJson = relatedProductsJson,
            TargetCustomerId = targetCustomerId,
            BranchId = branchId
        };
        entity.RaiseDomainEvent(new RecommendationGeneratedEvent(entity.Id, tenantId, productId, recommendationType));
        return entity;
    }
}
