using Asp.Versioning;
using GastroErp.Application.Features.Inventory.Commands;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Application.Features.Inventory.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Inventory;

/// <summary>
/// Opening Balance — Draft → Approve → Post (stock via InventoryMovementPipeline).
/// </summary>
[ApiVersion("1.0")]
public class OpeningBalanceController : BaseApiController
{
    [HttpGet(ApiRoutes.Inventory.OpeningBalances)]
    [HasPermission(Permissions.Inventory.View)]
    public async Task<IActionResult> GetOpeningBalances([FromQuery] PaginationQuery query)
        => HandlePagedResult(await Mediator.Send(new GetOpeningBalancesQuery(TenantId, query.Page, query.PageSize)));

    [HttpGet($"{ApiRoutes.Inventory.OpeningBalances}/{{id:guid}}")]
    [HasPermission(Permissions.Inventory.View)]
    public async Task<IActionResult> GetById(Guid id)
        => HandleResult(await Mediator.Send(new GetOpeningBalanceByIdQuery(id)));

    [HttpPost($"{ApiRoutes.Inventory.OpeningBalances}/next-number")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> NextNumber()
        => HandleResult(await Mediator.Send(new GenerateOpeningBalanceNumberCommand(TenantId)));

    [HttpPost(ApiRoutes.Inventory.OpeningBalances)]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> Create([FromBody] CreateOpeningBalanceDto dto)
        => HandleResult(await Mediator.Send(new CreateOpeningBalanceCommand(dto with { TenantId = TenantId })));

    [HttpPut($"{ApiRoutes.Inventory.OpeningBalances}/{{id:guid}}")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOpeningBalanceDto dto)
        => HandleResult(await Mediator.Send(new UpdateOpeningBalanceCommand(id, dto with { TenantId = TenantId })));

    [HttpPost($"{ApiRoutes.Inventory.OpeningBalances}/{{id:guid}}/lines")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> AddLine(Guid id, [FromBody] AddOpeningBalanceLineDto dto)
        => HandleResult(await Mediator.Send(new AddOpeningBalanceLineCommand(id, dto)));

    [HttpPost($"{ApiRoutes.Inventory.OpeningBalances}/{{id:guid}}/approve")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> Approve(Guid id)
        => HandleResult(await Mediator.Send(new ApproveOpeningBalanceCommand(id)));

    [HttpPost($"{ApiRoutes.Inventory.OpeningBalances}/{{id:guid}}/unapprove")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> Unapprove(Guid id)
        => HandleResult(await Mediator.Send(new UnapproveOpeningBalanceCommand(id)));

    [HttpPost($"{ApiRoutes.Inventory.OpeningBalances}/{{id:guid}}/post")]
    [HasPermission(Permissions.Inventory.Manage)]
    public async Task<IActionResult> Post(Guid id)
        => HandleResult(await Mediator.Send(new PostOpeningBalanceCommand(id)));

    [HttpGet($"{ApiRoutes.Inventory.OpeningBalances}/excel-template")]
    [HasPermission(Permissions.Inventory.View)]
    public IActionResult DownloadTemplate()
    {
        const string csv =
            "ItemSku,WarehouseCode,UnitSymbol,Quantity,UnitCost,BatchNumber,ExpiryDate,SerialNumber\r\n" +
            "ITEM-001,WH-MAIN,PCS,10,5.00,,,\r\n";
        var bytes = System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(csv)).ToArray();
        return File(bytes, "text/csv", "opening-balance-template.csv");
    }
}
