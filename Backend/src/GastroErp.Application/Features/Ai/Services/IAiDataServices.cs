using GastroErp.Application.Features.Ai.DTOs;
using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Ai.Services;

public interface IDataWarehouseSyncService
{
    Task<WarehouseSyncRunDto> SyncAsync(Guid tenantId, int lookbackDays = 90, CancellationToken ct = default);
    Task<WarehouseStatusDto> GetStatusAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<WarehouseSyncRunDto>> GetHistoryAsync(Guid tenantId, int take = 20, CancellationToken ct = default);
}

public interface IDataQualityService
{
    Task<DataQualityDashboardDto> EvaluateAsync(Guid tenantId, CancellationToken ct = default);
}

public interface IFeatureStoreService
{
    Task<IReadOnlyList<FeatureDefinitionDto>> GetDefinitionsAsync(Guid tenantId, CancellationToken ct = default);
    Task EnsureDefaultDefinitionsAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<FeatureSnapshotDto>> GetSnapshotsAsync(
        Guid tenantId, AiDataFilterDto filter, CancellationToken ct = default);
    Task<IReadOnlyList<FeatureLineageDto>> GetLineageAsync(Guid tenantId, CancellationToken ct = default);
}

public interface IFeatureComputationService
{
    Task ComputeAllAsync(Guid tenantId, CancellationToken ct = default);
    Task ComputeGroupAsync(Guid tenantId, AiFeatureGroup group, CancellationToken ct = default);
}

public interface IMlDatasetBuilderService
{
    Task<MlDatasetDefinitionDto> CreateDefinitionAsync(Guid tenantId, CreateDatasetDefinitionDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<MlDatasetDefinitionDto>> GetDefinitionsAsync(Guid tenantId, CancellationToken ct = default);
    Task<MlDatasetExportDto> BuildAsync(Guid tenantId, BuildDatasetDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<MlDatasetExportDto>> GetExportsAsync(Guid tenantId, Guid definitionId, CancellationToken ct = default);
}

public interface IAiDataJobExecutor
{
    Task SyncWarehouseAsync(Guid tenantId, CancellationToken ct = default);
    Task ComputeFeaturesAsync(Guid tenantId, CancellationToken ct = default);
    Task EvaluateDataQualityAsync(Guid tenantId, CancellationToken ct = default);
    Task RefreshForecastsAsync(Guid tenantId, CancellationToken ct = default);
    Task RefreshRecommendationsAsync(Guid tenantId, CancellationToken ct = default);
}
