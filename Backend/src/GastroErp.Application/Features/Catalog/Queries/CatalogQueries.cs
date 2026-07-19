using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Catalog.DTOs;
using GastroErp.Domain.Enums;
using MediatR;

namespace GastroErp.Application.Features.Catalog.Queries;

public record GetProductCatalogTypesQuery : IRequest<Result<List<ProductCatalogTypeDto>>>;

public record GetProductCatalogDefinitionsQuery(
    Guid TenantId,
    ProductCatalogType? CatalogType = null,
    ProductCatalogStatus? Status = null,
    string? SearchTerm = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<ProductCatalogDefinitionDto>>;

public record GetProductCatalogDefinitionByIdQuery(Guid Id) : IRequest<Result<ProductCatalogDefinitionDto>>;

public record GetCatalogDefinitionByInventoryItemIdQuery(Guid InventoryItemId) : IRequest<Result<ProductCatalogDefinitionDto>>;

public record ExportCatalogDefinitionsQuery(Guid TenantId, ProductCatalogType? CatalogType = null, string? SearchTerm = null)
    : IRequest<Result<byte[]> >;

public record GetCatalogAuditTimelineQuery(Guid Id) : IRequest<Result<List<CatalogAuditEntryDto>> >;

public record GetCatalogPriceHistoryQuery(Guid Id) : IRequest<Result<List<CatalogPriceHistoryDto>> >;
