using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Ai.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Ai.Queries;

public record GetWarehouseStatusQuery(Guid TenantId) : IRequest<Result<WarehouseStatusDto>>;
public record GetWarehouseHistoryQuery(Guid TenantId, int Take = 20) : IRequest<Result<IReadOnlyList<WarehouseSyncRunDto>>>;
public record GetDataQualityDashboardQuery(Guid TenantId) : IRequest<Result<DataQualityDashboardDto>>;
public record GetFeatureDefinitionsQuery(Guid TenantId) : IRequest<Result<IReadOnlyList<FeatureDefinitionDto>>>;
public record GetFeatureSnapshotsQuery(Guid TenantId, AiDataFilterDto Filter) : IRequest<Result<IReadOnlyList<FeatureSnapshotDto>>>;
public record GetFeatureLineageQuery(Guid TenantId) : IRequest<Result<IReadOnlyList<FeatureLineageDto>>>;
public record GetDatasetDefinitionsQuery(Guid TenantId) : IRequest<Result<IReadOnlyList<MlDatasetDefinitionDto>>>;
public record GetDatasetExportsQuery(Guid TenantId, Guid DefinitionId) : IRequest<Result<IReadOnlyList<MlDatasetExportDto>>>;
