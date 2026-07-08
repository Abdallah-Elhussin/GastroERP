using GastroErp.Domain.Common;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Ai;

/// <summary>Registered AI/ML model metadata per tenant</summary>
public sealed class AiModelRegistry : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public ForecastType ForecastType { get; private set; }
    public AiModelProvider Provider { get; private set; }
    public string ModelName { get; private set; }
    public string Version { get; private set; }
    public bool IsActive { get; private set; }
    public string MetricsJson { get; private set; }

    private AiModelRegistry() { ModelName = string.Empty; Version = "1.0"; MetricsJson = "{}"; }

    public static AiModelRegistry Create(
        Guid tenantId, ForecastType forecastType, AiModelProvider provider,
        string modelName, string version = "1.0", string metricsJson = "{}")
        => new()
        {
            TenantId = tenantId,
            ForecastType = forecastType,
            Provider = provider,
            ModelName = modelName,
            Version = version,
            MetricsJson = metricsJson,
            IsActive = true
        };

    public void Deactivate() => IsActive = false;
}

/// <summary>Stored prediction run with explainability</summary>
public sealed class PredictionRun : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public ForecastType ForecastType { get; private set; }
    public AiModelProvider Provider { get; private set; }
    public string ModelVersion { get; private set; }
    public PredictionRunStatus Status { get; private set; }
    public Guid? BranchId { get; private set; }
    public Guid? EntityId { get; private set; }
    public DateOnly ForecastDate { get; private set; }
    public double Confidence { get; private set; }
    public string OutputJson { get; private set; }
    public string ExplainabilityJson { get; private set; }
    public string? ErrorMessage { get; private set; }

    private PredictionRun()
    {
        ModelVersion = "1.0";
        OutputJson = "{}";
        ExplainabilityJson = "{}";
    }

    public static PredictionRun Create(
        Guid tenantId, ForecastType forecastType, AiModelProvider provider,
        DateOnly forecastDate, Guid? branchId = null, Guid? entityId = null, string modelVersion = "1.0")
        => new()
        {
            TenantId = tenantId,
            ForecastType = forecastType,
            Provider = provider,
            ModelVersion = modelVersion,
            ForecastDate = forecastDate,
            BranchId = branchId,
            EntityId = entityId,
            Status = PredictionRunStatus.Running
        };

    public void Complete(double confidence, string outputJson, string explainabilityJson)
    {
        Status = PredictionRunStatus.Completed;
        Confidence = confidence;
        OutputJson = outputJson;
        ExplainabilityJson = explainabilityJson;
    }

    public void Fail(string error)
    {
        Status = PredictionRunStatus.Failed;
        ErrorMessage = error;
    }
}
