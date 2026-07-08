using Asp.Versioning;
using GastroErp.Application.Features.Menu.Commands;
using GastroErp.Application.Features.Menu.DTOs;
using GastroErp.Application.Features.Menu.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Menu;

/// <summary>
/// Combo management
/// </summary>
[ApiVersion("1.0")]
public class ComboController : BaseApiController
{
    [HttpGet(ApiRoutes.Menu.Combos)]
    [HasPermission(Permissions.Combo.View)]
    public async Task<IActionResult> GetCombos([FromQuery] PaginationQuery query)
    {
        return HandlePagedResult(await Mediator.Send(new GetCombosQuery(TenantId, query.Page, query.PageSize)));
    }

    [HttpGet($"{ApiRoutes.Menu.Combos}/{{id:guid}}")]
    [HasPermission(Permissions.Combo.View)]
    public async Task<IActionResult> GetComboById(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetComboByIdQuery(id)));
    }

    [HttpPost(ApiRoutes.Menu.Combos)]
    [HasPermission(Permissions.Combo.Create)]
    public async Task<IActionResult> CreateCombo([FromBody] CreateComboDto dto)
    {
        return HandleResult(await Mediator.Send(new CreateComboCommand(TenantId, dto)));
    }

    [HttpPut($"{ApiRoutes.Menu.Combos}/{{id:guid}}")]
    [HasPermission(Permissions.Combo.Update)]
    public async Task<IActionResult> UpdateCombo(Guid id, [FromBody] UpdateComboDto dto)
    {
        return HandleResult(await Mediator.Send(new UpdateComboCommand(id, dto)));
    }

    [HttpPut($"{ApiRoutes.Menu.Combos}/{{id:guid}}/activate")]
    [HasPermission(Permissions.Combo.Update)]
    public async Task<IActionResult> Activate(Guid id)
    {
        return HandleResult(await Mediator.Send(new ActivateComboCommand(id)));
    }

    [HttpPut($"{ApiRoutes.Menu.Combos}/{{id:guid}}/deactivate")]
    [HasPermission(Permissions.Combo.Update)]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        return HandleResult(await Mediator.Send(new DeactivateComboCommand(id)));
    }

    [HttpPost($"{ApiRoutes.Menu.Combos}/{{id:guid}}/items")]
    [HasPermission(Permissions.Combo.Update)]
    public async Task<IActionResult> AddComboItem(Guid id, [FromBody] AddComboItemDto dto)
    {
        return HandleResult(await Mediator.Send(new AddComboItemCommand(id, dto)));
    }

    [HttpDelete($"{ApiRoutes.Menu.Combos}/{{id:guid}}/items/{{productId:guid}}")]
    [HasPermission(Permissions.Combo.Update)]
    public async Task<IActionResult> RemoveComboItem(Guid id, Guid productId)
    {
        return HandleResult(await Mediator.Send(new RemoveComboItemCommand(id, productId)));
    }

    [HttpPut($"{ApiRoutes.Menu.Combos}/{{id:guid}}/items/{{productId:guid}}")]
    [HasPermission(Permissions.Combo.Update)]
    public async Task<IActionResult> UpdateComboItem(Guid id, Guid productId, [FromBody] UpdateComboItemDto dto)
    {
        return HandleResult(await Mediator.Send(new UpdateComboItemCommand(id, productId, dto)));
    }
}
