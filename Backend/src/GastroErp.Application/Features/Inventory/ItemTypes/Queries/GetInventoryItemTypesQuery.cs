using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Inventory.ItemTypes.Commands;
using GastroErp.Application.Features.Inventory.ItemTypes.Dtos;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Inventory.ItemTypes.Queries;

public sealed record GetInventoryItemTypesQuery(
    Guid TenantId,
    string? SearchTerm = null,
    InventoryItemTypeCategory? Category = null,
    bool? IsActive = null,
    bool? IsInventory = null,
    bool? CanSell = null,
    bool? CanPurchase = null,
    bool? IsRecipe = null,
    bool? IsProduction = null,
    string? SortBy = null,
    bool SortDesc = false,
    int PageNumber = 1,
    int PageSize = 50) : IRequest<PagedResult<InventoryItemTypeDto>>;

public sealed class GetInventoryItemTypesQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetInventoryItemTypesQuery, PagedResult<InventoryItemTypeDto>>
{
    public async Task<PagedResult<InventoryItemTypeDto>> Handle(
        GetInventoryItemTypesQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 200 ? 50 : request.PageSize;

        var query = db.InventoryItemTypes.AsNoTracking()
            .Where(x => x.TenantId == request.TenantId);

        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        if (request.Category.HasValue)
            query = query.Where(x => x.Category == request.Category.Value);
        if (request.IsInventory.HasValue)
            query = query.Where(x => x.IsInventory == request.IsInventory.Value);
        if (request.CanSell.HasValue)
            query = query.Where(x => x.CanSell == request.CanSell.Value);
        if (request.CanPurchase.HasValue)
            query = query.Where(x => x.CanPurchase == request.CanPurchase.Value);
        if (request.IsRecipe.HasValue)
            query = query.Where(x => x.IsRecipe == request.IsRecipe.Value);
        if (request.IsProduction.HasValue)
            query = query.Where(x => x.IsProduction == request.IsProduction.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(x =>
                x.Code.Contains(term)
                || x.NameAr.Contains(term)
                || (x.NameEn != null && x.NameEn.Contains(term)));
        }

        query = ApplySort(query, request.SortBy, request.SortDesc);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(InventoryItemTypeMapper.ToDto).ToList();
        return PagedResult<InventoryItemTypeDto>.Success(dtos, page, pageSize, total);
    }

    private static IQueryable<Domain.Entities.Inventory.Catalog.InventoryItemType> ApplySort(
        IQueryable<Domain.Entities.Inventory.Catalog.InventoryItemType> query,
        string? sortBy,
        bool desc)
    {
        return (sortBy?.Trim().ToLowerInvariant()) switch
        {
            "code" => desc ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code),
            "namear" => desc ? query.OrderByDescending(x => x.NameAr) : query.OrderBy(x => x.NameAr),
            "nameen" => desc ? query.OrderByDescending(x => x.NameEn) : query.OrderBy(x => x.NameEn),
            "category" => desc ? query.OrderByDescending(x => x.Category) : query.OrderBy(x => x.Category),
            "codestart" => desc ? query.OrderByDescending(x => x.CodeStart) : query.OrderBy(x => x.CodeStart),
            "codeend" => desc ? query.OrderByDescending(x => x.CodeEnd) : query.OrderBy(x => x.CodeEnd),
            "isinventory" => desc ? query.OrderByDescending(x => x.IsInventory) : query.OrderBy(x => x.IsInventory),
            "cansell" => desc ? query.OrderByDescending(x => x.CanSell) : query.OrderBy(x => x.CanSell),
            "canpurchase" => desc ? query.OrderByDescending(x => x.CanPurchase) : query.OrderBy(x => x.CanPurchase),
            "isrecipe" => desc ? query.OrderByDescending(x => x.IsRecipe) : query.OrderBy(x => x.IsRecipe),
            "isproduction" => desc ? query.OrderByDescending(x => x.IsProduction) : query.OrderBy(x => x.IsProduction),
            "allownegativestock" => desc ? query.OrderByDescending(x => x.AllowNegativeStock) : query.OrderBy(x => x.AllowNegativeStock),
            "isactive" => desc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
            "sortorder" => desc ? query.OrderByDescending(x => x.SortOrder) : query.OrderBy(x => x.SortOrder),
            _ => query.OrderBy(x => x.SortOrder).ThenBy(x => x.Code)
        };
    }
}

public sealed record GetInventoryItemTypeByIdQuery(Guid Id, Guid TenantId)
    : IRequest<Result<InventoryItemTypeDto>>;

public sealed class GetInventoryItemTypeByIdQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetInventoryItemTypeByIdQuery, Result<InventoryItemTypeDto>>
{
    public async Task<Result<InventoryItemTypeDto>> Handle(
        GetInventoryItemTypeByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await db.InventoryItemTypes.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.TenantId == request.TenantId, cancellationToken);
        if (entity is null)
            return Result<InventoryItemTypeDto>.Failure("ItemTypeNotFound", "Item type not found.");

        return Result<InventoryItemTypeDto>.Success(InventoryItemTypeMapper.ToDto(entity));
    }
}
