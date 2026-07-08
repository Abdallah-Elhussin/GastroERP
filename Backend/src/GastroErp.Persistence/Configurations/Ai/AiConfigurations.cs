using GastroErp.Domain.Entities.Ai;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GastroErp.Persistence.Configurations.Ai;

public sealed class WarehouseSyncRunConfiguration : IEntityTypeConfiguration<WarehouseSyncRun>
{
    public void Configure(EntityTypeBuilder<WarehouseSyncRun> builder)
    {
        builder.ToTable("WarehouseSyncRuns");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000);
        builder.Property(x => x.CreatedBy).HasMaxLength(200);
        builder.Property(x => x.UpdatedBy).HasMaxLength(200);
        builder.Property(x => x.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => new { x.TenantId, x.CreatedAt }).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class SalesDailySnapshotConfiguration : IEntityTypeConfiguration<SalesDailySnapshot>
{
    public void Configure(EntityTypeBuilder<SalesDailySnapshot> builder)
    {
        builder.ToTable("SalesDailySnapshots");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.GrossRevenue).HasPrecision(18, 4);
        builder.Property(x => x.TaxTotal).HasPrecision(18, 4);
        builder.Property(x => x.DiscountTotal).HasPrecision(18, 4);
        builder.Property(x => x.NetRevenue).HasPrecision(18, 4);
        builder.Property(x => x.AverageOrderValue).HasPrecision(18, 4);
        builder.Property(x => x.CreatedBy).HasMaxLength(200);
        builder.Property(x => x.UpdatedBy).HasMaxLength(200);
        builder.Property(x => x.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => new { x.TenantId, x.BranchId, x.BusinessDate }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

public sealed class InventoryDailySnapshotConfiguration : IEntityTypeConfiguration<InventoryDailySnapshot>
{
    public void Configure(EntityTypeBuilder<InventoryDailySnapshot> builder)
    {
        builder.ToTable("InventoryDailySnapshots");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NetQuantityChange).HasPrecision(18, 4);
        builder.Property(x => x.ConsumptionQty).HasPrecision(18, 4);
        builder.Property(x => x.WasteQty).HasPrecision(18, 4);
        builder.Property(x => x.ClosingBalance).HasPrecision(18, 4);
        builder.Property(x => x.CreatedBy).HasMaxLength(200);
        builder.Property(x => x.UpdatedBy).HasMaxLength(200);
        builder.Property(x => x.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => new { x.TenantId, x.InventoryItemId, x.BusinessDate }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

public sealed class DataQualityMetricConfiguration : IEntityTypeConfiguration<DataQualityMetric>
{
    public void Configure(EntityTypeBuilder<DataQualityMetric> builder)
    {
        builder.ToTable("DataQualityMetrics");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.MetricName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.DetailsJson).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(200);
        builder.Property(x => x.UpdatedBy).HasMaxLength(200);
        builder.Property(x => x.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => new { x.TenantId, x.MetricName, x.MeasuredAt }).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class FeatureDefinitionConfiguration : IEntityTypeConfiguration<FeatureDefinition>
{
    public void Configure(EntityTypeBuilder<FeatureDefinition> builder)
    {
        builder.ToTable("FeatureDefinitions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.CreatedBy).HasMaxLength(200);
        builder.Property(x => x.UpdatedBy).HasMaxLength(200);
        builder.Property(x => x.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => new { x.TenantId, x.FeatureGroup, x.Version }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

public sealed class FeatureStoreSnapshotConfiguration : IEntityTypeConfiguration<FeatureStoreSnapshot>
{
    public void Configure(EntityTypeBuilder<FeatureStoreSnapshot> builder)
    {
        builder.ToTable("FeatureStoreSnapshots");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FeaturesJson).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(200);
        builder.Property(x => x.UpdatedBy).HasMaxLength(200);
        builder.Property(x => x.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => new { x.TenantId, x.FeatureGroup, x.EntityType, x.EntityId, x.AsOfDate })
            .IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

public sealed class FeatureLineageConfiguration : IEntityTypeConfiguration<FeatureLineage>
{
    public void Configure(EntityTypeBuilder<FeatureLineage> builder)
    {
        builder.ToTable("FeatureLineages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SourceTables).IsRequired().HasMaxLength(500);
        builder.Property(x => x.CreatedBy).HasMaxLength(200);
        builder.Property(x => x.UpdatedBy).HasMaxLength(200);
        builder.Property(x => x.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => new { x.TenantId, x.FeatureGroup }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

public sealed class MlDatasetDefinitionConfiguration : IEntityTypeConfiguration<MlDatasetDefinition>
{
    public void Configure(EntityTypeBuilder<MlDatasetDefinition> builder)
    {
        builder.ToTable("MlDatasetDefinitions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.SpecJson).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(200);
        builder.Property(x => x.UpdatedBy).HasMaxLength(200);
        builder.Property(x => x.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => new { x.TenantId, x.Name }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

public sealed class MlDatasetExportConfiguration : IEntityTypeConfiguration<MlDatasetExport>
{
    public void Configure(EntityTypeBuilder<MlDatasetExport> builder)
    {
        builder.ToTable("MlDatasetExports");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ContentPath).HasMaxLength(500);
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000);
        builder.Property(x => x.CreatedBy).HasMaxLength(200);
        builder.Property(x => x.UpdatedBy).HasMaxLength(200);
        builder.Property(x => x.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => new { x.TenantId, x.DatasetDefinitionId, x.CreatedAt }).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class AiModelRegistryConfiguration : IEntityTypeConfiguration<AiModelRegistry>
{
    public void Configure(EntityTypeBuilder<AiModelRegistry> builder)
    {
        builder.ToTable("AiModelRegistries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ModelName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Version).IsRequired().HasMaxLength(20);
        builder.Property(x => x.MetricsJson).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(200);
        builder.Property(x => x.UpdatedBy).HasMaxLength(200);
        builder.Property(x => x.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => new { x.TenantId, x.ForecastType, x.Provider }).IsUnique().HasFilter("[IsDeleted] = 0 AND [IsActive] = 1");
    }
}

public sealed class PredictionRunConfiguration : IEntityTypeConfiguration<PredictionRun>
{
    public void Configure(EntityTypeBuilder<PredictionRun> builder)
    {
        builder.ToTable("PredictionRuns");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ModelVersion).IsRequired().HasMaxLength(20);
        builder.Property(x => x.OutputJson).IsRequired();
        builder.Property(x => x.ExplainabilityJson).IsRequired();
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000);
        builder.Property(x => x.CreatedBy).HasMaxLength(200);
        builder.Property(x => x.UpdatedBy).HasMaxLength(200);
        builder.Property(x => x.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => new { x.TenantId, x.ForecastType, x.ForecastDate }).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class RecommendationActionConfiguration : IEntityTypeConfiguration<RecommendationAction>
{
    public void Configure(EntityTypeBuilder<RecommendationAction> builder)
    {
        builder.ToTable("RecommendationActions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Description).IsRequired().HasMaxLength(2000);
        builder.Property(x => x.PayloadJson).IsRequired();
        builder.Property(x => x.ExplainabilityJson).IsRequired();
        builder.Property(x => x.ReferenceType).HasMaxLength(100);
        builder.Property(x => x.DismissReason).HasMaxLength(500);
        builder.Property(x => x.CreatedBy).HasMaxLength(200);
        builder.Property(x => x.UpdatedBy).HasMaxLength(200);
        builder.Property(x => x.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => new { x.TenantId, x.Type, x.Status }).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class AiGenerativeLogConfiguration : IEntityTypeConfiguration<AiGenerativeLog>
{
    public void Configure(EntityTypeBuilder<AiGenerativeLog> builder)
    {
        builder.ToTable("AiGenerativeLogs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.InputText).IsRequired().HasMaxLength(4000);
        builder.Property(x => x.OutputText).IsRequired();
        builder.Property(x => x.MetadataJson).HasMaxLength(4000);
        builder.Property(x => x.CreatedBy).HasMaxLength(200);
        builder.Property(x => x.UpdatedBy).HasMaxLength(200);
        builder.Property(x => x.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => new { x.TenantId, x.InteractionType, x.CreatedAt }).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class VoiceOrderDraftConfiguration : IEntityTypeConfiguration<VoiceOrderDraft>
{
    public void Configure(EntityTypeBuilder<VoiceOrderDraft> builder)
    {
        builder.ToTable("VoiceOrderDrafts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Transcript).IsRequired().HasMaxLength(2000);
        builder.Property(x => x.ParsedItemsJson).IsRequired();
        builder.Property(x => x.EstimatedTotal).HasPrecision(18, 4);
        builder.Property(x => x.Currency).HasMaxLength(3);
        builder.Property(x => x.CreatedBy).HasMaxLength(200);
        builder.Property(x => x.UpdatedBy).HasMaxLength(200);
        builder.Property(x => x.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => new { x.TenantId, x.Status }).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class FraudAlertConfiguration : IEntityTypeConfiguration<FraudAlert>
{
    public void Configure(EntityTypeBuilder<FraudAlert> builder)
    {
        builder.ToTable("FraudAlerts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RiskScore).HasPrecision(5, 2);
        builder.Property(x => x.Source).IsRequired().HasMaxLength(100);
        builder.Property(x => x.ReferenceType).HasMaxLength(100);
        builder.Property(x => x.DetailsJson).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(200);
        builder.Property(x => x.UpdatedBy).HasMaxLength(200);
        builder.Property(x => x.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => new { x.TenantId, x.AlertType, x.Status, x.CreatedAt }).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class CustomerSegmentConfiguration : IEntityTypeConfiguration<CustomerSegment>
{
    public void Configure(EntityTypeBuilder<CustomerSegment> builder)
    {
        builder.ToTable("CustomerSegments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Score).HasPrecision(8, 4);
        builder.Property(x => x.MetricsJson).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(200);
        builder.Property(x => x.UpdatedBy).HasMaxLength(200);
        builder.Property(x => x.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => new { x.TenantId, x.CustomerId }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.Segment }).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class ChurnPredictionConfiguration : IEntityTypeConfiguration<ChurnPrediction>
{
    public void Configure(EntityTypeBuilder<ChurnPrediction> builder)
    {
        builder.ToTable("ChurnPredictions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ChurnProbability).HasPrecision(5, 2);
        builder.Property(x => x.Recommendation).IsRequired().HasMaxLength(500);
        builder.Property(x => x.MetricsJson).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(200);
        builder.Property(x => x.UpdatedBy).HasMaxLength(200);
        builder.Property(x => x.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => new { x.TenantId, x.CustomerId }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.RiskLevel }).HasFilter("[IsDeleted] = 0");
    }
}

public sealed class ProductRecommendationConfiguration : IEntityTypeConfiguration<ProductRecommendation>
{
    public void Configure(EntityTypeBuilder<ProductRecommendation> builder)
    {
        builder.ToTable("ProductRecommendations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Confidence).HasPrecision(5, 2);
        builder.Property(x => x.RelatedProductsJson).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(200);
        builder.Property(x => x.UpdatedBy).HasMaxLength(200);
        builder.Property(x => x.DeletedBy).HasMaxLength(200);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => new { x.TenantId, x.ProductId, x.RecommendationType }).HasFilter("[IsDeleted] = 0");
    }
}
