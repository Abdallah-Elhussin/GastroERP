using Asp.Versioning;
using GastroErp.Application.Features.Organization.Commands;
using GastroErp.Application.Features.Organization.DTOs;
using GastroErp.Application.Features.Organization.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Organization;

/// <summary>
/// Department management within a branch
/// </summary>
[ApiVersion("1.0")]
public class DepartmentController : BaseApiController
{
    /// <summary>
    /// Get all departments
    /// </summary>
    [HttpGet(ApiRoutes.Organization.Departments)]
    [HasPermission(Permissions.Department.View)]
    public async Task<IActionResult> GetDepartments([FromQuery] Guid branchId, [FromQuery] PaginationQuery query)
    {
        return HandlePagedResult(await Mediator.Send(new GetDepartmentsQuery(TenantId, null, branchId, query.Page, query.PageSize))); // Update later if paginated
    }

    /// <summary>
    /// Get a department by its ID
    /// </summary>
    [HttpGet($"{ApiRoutes.Organization.Departments}/{{id:guid}}")]
    [HasPermission(Permissions.Department.View)]
    public async Task<IActionResult> GetDepartmentById(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetDepartmentByIdQuery(id)));
    }

    /// <summary>
    /// Create a new department
    /// </summary>
    [HttpPost(ApiRoutes.Organization.Departments)]
    [HasPermission(Permissions.Department.Create)]
    public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentDto dto)
    {
        return HandleResult(await Mediator.Send(new CreateDepartmentCommand(dto)));
    }

    /// <summary>
    /// Update an existing department
    /// </summary>
    [HttpPut($"{ApiRoutes.Organization.Departments}/{{id:guid}}")]
    [HasPermission(Permissions.Department.Update)]
    public async Task<IActionResult> UpdateDepartment(Guid id, [FromBody] UpdateDepartmentDto dto)
    {
        return HandleResult(await Mediator.Send(new UpdateDepartmentCommand(id, dto)));
    }
}
