using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Sales.ProductPricing.Commands;
using GastroErp.Application.Features.Sales.ProductPricing.Dtos;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Sales.ProductPricing.Queries;

public sealed record GetPriceListsQuery(
    Guid TenantId,
    string? Search = null,
    bool? IsActive = null,
    bool ActiveOnly = false) : IRequest<Result<List<SalesPriceListDto>>>;

public sealed class GetPriceListsQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetPriceListsQuery, Result<List<SalesPriceListDto>>>
{
    public async Task<Result<List<SalesPriceListDto>>> Handle(
        GetPriceListsQuery request,
        CancellationToken cancellationToken)
    {
        var query = db.SalesPriceLists.AsNoTracking()
            .Where(x => x.TenantId == request.TenantId);

        if (request.ActiveOnly || request.IsActive == true)
            query = query.Where(x => x.IsActive);
        else if (request.IsActive == false)
            query = query.Where(x => !x.IsActive);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(x =>
                x.Code.Contains(term)
                || x.NameAr.Contains(term)
                || (x.NameEn != null && x.NameEn.Contains(term)));
        }

        var items = await query
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.NameAr)
            .ToListAsync(cancellationToken);

        return Result<List<SalesPriceListDto>>.Success(
            items.Select(ProductPricingMapper.ToDto).ToList());
    }
}

public sealed record GetProductPricesQuery(
    Guid TenantId,
    string? Search = null,
    Guid? ProductId = null,
    Guid? BranchId = null,
    Guid? PriceListId = null,
    Guid? UnitId = null,
    bool? IsActive = null,
    DateTimeOffset? AsOfDate = null,
    string? SortBy = null,
    bool SortDesc = false,
    int PageNumber = 1,
    int PageSize = 50) : IRequest<PagedResult<ProductPriceDto>>;

public sealed class GetProductPricesQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetProductPricesQuery, PagedResult<ProductPriceDto>>
{
    public async Task<PagedResult<ProductPriceDto>> Handle(
        GetProductPricesQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 200 ? 50 : request.PageSize;

        var query = db.ProductPrices.AsNoTracking()
            .Where(x => x.TenantId == request.TenantId);

        if (request.ProductId.HasValue)
            query = query.Where(x => x.ProductId == request.ProductId.Value);
        if (request.BranchId.HasValue)
            query = query.Where(x => x.BranchId == request.BranchId.Value);
        if (request.PriceListId.HasValue)
            query = query.Where(x => x.PriceListId == request.PriceListId.Value);
        if (request.UnitId.HasValue)
            query = query.Where(x => x.UnitId == request.UnitId.Value);
        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);

        if (request.AsOfDate.HasValue)
        {
            var d = request.AsOfDate.Value;
            query = query.Where(x => x.StartDate <= d && (x.EndDate == null || x.EndDate >= d));
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            var productIds = await db.InventoryItems.AsNoTracking()
                .Where(i => i.TenantId == request.TenantId
                            && (i.NameAr.Contains(term)
                                || (i.NameEn != null && i.NameEn.Contains(term))
                                || (i.Sku != null && i.Sku.Contains(term))))
                .Select(i => i.Id)
                .ToListAsync(cancellationToken);

            query = query.Where(x => productIds.Contains(x.ProductId) || (x.Notes != null && x.Notes.Contains(term)));
        }

        query = (request.SortBy?.Trim().ToLowerInvariant()) switch
        {
            "sellingprice" => request.SortDesc
                ? query.OrderByDescending(x => x.SellingPrice)
                : query.OrderBy(x => x.SellingPrice),
            "cost" => request.SortDesc
                ? query.OrderByDescending(x => x.Cost)
                : query.OrderBy(x => x.Cost),
            "startdate" => request.SortDesc
                ? query.OrderByDescending(x => x.StartDate)
                : query.OrderBy(x => x.StartDate),
            "priority" => request.SortDesc
                ? query.OrderByDescending(x => x.Priority)
                : query.OrderBy(x => x.Priority),
            "isactive" => request.SortDesc
                ? query.OrderByDescending(x => x.IsActive)
                : query.OrderBy(x => x.IsActive),
            _ => request.SortDesc
                ? query.OrderByDescending(x => x.CreatedAt)
                : query.OrderByDescending(x => x.Priority).ThenByDescending(x => x.StartDate)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        var productIdsOnPage = items.Select(x => x.ProductId).Distinct().ToList();
        var branchIds = items.Where(x => x.BranchId.HasValue).Select(x => x.BranchId!.Value).Distinct().ToList();
        var priceListIds = items.Select(x => x.PriceListId).Distinct().ToList();
        var unitIds = items.Select(x => x.UnitId).Distinct().ToList();

        var products = productIdsOnPage.Count == 0
            ? new Dictionary<Guid, (string NameAr, string? Sku)>()
            : await db.InventoryItems.AsNoTracking()
                .Where(i => productIdsOnPage.Contains(i.Id))
                .ToDictionaryAsync(i => i.Id, i => (i.NameAr, i.Sku), cancellationToken);

        var branches = branchIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await db.Branches.AsNoTracking()
                .Where(b => branchIds.Contains(b.Id))
                .ToDictionaryAsync(b => b.Id, b => b.NameAr, cancellationToken);

        var priceLists = priceListIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await db.SalesPriceLists.AsNoTracking()
                .Where(p => priceListIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.NameAr, cancellationToken);

        var units = unitIds.Count == 0
            ? new Dictionary<Guid, (string NameAr, decimal Factor)>()
            : await db.InventoryUnits.AsNoTracking()
                .Where(u => unitIds.Contains(u.Id))
                .ToDictionaryAsync(
                    u => u.Id,
                    u => (NameAr: u.NameAr, Factor: u.ConversionFactor),
                    cancellationToken);

        var dtos = items.Select(e =>
        {
            products.TryGetValue(e.ProductId, out var prod);
            string? branchName = e.BranchId.HasValue && branches.TryGetValue(e.BranchId.Value, out var bn) ? bn : null;
            priceLists.TryGetValue(e.PriceListId, out var plName);
            units.TryGetValue(e.UnitId, out var unit);
            return ProductPricingMapper.ToDto(
                e,
                prod.NameAr,
                prod.Sku,
                branchName,
                plName,
                unit.NameAr,
                unit.Factor == 0 ? 1m : unit.Factor);
        }).ToList();

        return PagedResult<ProductPriceDto>.Success(dtos, page, pageSize, total);
    }
}

public sealed record GetProductPriceByIdQuery(Guid Id, Guid TenantId)
    : IRequest<Result<ProductPriceDto>>;

public sealed class GetProductPriceByIdQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetProductPriceByIdQuery, Result<ProductPriceDto>>
{
    public async Task<Result<ProductPriceDto>> Handle(
        GetProductPriceByIdQuery request,
        CancellationToken cancellationToken)
    {
        var entity = await db.ProductPrices.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.TenantId == request.TenantId, cancellationToken);
        if (entity is null)
            return Result<ProductPriceDto>.Failure("ProductPriceNotFound", "Product price not found.");

        var product = await db.InventoryItems.AsNoTracking()
            .Where(i => i.Id == entity.ProductId)
            .Select(i => new { i.NameAr, i.Sku })
            .FirstOrDefaultAsync(cancellationToken);

        string? branchName = null;
        if (entity.BranchId.HasValue)
        {
            branchName = await db.Branches.AsNoTracking()
                .Where(b => b.Id == entity.BranchId.Value)
                .Select(b => b.NameAr)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var priceListName = await db.SalesPriceLists.AsNoTracking()
            .Where(p => p.Id == entity.PriceListId)
            .Select(p => p.NameAr)
            .FirstOrDefaultAsync(cancellationToken);

        var unit = await db.InventoryUnits.AsNoTracking()
            .Where(u => u.Id == entity.UnitId)
            .Select(u => new { u.NameAr, u.ConversionFactor })
            .FirstOrDefaultAsync(cancellationToken);

        return Result<ProductPriceDto>.Success(ProductPricingMapper.ToDto(
            entity,
            product?.NameAr,
            product?.Sku,
            branchName,
            priceListName,
            unit?.NameAr,
            unit?.ConversionFactor ?? 1m));
    }
}

public sealed record GetProductUnitsForPricingQuery(Guid TenantId, Guid ProductId, ProductCostType CostType = ProductCostType.Average)
    : IRequest<Result<List<ProductUnitPricingRowDto>>>;

public sealed class GetProductUnitsForPricingQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetProductUnitsForPricingQuery, Result<List<ProductUnitPricingRowDto>>>
{
    public async Task<Result<List<ProductUnitPricingRowDto>>> Handle(
        GetProductUnitsForPricingQuery request,
        CancellationToken cancellationToken)
    {
        var item = await db.InventoryItems.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.ProductId && x.TenantId == request.TenantId, cancellationToken);
        if (item is null)
            return Result<List<ProductUnitPricingRowDto>>.Failure("ProductNotFound", "Product not found.");

        var baseCost = await ProductPricingRules.ResolveReferenceCostAsync(
            db, request.TenantId, request.ProductId, request.CostType, cancellationToken);

        var unitIds = new HashSet<Guid> { item.BaseUnitId };
        if (item.DefaultPurchaseUnitId.HasValue) unitIds.Add(item.DefaultPurchaseUnitId.Value);
        if (item.DefaultRecipeUnitId.HasValue) unitIds.Add(item.DefaultRecipeUnitId.Value);

        var conversions = await db.UnitConversions.AsNoTracking()
            .Where(c => c.TenantId == request.TenantId
                        && (c.FromUnitId == item.BaseUnitId || c.ToUnitId == item.BaseUnitId))
            .ToListAsync(cancellationToken);

        foreach (var c in conversions)
        {
            unitIds.Add(c.FromUnitId);
            unitIds.Add(c.ToUnitId);
        }

        var units = await db.InventoryUnits.AsNoTracking()
            .Where(u => u.TenantId == request.TenantId && unitIds.Contains(u.Id))
            .ToListAsync(cancellationToken);

        var rows = new List<ProductUnitPricingRowDto>();
        foreach (var unit in units.OrderBy(u => u.NameAr))
        {
            decimal factor = 1m;
            if (unit.Id == item.BaseUnitId)
            {
                factor = 1m;
            }
            else
            {
                var fromBase = conversions.FirstOrDefault(c => c.FromUnitId == item.BaseUnitId && c.ToUnitId == unit.Id);
                var toBase = conversions.FirstOrDefault(c => c.FromUnitId == unit.Id && c.ToUnitId == item.BaseUnitId);
                if (fromBase is not null)
                    factor = fromBase.ConversionFactor;
                else if (toBase is not null)
                    factor = toBase.ConversionFactor;
                else if (unit.ConversionFactor > 0)
                    factor = unit.ConversionFactor;
            }

            var unitCost = decimal.Round(baseCost * factor, 4, MidpointRounding.AwayFromZero);
            rows.Add(new ProductUnitPricingRowDto(
                unit.Id,
                unit.NameAr,
                unit.NameEn,
                factor,
                unitCost,
                unit.Id == item.BaseUnitId));
        }

        return Result<List<ProductUnitPricingRowDto>>.Success(rows);
    }
}
