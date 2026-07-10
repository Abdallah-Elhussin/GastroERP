using Asp.Versioning;
using GastroErp.Application.Features.Catalog.Commands;
using GastroErp.Application.Features.Catalog.DTOs;
using GastroErp.Application.Features.Catalog.Queries;
using GastroErp.Presentation.Authorization;
using GastroErp.Presentation.Common;
using Microsoft.AspNetCore.Mvc;

namespace GastroErp.Presentation.Controllers.Catalog;

/// <summary>
/// Enterprise Product Catalog Engine
/// </summary>
[ApiVersion("1.0")]
public class CatalogController : BaseApiController
{
    [HttpGet(ApiRoutes.Catalog.Types)]
    [HasPermission(Permissions.Catalog.View)]
    public async Task<IActionResult> GetTypes()
        => HandleResult(await Mediator.Send(new GetProductCatalogTypesQuery()));

    [HttpGet(ApiRoutes.Catalog.Definitions)]
    [HasPermission(Permissions.Catalog.View)]
    public async Task<IActionResult> GetDefinitions([FromQuery] PaginationQuery query, [FromQuery] int? catalogType, [FromQuery] int? status)
    {
        var typeFilter = catalogType.HasValue ? (Domain.Enums.ProductCatalogType)catalogType.Value : (Domain.Enums.ProductCatalogType?)null;
        var statusFilter = status.HasValue ? (Domain.Enums.ProductCatalogStatus)status.Value : (Domain.Enums.ProductCatalogStatus?)null;
        return HandlePagedResult(await Mediator.Send(new GetProductCatalogDefinitionsQuery(
            TenantId, typeFilter, statusFilter, query.Search, query.Page, query.PageSize)));
    }

    [HttpGet($"{ApiRoutes.Catalog.Definitions}/export")]
    [HasPermission(Permissions.Catalog.Export)]
    public async Task<IActionResult> Export([FromQuery] int? catalogType, [FromQuery] string? search)
    {
        var result = await Mediator.Send(new ExportCatalogDefinitionsQuery(
            TenantId,
            catalogType.HasValue ? (Domain.Enums.ProductCatalogType)catalogType.Value : null,
            search));
        if (!result.IsSuccess) return HandleResult(result);
        return File(result.Data!, "text/csv", $"catalog-export-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }

    [HttpGet($"{ApiRoutes.Catalog.Definitions}/{{id:guid}}")]
    [HasPermission(Permissions.Catalog.View)]
    public async Task<IActionResult> GetDefinitionById(Guid id)
        => HandleResult(await Mediator.Send(new GetProductCatalogDefinitionByIdQuery(id)));

    [HttpGet($"{ApiRoutes.Catalog.Definitions}/{{id:guid}}/audit")]
    [HasPermission(Permissions.Catalog.View)]
    public async Task<IActionResult> GetAuditTimeline(Guid id)
        => HandleResult(await Mediator.Send(new GetCatalogAuditTimelineQuery(id)));

    [HttpGet($"{ApiRoutes.Catalog.Definitions}/{{id:guid}}/price-history")]
    [HasPermission(Permissions.Catalog.View)]
    public async Task<IActionResult> GetPriceHistory(Guid id)
        => HandleResult(await Mediator.Send(new GetCatalogPriceHistoryQuery(id)));

    [HttpPost(ApiRoutes.Catalog.Definitions)]
    [HasPermission(Permissions.Catalog.Create)]
    public async Task<IActionResult> CreateDraft([FromBody] CreateCatalogDraftDto dto)
        => HandleResult(await Mediator.Send(new CreateCatalogDraftCommand(TenantId, dto)));

    [HttpPost($"{ApiRoutes.Catalog.Definitions}/import")]
    [HasPermission(Permissions.Catalog.Import)]
    public async Task<IActionResult> Import([FromBody] List<CatalogImportRowDto> rows)
        => HandleResult(await Mediator.Send(new ImportCatalogDefinitionsCommand(TenantId, rows)));

    [HttpPut($"{ApiRoutes.Catalog.Definitions}/{{id:guid}}/general")]
    [HasPermission(Permissions.Catalog.Update)]
    public async Task<IActionResult> UpdateGeneralInfo(Guid id, [FromBody] UpdateCatalogGeneralInfoDto dto)
        => HandleResult(await Mediator.Send(new UpdateCatalogGeneralInfoCommand(id, dto)));

    [HttpPut($"{ApiRoutes.Catalog.Definitions}/{{id:guid}}/inventory")]
    [HasPermission(Permissions.Catalog.Update)]
    public async Task<IActionResult> SaveInventory(Guid id, [FromBody] SaveCatalogInventoryDto dto)
        => HandleResult(await Mediator.Send(new SaveCatalogInventoryCommand(id, dto)));

    [HttpPut($"{ApiRoutes.Catalog.Definitions}/{{id:guid}}/recipe")]
    [HasPermission(Permissions.Catalog.Update)]
    public async Task<IActionResult> SaveRecipe(Guid id, [FromBody] SaveCatalogRecipeDto dto)
        => HandleResult(await Mediator.Send(new SaveCatalogRecipeCommand(id, dto)));

    [HttpPut($"{ApiRoutes.Catalog.Definitions}/{{id:guid}}/pos")]
    [HasPermission(Permissions.Catalog.Update)]
    public async Task<IActionResult> SavePos(Guid id, [FromBody] SaveCatalogPosDto dto)
        => HandleResult(await Mediator.Send(new SaveCatalogPosCommand(id, dto)));

    [HttpPut($"{ApiRoutes.Catalog.Definitions}/{{id:guid}}/pricing")]
    [HasPermission(Permissions.Catalog.Update)]
    public async Task<IActionResult> SavePricing(Guid id, [FromBody] SaveCatalogPricingDto dto)
        => HandleResult(await Mediator.Send(new SaveCatalogPricingCommand(id, dto)));

    [HttpPut($"{ApiRoutes.Catalog.Definitions}/{{id:guid}}/extensions")]
    [HasPermission(Permissions.Catalog.Update)]
    public async Task<IActionResult> SaveExtensions(Guid id, [FromBody] SaveCatalogExtensionsDto dto)
        => HandleResult(await Mediator.Send(new SaveCatalogExtensionsCommand(id, dto)));

    [HttpPut($"{ApiRoutes.Catalog.Definitions}/{{id:guid}}/relationships")]
    [HasPermission(Permissions.Catalog.Update)]
    public async Task<IActionResult> SaveRelationships(Guid id, [FromBody] SaveCatalogRelationshipsDto dto)
        => HandleResult(await Mediator.Send(new SaveCatalogRelationshipsCommand(id, dto)));

    [HttpPost($"{ApiRoutes.Catalog.Definitions}/{{id:guid}}/activate")]
    [HasPermission(Permissions.Catalog.Approve)]
    public async Task<IActionResult> Activate(Guid id)
        => HandleResult(await Mediator.Send(new ActivateCatalogDefinitionCommand(id)));
}
