using Asp.Versioning;
using GastroErp.Application.Features.Inventory.Commands;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Application.Features.Inventory.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Inventory;

/// <summary>
/// Company / branch inventory configuration policies.
/// </summary>
[ApiVersion("1.0")]
public class InventorySettingsController : BaseApiController
{
    [HttpGet(ApiRoutes.Inventory.Settings)]
    [HasPermission(Permissions.Inventory.Settings.View)]
    public async Task<IActionResult> Get([FromQuery] Guid? branchId = null, [FromQuery] Guid? companyId = null)
        => HandleResult(await Mediator.Send(
            new GetInventorySettingQuery(TenantId, branchId ?? BranchId, companyId)));

    [HttpGet($"{ApiRoutes.Inventory.Settings}/by-company/{{companyId:guid}}")]
    [HasPermission(Permissions.Inventory.Settings.View)]
    public async Task<IActionResult> GetByCompany(Guid companyId)
        => HandleResult(await Mediator.Send(new GetInventorySettingsByCompanyQuery(TenantId, companyId)));

    [HttpPut(ApiRoutes.Inventory.Settings)]
    [HasPermission(Permissions.Inventory.Settings.Edit)]
    public async Task<IActionResult> Update([FromBody] UpsertInventorySettingDto dto)
    {
        var payload = dto with
        {
            TenantId = TenantId,
            BranchId = dto.BranchId ?? BranchId
        };
        return HandleResult(await Mediator.Send(new UpdateInventorySettingsCommand(payload)));
    }

    [HttpPost($"{ApiRoutes.Inventory.Settings}/reset")]
    [HasPermission(Permissions.Inventory.Settings.Reset)]
    public async Task<IActionResult> Reset([FromQuery] Guid? branchId = null, [FromQuery] Guid? companyId = null)
        => HandleResult(await Mediator.Send(
            new ResetInventorySettingsCommand(TenantId, branchId ?? BranchId, companyId)));
}
