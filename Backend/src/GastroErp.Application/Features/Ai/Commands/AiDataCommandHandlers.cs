using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Ai.Commands;
using GastroErp.Application.Features.Ai.DTOs;
using GastroErp.Application.Features.Ai.Services;
using MediatR;

namespace GastroErp.Application.Features.Ai.Commands;

public sealed class TriggerWarehouseSyncCommandHandler : IRequestHandler<TriggerWarehouseSyncCommand, Result<WarehouseSyncRunDto>>
{
    private readonly IDataWarehouseSyncService _service;
    public TriggerWarehouseSyncCommandHandler(IDataWarehouseSyncService service) => _service = service;

    public async Task<Result<WarehouseSyncRunDto>> Handle(TriggerWarehouseSyncCommand request, CancellationToken cancellationToken)
        => Result<WarehouseSyncRunDto>.Success(
            await _service.SyncAsync(request.TenantId, request.Dto.LookbackDays, cancellationToken));
}

public sealed class ComputeFeaturesCommandHandler : IRequestHandler<ComputeFeaturesCommand, Result>
{
    private readonly IFeatureComputationService _service;
    public ComputeFeaturesCommandHandler(IFeatureComputationService service) => _service = service;

    public async Task<Result> Handle(ComputeFeaturesCommand request, CancellationToken cancellationToken)
    {
        await _service.ComputeAllAsync(request.TenantId, cancellationToken);
        return Result.Success();
    }
}

public sealed class CreateDatasetDefinitionCommandHandler : IRequestHandler<CreateDatasetDefinitionCommand, Result<MlDatasetDefinitionDto>>
{
    private readonly IMlDatasetBuilderService _service;
    public CreateDatasetDefinitionCommandHandler(IMlDatasetBuilderService service) => _service = service;

    public async Task<Result<MlDatasetDefinitionDto>> Handle(CreateDatasetDefinitionCommand request, CancellationToken cancellationToken)
        => Result<MlDatasetDefinitionDto>.Success(
            await _service.CreateDefinitionAsync(request.TenantId, request.Dto, cancellationToken));
}

public sealed class BuildDatasetCommandHandler : IRequestHandler<BuildDatasetCommand, Result<MlDatasetExportDto>>
{
    private readonly IMlDatasetBuilderService _service;
    public BuildDatasetCommandHandler(IMlDatasetBuilderService service) => _service = service;

    public async Task<Result<MlDatasetExportDto>> Handle(BuildDatasetCommand request, CancellationToken cancellationToken)
        => Result<MlDatasetExportDto>.Success(
            await _service.BuildAsync(request.TenantId, request.Dto, cancellationToken));
}
