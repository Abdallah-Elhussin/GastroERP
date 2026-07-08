using Asp.Versioning;
using GastroErp.Application.Features.Menu.Commands;
using GastroErp.Application.Features.Menu.DTOs;
using GastroErp.Application.Features.Menu.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Menu;

/// <summary>
/// Modifier management
/// </summary>
[ApiVersion("1.0")]
public class ModifierController : BaseApiController
{
    [HttpGet(ApiRoutes.Menu.Modifiers)]
    [HasPermission(Permissions.Modifier.View)]
    public async Task<IActionResult> GetModifiers([FromQuery] PaginationQuery query)
    {
        return HandlePagedResult(await Mediator.Send(new GetModifiersQuery(TenantId, query.Page, query.PageSize)));
    }

    [HttpGet($"{ApiRoutes.Menu.Modifiers}/{{id:guid}}")]
    [HasPermission(Permissions.Modifier.View)]
    public async Task<IActionResult> GetModifierById(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetModifierByIdQuery(id)));
    }

    [HttpPost(ApiRoutes.Menu.Modifiers)]
    [HasPermission(Permissions.Modifier.Create)]
    public async Task<IActionResult> CreateModifier([FromBody] CreateModifierDto dto)
    {
        return HandleResult(await Mediator.Send(new CreateModifierCommand(TenantId, dto)));
    }

    [HttpPut($"{ApiRoutes.Menu.Modifiers}/{{id:guid}}")]
    [HasPermission(Permissions.Modifier.Update)]
    public async Task<IActionResult> UpdateModifier(Guid id, [FromBody] UpdateModifierDto dto)
    {
        return HandleResult(await Mediator.Send(new UpdateModifierCommand(id, dto)));
    }
}
