using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Ai.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Ai.Commands;

public record TriggerWarehouseSyncCommand(Guid TenantId, TriggerWarehouseSyncDto Dto) : IRequest<Result<WarehouseSyncRunDto>>;
public record ComputeFeaturesCommand(Guid TenantId) : IRequest<Result>;
public record CreateDatasetDefinitionCommand(Guid TenantId, CreateDatasetDefinitionDto Dto) : IRequest<Result<MlDatasetDefinitionDto>>;
public record BuildDatasetCommand(Guid TenantId, BuildDatasetDto Dto) : IRequest<Result<MlDatasetExportDto>>;
