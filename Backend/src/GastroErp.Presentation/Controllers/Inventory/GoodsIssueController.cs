using Asp.Versioning;
using GastroErp.Application.Features.Inventory.Commands;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Application.Features.Inventory.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Inventory;

/// <summary>
/// Goods Issue — Draft → Approve → Post (stock OUT via InventoryMovementPipeline) | Cancelled.
/// </summary>
[ApiVersion("1.0")]
public class GoodsIssueController : BaseApiController
{
    [HttpGet(ApiRoutes.Inventory.GoodsIssues)]
    [HasPermission(Permissions.Inventory.View)]
    public async Task<IActionResult> GetGoodsIssues(
        [FromQuery] PaginationQuery query,
        [FromQuery] byte? status = null,
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null)
        => HandlePagedResult(await Mediator.Send(new GetGoodsIssuesQuery(
            TenantId, status, query.Search, from, to, query.Page, query.PageSize)));

    [HttpGet($"{ApiRoutes.Inventory.GoodsIssues}/{{id:guid}}")]
    [HasPermission(Permissions.Inventory.View)]
    public async Task<IActionResult> GetById(Guid id)
        => HandleResult(await Mediator.Send(new GetGoodsIssueByIdQuery(id)));

    [HttpPost($"{ApiRoutes.Inventory.GoodsIssues}/next-number")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> NextNumber()
        => HandleResult(await Mediator.Send(new GenerateGoodsIssueNumberCommand(TenantId)));

    [HttpPost(ApiRoutes.Inventory.GoodsIssues)]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> Create([FromBody] CreateGoodsIssueDto dto)
        => HandleResult(await Mediator.Send(new CreateGoodsIssueCommand(dto with { TenantId = TenantId })));

    [HttpPut($"{ApiRoutes.Inventory.GoodsIssues}/{{id:guid}}")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGoodsIssueDto dto)
        => HandleResult(await Mediator.Send(new UpdateGoodsIssueCommand(id, dto with { TenantId = TenantId })));

    [HttpPost($"{ApiRoutes.Inventory.GoodsIssues}/{{id:guid}}/lines")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> AddLine(Guid id, [FromBody] AddGoodsIssueLineDto dto)
        => HandleResult(await Mediator.Send(new AddGoodsIssueLineCommand(id, dto)));

    [HttpPost($"{ApiRoutes.Inventory.GoodsIssues}/{{id:guid}}/approve")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> Approve(Guid id)
        => HandleResult(await Mediator.Send(new ApproveGoodsIssueCommand(id)));

    [HttpPost($"{ApiRoutes.Inventory.GoodsIssues}/{{id:guid}}/unapprove")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> Unapprove(Guid id)
        => HandleResult(await Mediator.Send(new UnapproveGoodsIssueCommand(id)));

    [HttpPost($"{ApiRoutes.Inventory.GoodsIssues}/{{id:guid}}/post")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> Post(Guid id)
        => HandleResult(await Mediator.Send(new PostGoodsIssueCommand(id)));

    [HttpPost($"{ApiRoutes.Inventory.GoodsIssues}/{{id:guid}}/cancel")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> Cancel(Guid id)
        => HandleResult(await Mediator.Send(new CancelGoodsIssueCommand(id)));

    [HttpPost($"{ApiRoutes.Inventory.GoodsIssues}/{{id:guid}}/confirm")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> Confirm(Guid id)
        => HandleResult(await Mediator.Send(new ConfirmGoodsIssueCommand(id)));
}
