using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Ai.DTOs;
using GastroErp.Application.Features.Ai.Queries;
using GastroErp.Application.Features.Ai.Services;
using MediatR;

namespace GastroErp.Application.Features.Ai.Queries;

public sealed class GetWarehouseStatusQueryHandler : IRequestHandler<GetWarehouseStatusQuery, Result<WarehouseStatusDto>>
{
    private readonly IDataWarehouseSyncService _service;
    public GetWarehouseStatusQueryHandler(IDataWarehouseSyncService service) => _service = service;
    public async Task<Result<WarehouseStatusDto>> Handle(GetWarehouseStatusQuery request, CancellationToken ct)
        => Result<WarehouseStatusDto>.Success(await _service.GetStatusAsync(request.TenantId, ct));
}

public sealed class GetWarehouseHistoryQueryHandler : IRequestHandler<GetWarehouseHistoryQuery, Result<IReadOnlyList<WarehouseSyncRunDto>>>
{
    private readonly IDataWarehouseSyncService _service;
    public GetWarehouseHistoryQueryHandler(IDataWarehouseSyncService service) => _service = service;
    public async Task<Result<IReadOnlyList<WarehouseSyncRunDto>>> Handle(GetWarehouseHistoryQuery request, CancellationToken ct)
        => Result<IReadOnlyList<WarehouseSyncRunDto>>.Success(await _service.GetHistoryAsync(request.TenantId, request.Take, ct));
}

public sealed class GetDataQualityDashboardQueryHandler : IRequestHandler<GetDataQualityDashboardQuery, Result<DataQualityDashboardDto>>
{
    private readonly IDataQualityService _service;
    public GetDataQualityDashboardQueryHandler(IDataQualityService service) => _service = service;
    public async Task<Result<DataQualityDashboardDto>> Handle(GetDataQualityDashboardQuery request, CancellationToken ct)
        => Result<DataQualityDashboardDto>.Success(await _service.EvaluateAsync(request.TenantId, ct));
}

public sealed class GetFeatureDefinitionsQueryHandler : IRequestHandler<GetFeatureDefinitionsQuery, Result<IReadOnlyList<FeatureDefinitionDto>>>
{
    private readonly IFeatureStoreService _service;
    public GetFeatureDefinitionsQueryHandler(IFeatureStoreService service) => _service = service;
    public async Task<Result<IReadOnlyList<FeatureDefinitionDto>>> Handle(GetFeatureDefinitionsQuery request, CancellationToken ct)
        => Result<IReadOnlyList<FeatureDefinitionDto>>.Success(await _service.GetDefinitionsAsync(request.TenantId, ct));
}

public sealed class GetFeatureSnapshotsQueryHandler : IRequestHandler<GetFeatureSnapshotsQuery, Result<IReadOnlyList<FeatureSnapshotDto>>>
{
    private readonly IFeatureStoreService _service;
    public GetFeatureSnapshotsQueryHandler(IFeatureStoreService service) => _service = service;
    public async Task<Result<IReadOnlyList<FeatureSnapshotDto>>> Handle(GetFeatureSnapshotsQuery request, CancellationToken ct)
        => Result<IReadOnlyList<FeatureSnapshotDto>>.Success(await _service.GetSnapshotsAsync(request.TenantId, request.Filter, ct));
}

public sealed class GetFeatureLineageQueryHandler : IRequestHandler<GetFeatureLineageQuery, Result<IReadOnlyList<FeatureLineageDto>>>
{
    private readonly IFeatureStoreService _service;
    public GetFeatureLineageQueryHandler(IFeatureStoreService service) => _service = service;
    public async Task<Result<IReadOnlyList<FeatureLineageDto>>> Handle(GetFeatureLineageQuery request, CancellationToken ct)
        => Result<IReadOnlyList<FeatureLineageDto>>.Success(await _service.GetLineageAsync(request.TenantId, ct));
}

public sealed class GetDatasetDefinitionsQueryHandler : IRequestHandler<GetDatasetDefinitionsQuery, Result<IReadOnlyList<MlDatasetDefinitionDto>>>
{
    private readonly IMlDatasetBuilderService _service;
    public GetDatasetDefinitionsQueryHandler(IMlDatasetBuilderService service) => _service = service;
    public async Task<Result<IReadOnlyList<MlDatasetDefinitionDto>>> Handle(GetDatasetDefinitionsQuery request, CancellationToken ct)
        => Result<IReadOnlyList<MlDatasetDefinitionDto>>.Success(await _service.GetDefinitionsAsync(request.TenantId, ct));
}

public sealed class GetDatasetExportsQueryHandler : IRequestHandler<GetDatasetExportsQuery, Result<IReadOnlyList<MlDatasetExportDto>>>
{
    private readonly IMlDatasetBuilderService _service;
    public GetDatasetExportsQueryHandler(IMlDatasetBuilderService service) => _service = service;
    public async Task<Result<IReadOnlyList<MlDatasetExportDto>>> Handle(GetDatasetExportsQuery request, CancellationToken ct)
        => Result<IReadOnlyList<MlDatasetExportDto>>.Success(await _service.GetExportsAsync(request.TenantId, request.DefinitionId, ct));
}
