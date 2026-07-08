using GastroErp.Domain.Common;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Ai;

/// <summary>Warehouse sync execution audit</summary>
public sealed class WarehouseSyncRun : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public WarehouseSyncStatus Status { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? FinishedAt { get; private set; }
    public int SalesFactsWritten { get; private set; }
    public int InventoryFactsWritten { get; private set; }
    public string? ErrorMessage { get; private set; }

    private WarehouseSyncRun() { }

    public static WarehouseSyncRun Create(Guid tenantId)
        => new() { TenantId = tenantId, Status = WarehouseSyncStatus.Queued };

    public void Start()
    {
        Status = WarehouseSyncStatus.Running;
        StartedAt = DateTimeOffset.UtcNow;
    }

    public void Complete(int salesFacts, int inventoryFacts)
    {
        Status = WarehouseSyncStatus.Succeeded;
        SalesFactsWritten = salesFacts;
        InventoryFactsWritten = inventoryFacts;
        FinishedAt = DateTimeOffset.UtcNow;
    }

    public void Fail(string error)
    {
        Status = WarehouseSyncStatus.Failed;
        ErrorMessage = error;
        FinishedAt = DateTimeOffset.UtcNow;
    }
}

/// <summary>Daily sales aggregate per branch (AI read model)</summary>
public sealed class SalesDailySnapshot : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid BranchId { get; private set; }
    public DateOnly BusinessDate { get; private set; }
    public int OrderCount { get; private set; }
    public decimal GrossRevenue { get; private set; }
    public decimal TaxTotal { get; private set; }
    public decimal DiscountTotal { get; private set; }
    public decimal NetRevenue { get; private set; }
    public decimal AverageOrderValue { get; private set; }

    private SalesDailySnapshot() { }

    public static SalesDailySnapshot Create(
        Guid tenantId, Guid branchId, DateOnly businessDate,
        int orderCount, decimal grossRevenue, decimal taxTotal, decimal discountTotal)
    {
        var net = grossRevenue - discountTotal;
        return new SalesDailySnapshot
        {
            TenantId = tenantId,
            BranchId = branchId,
            BusinessDate = businessDate,
            OrderCount = orderCount,
            GrossRevenue = grossRevenue,
            TaxTotal = taxTotal,
            DiscountTotal = discountTotal,
            NetRevenue = net,
            AverageOrderValue = orderCount > 0 ? net / orderCount : 0
        };
    }
}

/// <summary>Daily inventory movement aggregate per item</summary>
public sealed class InventoryDailySnapshot : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid InventoryItemId { get; private set; }
    public DateOnly BusinessDate { get; private set; }
    public decimal NetQuantityChange { get; private set; }
    public decimal ConsumptionQty { get; private set; }
    public decimal WasteQty { get; private set; }
    public decimal ClosingBalance { get; private set; }

    private InventoryDailySnapshot() { }

    public static InventoryDailySnapshot Create(
        Guid tenantId, Guid itemId, DateOnly businessDate,
        decimal netChange, decimal consumption, decimal waste, decimal closingBalance)
        => new()
        {
            TenantId = tenantId,
            InventoryItemId = itemId,
            BusinessDate = businessDate,
            NetQuantityChange = netChange,
            ConsumptionQty = consumption,
            WasteQty = waste,
            ClosingBalance = closingBalance
        };
}

/// <summary>Data quality metric snapshot</summary>
public sealed class DataQualityMetric : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public string MetricName { get; private set; }
    public DataQualityLevel Level { get; private set; }
    public double Score { get; private set; }
    public string DetailsJson { get; private set; }
    public DateTimeOffset MeasuredAt { get; private set; }

    private DataQualityMetric() { MetricName = string.Empty; DetailsJson = "{}"; }

    public static DataQualityMetric Create(
        Guid tenantId, string metricName, DataQualityLevel level, double score, string detailsJson)
        => new()
        {
            TenantId = tenantId,
            MetricName = metricName,
            Level = level,
            Score = score,
            DetailsJson = detailsJson,
            MeasuredAt = DateTimeOffset.UtcNow
        };
}

/// <summary>ML feature definition metadata</summary>
public sealed class FeatureDefinition : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public AiFeatureGroup FeatureGroup { get; private set; }
    public FeatureEntityType EntityType { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public int Version { get; private set; }
    public bool IsActive { get; private set; }

    private FeatureDefinition() { Name = string.Empty; Description = string.Empty; }

    public static FeatureDefinition Create(
        Guid tenantId, AiFeatureGroup group, FeatureEntityType entityType, string name, string description, int version = 1)
        => new()
        {
            TenantId = tenantId,
            FeatureGroup = group,
            EntityType = entityType,
            Name = name,
            Description = description,
            Version = version,
            IsActive = true
        };
}

/// <summary>Point-in-time feature values for an entity</summary>
public sealed class FeatureStoreSnapshot : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public AiFeatureGroup FeatureGroup { get; private set; }
    public FeatureEntityType EntityType { get; private set; }
    public Guid EntityId { get; private set; }
    public DateOnly AsOfDate { get; private set; }
    public string FeaturesJson { get; private set; }

    private FeatureStoreSnapshot() { FeaturesJson = "{}"; }

    public static FeatureStoreSnapshot Create(
        Guid tenantId, AiFeatureGroup group, FeatureEntityType entityType,
        Guid entityId, DateOnly asOfDate, string featuresJson)
        => new()
        {
            TenantId = tenantId,
            FeatureGroup = group,
            EntityType = entityType,
            EntityId = entityId,
            AsOfDate = asOfDate,
            FeaturesJson = featuresJson
        };
}

/// <summary>Feature lineage and freshness tracking</summary>
public sealed class FeatureLineage : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public AiFeatureGroup FeatureGroup { get; private set; }
    public string SourceTables { get; private set; }
    public DateTimeOffset LastRefreshedAt { get; private set; }
    public double QualityScore { get; private set; }
    public int RecordCount { get; private set; }

    private FeatureLineage() { SourceTables = string.Empty; }

    public static FeatureLineage Create(
        Guid tenantId, AiFeatureGroup group, string sourceTables, double qualityScore, int recordCount)
        => new()
        {
            TenantId = tenantId,
            FeatureGroup = group,
            SourceTables = sourceTables,
            LastRefreshedAt = DateTimeOffset.UtcNow,
            QualityScore = qualityScore,
            RecordCount = recordCount
        };

    public void Refresh(double qualityScore, int recordCount)
    {
        LastRefreshedAt = DateTimeOffset.UtcNow;
        QualityScore = qualityScore;
        RecordCount = recordCount;
    }
}

/// <summary>ML dataset specification</summary>
public sealed class MlDatasetDefinition : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public AiFeatureGroup PrimaryFeatureGroup { get; private set; }
    public string SpecJson { get; private set; }
    public bool IsActive { get; private set; }

    private MlDatasetDefinition() { Name = string.Empty; Description = string.Empty; SpecJson = "{}"; }

    public static MlDatasetDefinition Create(
        Guid tenantId, string name, string description, AiFeatureGroup group, string specJson)
        => new()
        {
            TenantId = tenantId,
            Name = name,
            Description = description,
            PrimaryFeatureGroup = group,
            SpecJson = specJson,
            IsActive = true
        };
}

/// <summary>ML dataset export audit</summary>
public sealed class MlDatasetExport : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid DatasetDefinitionId { get; private set; }
    public MlDatasetFormat Format { get; private set; }
    public MlDatasetSplit Split { get; private set; }
    public MlDatasetExportStatus Status { get; private set; }
    public int RowCount { get; private set; }
    public string ContentPath { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    private MlDatasetExport() { ContentPath = string.Empty; }

    public static MlDatasetExport Create(
        Guid tenantId, Guid definitionId, MlDatasetFormat format, MlDatasetSplit split)
        => new()
        {
            TenantId = tenantId,
            DatasetDefinitionId = definitionId,
            Format = format,
            Split = split,
            Status = MlDatasetExportStatus.Pending
        };

    public void Start() => Status = MlDatasetExportStatus.Running;

    public void Complete(int rowCount, string contentPath)
    {
        Status = MlDatasetExportStatus.Completed;
        RowCount = rowCount;
        ContentPath = contentPath;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void Fail(string error)
    {
        Status = MlDatasetExportStatus.Failed;
        ErrorMessage = error;
        CompletedAt = DateTimeOffset.UtcNow;
    }
}
