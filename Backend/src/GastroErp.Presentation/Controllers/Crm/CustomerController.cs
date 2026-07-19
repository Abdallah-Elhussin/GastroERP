using Asp.Versioning;
using GastroErp.Application.Features.Crm.Commands;
using GastroErp.Application.Features.Crm.DTOs;
using GastroErp.Application.Features.Crm.Queries;
using GastroErp.Domain.Enums;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Crm;

[ApiVersion("1.0")]
public class CustomerController : BaseApiController
{
    [HttpGet(ApiRoutes.Crm.Customers)]
    [HasPermission(Permissions.Crm.View)]
    public async Task<IActionResult> List(
        [FromQuery] PaginationQuery query,
        [FromQuery] string? search = null)
        => HandlePagedResult(await Mediator.Send(new GetCustomersQuery(query.Page, query.PageSize, search)));

    [HttpGet($"{ApiRoutes.Crm.Customers}/{{id:guid}}")]
    [HasPermission(Permissions.Crm.View)]
    public async Task<IActionResult> GetById(Guid id)
        => Ok(await Mediator.Send(new GetCustomerQuery(id)));

    [HttpPost(ApiRoutes.Crm.Customers)]
    [HasPermission(Permissions.Crm.Create)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerDto dto)
        => Ok(await Mediator.Send(new CreateCustomerCommand(dto)));

    [HttpPut($"{ApiRoutes.Crm.Customers}/{{id:guid}}")]
    [HasPermission(Permissions.Crm.Update)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerDto dto)
    {
        await Mediator.Send(new UpdateCustomerCommand(id, dto));
        return Ok();
    }

    [HttpPost($"{ApiRoutes.Crm.Customers}/{{id:guid}}/status")]
    [HasPermission(Permissions.Crm.Update)]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] CustomerStatus status)
    {
        await Mediator.Send(new ChangeCustomerStatusCommand(id, status));
        return Ok();
    }
}
