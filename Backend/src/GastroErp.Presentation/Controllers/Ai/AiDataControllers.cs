using Asp.Versioning;
using GastroErp.Application.Features.Ai.Commands;
using GastroErp.Application.Features.Ai.DTOs;
using GastroErp.Application.Features.Ai.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Ai;

[ApiVersion("1.0")]
public class DataWarehouseController : BaseApiController
{
    [HttpGet($"{ApiRoutes.Ai.Warehouse}/status")]
    [HasPermission(Permissions.Ai.DataView)]
    public async Task<IActionResult> GetStatus()
        => HandleResult(await Mediator.Send(new GetWarehouseStatusQuery(TenantId)));

    [HttpGet($"{ApiRoutes.Ai.Warehouse}/history")]
    [HasPermission(Permissions.Ai.DataView)]
    public async Task<IActionResult> GetHistory([FromQuery] int take = 20)
        => HandleResult(await Mediator.Send(new GetWarehouseHistoryQuery(TenantId, take)));

    [HttpPost($"{ApiRoutes.Ai.Warehouse}/sync")]
    [HasPermission(Permissions.Ai.DataManage)]
    public async Task<IActionResult> Sync([FromBody] TriggerWarehouseSyncDto dto)
        => HandleResult(await Mediator.Send(new TriggerWarehouseSyncCommand(TenantId, dto)));
}

[ApiVersion("1.0")]
public class FeatureStoreController : BaseApiController
{
    [HttpGet($"{ApiRoutes.Ai.Features}/definitions")]
    [HasPermission(Permissions.Ai.DataView)]
    public async Task<IActionResult> GetDefinitions()
        => HandleResult(await Mediator.Send(new GetFeatureDefinitionsQuery(TenantId)));

    [HttpGet(ApiRoutes.Ai.Features)]
    [HasPermission(Permissions.Ai.DataView)]
    public async Task<IActionResult> GetSnapshots([FromQuery] AiDataFilterDto filter)
        => HandleResult(await Mediator.Send(new GetFeatureSnapshotsQuery(TenantId, filter)));

    [HttpGet($"{ApiRoutes.Ai.Features}/lineage")]
    [HasPermission(Permissions.Ai.DataView)]
    public async Task<IActionResult> GetLineage()
        => HandleResult(await Mediator.Send(new GetFeatureLineageQuery(TenantId)));

    [HttpPost($"{ApiRoutes.Ai.Features}/compute")]
    [HasPermission(Permissions.Ai.DataManage)]
    public async Task<IActionResult> ComputeFeatures()
        => HandleResult(await Mediator.Send(new ComputeFeaturesCommand(TenantId)));
}

[ApiVersion("1.0")]
public class MlDatasetController : BaseApiController
{
    [HttpGet(ApiRoutes.Ai.Datasets)]
    [HasPermission(Permissions.Ai.DataView)]
    public async Task<IActionResult> GetDefinitions()
        => HandleResult(await Mediator.Send(new GetDatasetDefinitionsQuery(TenantId)));

    [HttpPost(ApiRoutes.Ai.Datasets)]
    [HasPermission(Permissions.Ai.DataManage)]
    public async Task<IActionResult> CreateDefinition([FromBody] CreateDatasetDefinitionDto dto)
        => HandleResult(await Mediator.Send(new CreateDatasetDefinitionCommand(TenantId, dto)));

    [HttpPost($"{ApiRoutes.Ai.Datasets}/build")]
    [HasPermission(Permissions.Ai.DataManage)]
    public async Task<IActionResult> Build([FromBody] BuildDatasetDto dto)
        => HandleResult(await Mediator.Send(new BuildDatasetCommand(TenantId, dto)));

    [HttpGet($"{ApiRoutes.Ai.Datasets}/{{definitionId:guid}}/exports")]
    [HasPermission(Permissions.Ai.DataView)]
    public async Task<IActionResult> GetExports(Guid definitionId)
        => HandleResult(await Mediator.Send(new GetDatasetExportsQuery(TenantId, definitionId)));
}

[ApiVersion("1.0")]
public class DataQualityController : BaseApiController
{
    [HttpGet(ApiRoutes.Ai.DataQuality)]
    [HasPermission(Permissions.Ai.DataView)]
    public async Task<IActionResult> GetDashboard()
        => HandleResult(await Mediator.Send(new GetDataQualityDashboardQuery(TenantId)));
}
