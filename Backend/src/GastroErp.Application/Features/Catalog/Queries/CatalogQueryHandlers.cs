using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Catalog.Commands;
using GastroErp.Application.Features.Catalog.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Catalog.Queries;

public class GetProductCatalogTypesQueryHandler : IRequestHandler<GetProductCatalogTypesQuery, Result<List<ProductCatalogTypeDto>>>
{
    public Task<Result<List<ProductCatalogTypeDto>>> Handle(GetProductCatalogTypesQuery request, CancellationToken cancellationToken) =>
        Task.FromResult(Result<List<ProductCatalogTypeDto>>.Success(ProductCatalogTypeRegistry.All().ToList()));
}

public class GetProductCatalogDefinitionsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetProductCatalogDefinitionsQuery, PagedResult<ProductCatalogDefinitionDto>>
{
    public async Task<PagedResult<ProductCatalogDefinitionDto>> Handle(GetProductCatalogDefinitionsQuery request, CancellationToken cancellationToken)
    {
        var query = context.ProductCatalogDefinitions.AsNoTracking().Where(c => c.TenantId == request.TenantId);
        if (request.CatalogType.HasValue) query = query.Where(c => c.CatalogType == request.CatalogType.Value);
        if (request.Status.HasValue) query = query.Where(c => c.Status == request.Status.Value);
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(c =>
                c.NameAr.Contains(request.SearchTerm) ||
                (c.NameEn != null && c.NameEn.Contains(request.SearchTerm)) ||
                c.Code.Contains(request.SearchTerm) ||
                (c.Sku != null && c.Sku.Contains(request.SearchTerm)) ||
                (c.Barcode != null && c.Barcode.Contains(request.SearchTerm)));

        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(c => c.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(CreateCatalogDraftCommandHandler.Map).ToList();
        return PagedResult<ProductCatalogDefinitionDto>.Success(dtos, total, request.PageNumber, request.PageSize);
    }
}

public class GetProductCatalogDefinitionByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetProductCatalogDefinitionByIdQuery, Result<ProductCatalogDefinitionDto>>
{
    public async Task<Result<ProductCatalogDefinitionDto>> Handle(GetProductCatalogDefinitionByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await context.ProductCatalogDefinitions.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (entity is null) return Result<ProductCatalogDefinitionDto>.Failure("CatalogNotFound", "Product catalog definition not found.");
        return Result<ProductCatalogDefinitionDto>.Success(await CatalogDefinitionMapper.MapAsync(context, entity, cancellationToken));
    }
}

public class ExportCatalogDefinitionsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<ExportCatalogDefinitionsQuery, Result<byte[]>>
{
    public async Task<Result<byte[]>> Handle(ExportCatalogDefinitionsQuery request, CancellationToken cancellationToken)
    {
        var query = context.ProductCatalogDefinitions.AsNoTracking().Where(c => c.TenantId == request.TenantId);
        if (request.CatalogType.HasValue) query = query.Where(c => c.CatalogType == request.CatalogType.Value);
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(c => c.NameAr.Contains(request.SearchTerm) || c.Code.Contains(request.SearchTerm));

        var items = await query.OrderBy(c => c.Code).ToListAsync(cancellationToken);
        var lines = new List<string> { "Code,CatalogType,NameAr,NameEn,Sku,Barcode,BasePrice,Status,InventoryItemId,ProductId,RecipeId" };
        lines.AddRange(items.Select(i =>
            $"{Escape(i.Code)},{(int)i.CatalogType},{Escape(i.NameAr)},{Escape(i.NameEn)},{Escape(i.Sku)},{Escape(i.Barcode)},{i.BasePrice},{(int)i.Status},{i.InventoryItemId},{i.ProductId},{i.RecipeId}"));
        return Result<byte[]>.Success(System.Text.Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, lines)));
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Contains(',') ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
    }
}

public class GetCatalogAuditTimelineQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetCatalogAuditTimelineQuery, Result<List<CatalogAuditEntryDto>>>
{
    public async Task<Result<List<CatalogAuditEntryDto>>> Handle(GetCatalogAuditTimelineQuery request, CancellationToken cancellationToken)
    {
        var catalog = await context.ProductCatalogDefinitions.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (catalog is null) return Result<List<CatalogAuditEntryDto>>.Failure("CatalogNotFound", "Product catalog definition not found.");

        var entries = new List<CatalogAuditEntryDto>
        {
            new("Created", $"Catalog draft {catalog.Code} created", catalog.CreatedAt.UtcDateTime, catalog.CreatedBy)
        };

        if (catalog.UpdatedAt.HasValue)
            entries.Add(new("Updated", "Catalog definition updated", catalog.UpdatedAt.Value.UtcDateTime, catalog.UpdatedBy));

        var priceHistory = await context.ProductPriceHistories.AsNoTracking()
            .Where(h => h.CatalogDefinitionId == catalog.Id)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync(cancellationToken);

        entries.AddRange(priceHistory.Select(h => new CatalogAuditEntryDto(
            "PriceChanged",
            $"Price changed from {h.OldPrice} to {h.NewPrice} {h.Currency}" + (h.PriceLevelName != null ? $" ({h.PriceLevelName})" : string.Empty),
            h.CreatedAt.UtcDateTime,
            h.CreatedBy)));

        return Result<List<CatalogAuditEntryDto>>.Success(entries.OrderByDescending(e => e.OccurredAt).ToList());
    }
}

public class GetCatalogPriceHistoryQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetCatalogPriceHistoryQuery, Result<List<CatalogAuditEntryDto>>>
{
    public async Task<Result<List<CatalogAuditEntryDto>>> Handle(GetCatalogPriceHistoryQuery request, CancellationToken cancellationToken)
    {
        var history = await context.ProductPriceHistories.AsNoTracking()
            .Where(h => h.CatalogDefinitionId == request.Id)
            .OrderByDescending(h => h.CreatedAt)
            .Select(h => new CatalogAuditEntryDto(
                "PriceChanged",
                $"Price changed from {h.OldPrice} to {h.NewPrice} {h.Currency}",
                h.CreatedAt.UtcDateTime,
                h.CreatedBy))
            .ToListAsync(cancellationToken);
        return Result<List<CatalogAuditEntryDto>>.Success(history);
    }
}
