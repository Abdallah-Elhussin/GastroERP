using Asp.Versioning;
using GastroErp.Application.Features.Organization.Commands;
using GastroErp.Application.Features.Organization.DTOs;
using GastroErp.Application.Features.Organization.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Organization;

/// <summary>
/// Branch management within a company
/// </summary>
[ApiVersion("1.0")]
public class BranchController : BaseApiController
{
    /// <summary>
    /// Get all branches
    /// </summary>
    [HttpGet(ApiRoutes.Organization.Branches)]
    [HasPermission(Permissions.Branch.View)]
    public async Task<IActionResult> GetBranches([FromQuery] Guid? companyId, [FromQuery] PaginationQuery query)
    {
        return HandlePagedResult(await Mediator.Send(new GetBranchesQuery(TenantId, companyId, query.Page, query.PageSize, query.Search))); // Update later if paginated
    }

    /// <summary>
    /// Get a branch by its ID
    /// </summary>
    [HttpGet($"{ApiRoutes.Organization.Branches}/{{id:guid}}")]
    [HasPermission(Permissions.Branch.View)]
    public async Task<IActionResult> GetBranchById(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetBranchByIdQuery(id)));
    }

    /// <summary>
    /// Create a new branch
    /// </summary>
    [HttpPost(ApiRoutes.Organization.Branches)]
    [HasPermission(Permissions.Branch.Create)]
    public async Task<IActionResult> CreateBranch([FromBody] CreateBranchDto dto)
    {
        return HandleResult(await Mediator.Send(new CreateBranchCommand(dto)));
    }

    /// <summary>
    /// Update an existing branch
    /// </summary>
    [HttpPut($"{ApiRoutes.Organization.Branches}/{{id:guid}}")]
    [HasPermission(Permissions.Branch.Update)]
    public async Task<IActionResult> UpdateBranch(Guid id, [FromBody] UpdateBranchDto dto)
    {
        return HandleResult(await Mediator.Send(new UpdateBranchCommand(id, dto)));
    }

    /// <summary>
    /// Change branch status (Activate/Deactivate)
    /// </summary>
    [HttpPost($"{ApiRoutes.Organization.Branches}/{{id:guid}}/status")]
    [HasPermission(Permissions.Branch.Activate)]
    public async Task<IActionResult> ChangeBranchStatus(Guid id, [FromBody] bool isActive)
    {
        return HandleResult(await Mediator.Send(new ChangeBranchStatusCommand(id, isActive)));
    }

    /// <summary>
    /// Archive a branch
    /// </summary>
    [HttpPost($"{ApiRoutes.Organization.Branches}/{{id:guid}}/archive")]
    [HasPermission(Permissions.Branch.Update)] // Use update permission or specific archive permission
    public async Task<IActionResult> ArchiveBranch(Guid id)
    {
        return HandleResult(await Mediator.Send(new ArchiveBranchCommand(id)));
    }

    /// <summary>
    /// Restore an archived branch
    /// </summary>
    [HttpPost($"{ApiRoutes.Organization.Branches}/{{id:guid}}/restore")]
    [HasPermission(Permissions.Branch.Update)]
    public async Task<IActionResult> RestoreBranch(Guid id)
    {
        return HandleResult(await Mediator.Send(new RestoreBranchCommand(id)));
    }
}
