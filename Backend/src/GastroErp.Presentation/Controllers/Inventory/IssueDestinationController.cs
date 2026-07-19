using Asp.Versioning;
using GastroErp.Application.Features.Inventory.Commands;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Application.Features.Inventory.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Inventory;

/// <summary>جهات الصرف — master data for inventory issue destinations.</summary>
[ApiVersion("1.0")]
public class IssueDestinationController : BaseApiController
{
    [HttpGet(ApiRoutes.Inventory.IssueDestinations)]
    [HasPermission(Permissions.Inventory.IssueDestinations.View)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool activeOnly = false,
        [FromQuery] string? search = null,
        [FromQuery] byte? destinationType = null)
        => HandleResult(await Mediator.Send(new GetIssueDestinationsQuery(TenantId, activeOnly, search, destinationType)));

    [HttpGet($"{ApiRoutes.Inventory.IssueDestinations}/{{id:guid}}")]
    [HasPermission(Permissions.Inventory.IssueDestinations.View)]
    public async Task<IActionResult> GetById(Guid id)
        => HandleResult(await Mediator.Send(new GetIssueDestinationByIdQuery(id)));

    [HttpPost(ApiRoutes.Inventory.IssueDestinations)]
    [HasPermission(Permissions.Inventory.IssueDestinations.Create)]
    public async Task<IActionResult> Create([FromBody] CreateIssueDestinationDto dto)
        => HandleResult(await Mediator.Send(new CreateIssueDestinationCommand(dto with { TenantId = TenantId })));

    [HttpPut($"{ApiRoutes.Inventory.IssueDestinations}/{{id:guid}}")]
    [HasPermission(Permissions.Inventory.IssueDestinations.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateIssueDestinationDto dto)
        => HandleResult(await Mediator.Send(new UpdateIssueDestinationCommand(id, dto with { TenantId = TenantId })));

    [HttpDelete($"{ApiRoutes.Inventory.IssueDestinations}/{{id:guid}}")]
    [HasPermission(Permissions.Inventory.IssueDestinations.Delete)]
    public async Task<IActionResult> Delete(Guid id)
        => HandleResult(await Mediator.Send(new DeleteIssueDestinationCommand(id, TenantId)));

    [HttpPost($"{ApiRoutes.Inventory.IssueDestinations}/{{id:guid}}/activate")]
    [HasPermission(Permissions.Inventory.IssueDestinations.Edit)]
    public async Task<IActionResult> Activate(Guid id)
        => HandleResult(await Mediator.Send(new ActivateIssueDestinationCommand(id, TenantId)));

    [HttpPost($"{ApiRoutes.Inventory.IssueDestinations}/{{id:guid}}/deactivate")]
    [HasPermission(Permissions.Inventory.IssueDestinations.Edit)]
    public async Task<IActionResult> Deactivate(Guid id)
        => HandleResult(await Mediator.Send(new DeactivateIssueDestinationCommand(id, TenantId)));
}
