using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Ai.DTOs;

public record AiDataFilterDto(
    DateOnly? FromDate = null, DateOnly? ToDate = null,
    Guid? BranchId = null, AiFeatureGroup? FeatureGroup = null);

public record WarehouseSyncRunDto(
    Guid Id, WarehouseSyncStatus Status, DateTimeOffset? StartedAt, DateTimeOffset? FinishedAt,
    int SalesFactsWritten, int InventoryFactsWritten, string? ErrorMessage);

public record WarehouseStatusDto(
    DateTimeOffset? LastSyncAt, WarehouseSyncStatus? LastStatus,
    int TotalSalesFacts, int TotalInventoryFacts, bool IsStale);

public record TriggerWarehouseSyncDto(int LookbackDays = 90);

public record DataQualityMetricDto(
    string MetricName, DataQualityLevel Level, double Score,
    DateTimeOffset MeasuredAt, string DetailsJson);

public record DataQualityDashboardDto(
    double OverallScore, DataQualityLevel OverallLevel,
    IReadOnlyList<DataQualityMetricDto> Metrics);

public record FeatureDefinitionDto(
    Guid Id, AiFeatureGroup FeatureGroup, FeatureEntityType EntityType,
    string Name, string Description, int Version, bool IsActive);

public record FeatureSnapshotDto(
    Guid Id, AiFeatureGroup FeatureGroup, FeatureEntityType EntityType,
    Guid EntityId, DateOnly AsOfDate, string FeaturesJson);

public record FeatureLineageDto(
    AiFeatureGroup FeatureGroup, string SourceTables,
    DateTimeOffset LastRefreshedAt, double QualityScore, int RecordCount);

public record CreateDatasetDefinitionDto(
    string Name, string Description, AiFeatureGroup PrimaryFeatureGroup, string SpecJson);

public record MlDatasetDefinitionDto(
    Guid Id, string Name, string Description, AiFeatureGroup PrimaryFeatureGroup,
    bool IsActive, DateTimeOffset CreatedAt);

public record BuildDatasetDto(
    Guid DefinitionId, MlDatasetFormat Format = MlDatasetFormat.Csv,
    MlDatasetSplit Split = MlDatasetSplit.Full, double TrainRatio = 0.7, double ValidationRatio = 0.15);

public record MlDatasetExportDto(
    Guid Id, Guid DefinitionId, MlDatasetFormat Format, MlDatasetSplit Split,
    MlDatasetExportStatus Status, int RowCount, string? ContentPath,
    DateTimeOffset CreatedAt, DateTimeOffset? CompletedAt, string? ErrorMessage);
