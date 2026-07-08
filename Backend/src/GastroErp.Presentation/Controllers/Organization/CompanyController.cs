using Asp.Versioning;
using GastroErp.Application.Features.Organization.Commands;
using GastroErp.Application.Features.Organization.DTOs;
using GastroErp.Application.Features.Organization.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Organization;

/// <summary>
/// Company management within a tenant
/// </summary>
[ApiVersion("1.0")]
public class CompanyController : BaseApiController
{
    /// <summary>
    /// Get all companies for the current tenant
    /// </summary>
    [HttpGet(ApiRoutes.Organization.Companies)]
    [HasPermission(Permissions.Company.View)]
    public async Task<IActionResult> GetCompanies([FromQuery] PaginationQuery query)
    {
        return HandlePagedResult(await Mediator.Send(new GetCompaniesQuery(TenantId, query.Page, query.PageSize, query.Search))); // Update later if paginated
    }

    /// <summary>
    /// Get a company by its ID
    /// </summary>
    [HttpGet($"{ApiRoutes.Organization.Companies}/{{id:guid}}")]
    [HasPermission(Permissions.Company.View)]
    public async Task<IActionResult> GetCompanyById(Guid id)
    {
        return HandleResult(await Mediator.Send(new GetCompanyByIdQuery(id)));
    }

    /// <summary>
    /// Create a new company
    /// </summary>
    [HttpPost(ApiRoutes.Organization.Companies)]
    [HasPermission(Permissions.Company.Create)]
    public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyDto dto)
    {
        return HandleResult(await Mediator.Send(new CreateCompanyCommand(dto)));
    }

    /// <summary>
    /// Update an existing company
    /// </summary>
    [HttpPut($"{ApiRoutes.Organization.Companies}/{{id:guid}}")]
    [HasPermission(Permissions.Company.Update)]
    public async Task<IActionResult> UpdateCompany(Guid id, [FromBody] UpdateCompanyDto dto)
    {
        return HandleResult(await Mediator.Send(new UpdateCompanyCommand(id, dto)));
    }
}
