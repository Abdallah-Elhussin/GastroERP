using AutoMapper;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Inventory.Commands;
using GastroErp.Application.Features.Inventory.DTOs;
using GastroErp.Application.Features.Inventory.Mapping;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Inventory.Queries;

// ─── InventoryCategory Handlers ───────────────────────────────────────────────

public class GetInventoryCategoriesQueryHandler : IRequestHandler<GetInventoryCategoriesQuery, PagedResult<InventoryCategoryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetInventoryCategoriesQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<PagedResult<InventoryCategoryDto>> Handle(GetInventoryCategoriesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.InventoryCategories.AsNoTracking().Where(c => c.TenantId == request.TenantId);
        if (request.IsActive.HasValue) query = query.Where(c => c.IsActive == request.IsActive.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(c => c.NameAr)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<InventoryCategoryDto>.Success(
            _mapper.Map<List<InventoryCategoryDto>>(items),
            total,
            request.PageNumber,
            request.PageSize);
    }
}

public class GetInventoryCategoryByIdQueryHandler : IRequestHandler<GetInventoryCategoryByIdQuery, Result<InventoryCategoryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetInventoryCategoryByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<InventoryCategoryDto>> Handle(GetInventoryCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var cat = await _context.InventoryCategories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (cat == null) return Result<InventoryCategoryDto>.Failure("CategoryNotFound", "Inventory category not found.");
        return Result<InventoryCategoryDto>.Success(_mapper.Map<InventoryCategoryDto>(cat));
    }
}

// ─── InventoryUnit Handlers ───────────────────────────────────────────────────

public class GetInventoryUnitsQueryHandler : IRequestHandler<GetInventoryUnitsQuery, Result<List<InventoryUnitDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetInventoryUnitsQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<List<InventoryUnitDto>>> Handle(GetInventoryUnitsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.InventoryUnits.AsNoTracking().Where(u => u.TenantId == request.TenantId);
        if (request.IsActive.HasValue) query = query.Where(u => u.IsActive == request.IsActive.Value);
        var items = await query.OrderBy(u => u.SortOrder).ThenBy(u => u.NameAr).ToListAsync(cancellationToken);
        return Result<List<InventoryUnitDto>>.Success(_mapper.Map<List<InventoryUnitDto>>(items));
    }
}

// ─── InventoryItem Handlers ───────────────────────────────────────────────────

public class GetInventoryItemsQueryHandler : IRequestHandler<GetInventoryItemsQuery, PagedResult<InventoryItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetInventoryItemsQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<PagedResult<InventoryItemDto>> Handle(GetInventoryItemsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.InventoryItems.AsNoTracking().Where(i => i.TenantId == request.TenantId);
        if (request.CategoryId.HasValue) query = query.Where(i => i.CategoryId == request.CategoryId.Value);
        if (request.IsActive.HasValue) query = query.Where(i => i.IsActive == request.IsActive.Value);
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(i => i.NameAr.Contains(request.SearchTerm) || (i.NameEn != null && i.NameEn.Contains(request.SearchTerm)));

        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(i => i.NameAr).Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToListAsync(cancellationToken);
        var projected = await InventoryItemDtoProjector.ProjectAsync(_context, items, cancellationToken);

        return PagedResult<InventoryItemDto>.Success(projected, total, request.PageNumber, request.PageSize);
    }
}

public class GetInventoryItemByIdQueryHandler : IRequestHandler<GetInventoryItemByIdQuery, Result<InventoryItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetInventoryItemByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<InventoryItemDto>> Handle(GetInventoryItemByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await _context.InventoryItems.AsNoTracking().FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);
        if (item == null) return Result<InventoryItemDto>.Failure("ItemNotFound", "Inventory item not found.");
        var projected = await InventoryItemDtoProjector.ProjectAsync(_context, [item], cancellationToken);
        return Result<InventoryItemDto>.Success(projected[0]);
    }
}

// ─── Warehouse Handlers ───────────────────────────────────────────────────────

public class GetWarehousesQueryHandler : IRequestHandler<GetWarehousesQuery, PagedResult<WarehouseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetWarehousesQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<PagedResult<WarehouseDto>> Handle(GetWarehousesQuery request, CancellationToken cancellationToken)
    {
        var page = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 200 ? 50 : request.PageSize;

        var query = _context.Warehouses.AsNoTracking().Include(w => w.Zones).Where(w => w.TenantId == request.TenantId);
        if (request.BranchId.HasValue) query = query.Where(w => w.BranchId == request.BranchId.Value);
        if (request.IsActive.HasValue) query = query.Where(w => w.IsActive == request.IsActive.Value);
        if (request.WarehouseType.HasValue) query = query.Where(w => w.WarehouseType == request.WarehouseType.Value);
        if (request.WarehouseTypeId.HasValue) query = query.Where(w => w.WarehouseTypeId == request.WarehouseTypeId.Value);
        if (request.IsPosWarehouse.HasValue) query = query.Where(w => w.IsPosWarehouse == request.IsPosWarehouse.Value);
        if (request.IsDefault.HasValue) query = query.Where(w => w.IsDefault == request.IsDefault.Value);
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(w =>
                (w.Code != null && w.Code.Contains(term))
                || w.NameAr.Contains(term)
                || (w.NameEn != null && w.NameEn.Contains(term)));
        }

        query = (request.SortBy?.Trim().ToLowerInvariant()) switch
        {
            "code" => request.SortDesc ? query.OrderByDescending(w => w.Code) : query.OrderBy(w => w.Code),
            "nameen" => request.SortDesc ? query.OrderByDescending(w => w.NameEn) : query.OrderBy(w => w.NameEn),
            "warehousetype" => request.SortDesc ? query.OrderByDescending(w => w.WarehouseType) : query.OrderBy(w => w.WarehouseType),
            "isactive" => request.SortDesc ? query.OrderByDescending(w => w.IsActive) : query.OrderBy(w => w.IsActive),
            "isdefault" => request.SortDesc ? query.OrderByDescending(w => w.IsDefault) : query.OrderBy(w => w.IsDefault),
            _ => request.SortDesc ? query.OrderByDescending(w => w.NameAr) : query.OrderBy(w => w.NameAr)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        var dtos = await EnrichWarehouseDtosAsync(_context, _mapper.Map<List<WarehouseDto>>(items), cancellationToken);
        return PagedResult<WarehouseDto>.Success(dtos, page, pageSize, total);
    }

    internal static async Task<List<WarehouseDto>> EnrichWarehouseDtosAsync(
        IApplicationDbContext context,
        List<WarehouseDto> dtos,
        CancellationToken cancellationToken)
    {
        if (dtos.Count == 0) return dtos;

        var typeIds = dtos.Where(d => d.WarehouseTypeId.HasValue).Select(d => d.WarehouseTypeId!.Value).Distinct().ToList();
        var parentIds = dtos.Where(d => d.ParentWarehouseId.HasValue).Select(d => d.ParentWarehouseId!.Value).Distinct().ToList();
        var branchIds = dtos.Where(d => d.BranchId.HasValue).Select(d => d.BranchId!.Value).Distinct().ToList();

        var types = await context.WarehouseTypeDefinitions.AsNoTracking()
            .Where(t => typeIds.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id, cancellationToken);
        var parents = await context.Warehouses.AsNoTracking()
            .Where(w => parentIds.Contains(w.Id))
            .ToDictionaryAsync(w => w.Id, w => w.NameAr, cancellationToken);
        var branches = await context.Branches.AsNoTracking()
            .Where(b => branchIds.Contains(b.Id))
            .ToDictionaryAsync(b => b.Id, b => b.NameAr, cancellationToken);

        return dtos.Select(d => d with
        {
            WarehouseTypeNameAr = d.WarehouseTypeId.HasValue && types.TryGetValue(d.WarehouseTypeId.Value, out var t)
                ? t.NameAr
                : d.WarehouseType.ToString(),
            ParentWarehouseNameAr = d.ParentWarehouseId.HasValue && parents.TryGetValue(d.ParentWarehouseId.Value, out var pn)
                ? pn
                : null,
            BranchNameAr = d.BranchId.HasValue && branches.TryGetValue(d.BranchId.Value, out var bn)
                ? bn
                : null
        }).ToList();
    }
}

public class GetWarehouseLookupQueryHandler : IRequestHandler<GetWarehouseLookupQuery, Result<List<WarehouseDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetWarehouseLookupQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<List<WarehouseDto>>> Handle(GetWarehouseLookupQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Warehouses.AsNoTracking().Include(w => w.Zones).Where(w => w.TenantId == request.TenantId);
        if (request.BranchId.HasValue) query = query.Where(w => w.BranchId == request.BranchId.Value);
        if (request.ActiveOnly) query = query.Where(w => w.IsActive);
        var items = await query.OrderBy(w => w.NameAr).ToListAsync(cancellationToken);
        var dtos = await GetWarehousesQueryHandler.EnrichWarehouseDtosAsync(
            _context, _mapper.Map<List<WarehouseDto>>(items), cancellationToken);
        return Result<List<WarehouseDto>>.Success(dtos);
    }
}

public class GetWarehouseTypeDefinitionsQueryHandler
    : IRequestHandler<GetWarehouseTypeDefinitionsQuery, Result<List<WarehouseTypeDefinitionDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetWarehouseTypeDefinitionsQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<List<WarehouseTypeDefinitionDto>>> Handle(
        GetWarehouseTypeDefinitionsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.WarehouseTypeDefinitions.AsNoTracking().Where(t => t.TenantId == request.TenantId);
        if (request.IsActive.HasValue) query = query.Where(t => t.IsActive == request.IsActive.Value);
        var items = await query.OrderBy(t => t.SortOrder).ThenBy(t => t.NameAr).ToListAsync(cancellationToken);
        return Result<List<WarehouseTypeDefinitionDto>>.Success(_mapper.Map<List<WarehouseTypeDefinitionDto>>(items));
    }
}

public class GetWarehouseByIdQueryHandler : IRequestHandler<GetWarehouseByIdQuery, Result<WarehouseDetailDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetWarehouseByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<WarehouseDetailDto>> Handle(GetWarehouseByIdQuery request, CancellationToken cancellationToken)
    {
        var wh = await _context.Warehouses.AsNoTracking()
            .Include(w => w.Zones)
                .ThenInclude(z => z.Shelves)
                    .ThenInclude(s => s.Bins)
            .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);
        if (wh == null) return Result<WarehouseDetailDto>.Failure("WarehouseNotFound", "Warehouse not found.");
        return Result<WarehouseDetailDto>.Success(_mapper.Map<WarehouseDetailDto>(wh));
    }
}

// ─── Supplier Handlers ────────────────────────────────────────────────────────

// Supplier query handlers moved to SupplierQueryHandlers.cs

// ─── PurchaseOrder Handlers ───────────────────────────────────────────────────

public class GetPurchaseOrdersQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetPurchaseOrdersQuery, PagedResult<PurchaseOrderDto>>
{
    public async Task<PagedResult<PurchaseOrderDto>> Handle(GetPurchaseOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = context.PurchaseOrders.AsNoTracking()
            .Include(p => p.Lines)
            .Where(p => p.TenantId == request.TenantId);

        if (request.SupplierId.HasValue)
            query = query.Where(p => p.SupplierId == request.SupplierId.Value);
        if (request.Status.HasValue)
            query = query.Where(p => p.Status == request.Status.Value);
        if (request.WarehouseId.HasValue)
            query = query.Where(p => p.DestinationWarehouseId == request.WarehouseId.Value);
        if (request.From.HasValue)
            query = query.Where(p => p.OrderDate >= request.From.Value);
        if (request.To.HasValue)
            query = query.Where(p => p.OrderDate <= request.To.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(p =>
                p.PoNumber.Contains(term) ||
                (p.ExternalReference != null && p.ExternalReference.Contains(term)) ||
                (p.Notes != null && p.Notes.Contains(term)));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(p => p.OrderDate)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = new List<PurchaseOrderDto>(items.Count);
        foreach (var po in items)
            dtos.Add(await PurchaseOrderMapper.ToDtoAsync(context, po, cancellationToken));

        return PagedResult<PurchaseOrderDto>.Success(dtos, total, request.PageNumber, request.PageSize);
    }
}

public class GetPurchaseOrderByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetPurchaseOrderByIdQuery, Result<PurchaseOrderDto>>
{
    public async Task<Result<PurchaseOrderDto>> Handle(GetPurchaseOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var po = await context.PurchaseOrders.AsNoTracking().Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (po is null)
            return Result<PurchaseOrderDto>.Failure("PurchaseOrderNotFound", "Purchase order not found.");
        return Result<PurchaseOrderDto>.Success(await PurchaseOrderMapper.ToDtoAsync(context, po, cancellationToken));
    }
}

public class GetPurchaseOrderDashboardQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetPurchaseOrderDashboardQuery, Result<PurchaseOrderDashboardDto>>
{
    public async Task<Result<PurchaseOrderDashboardDto>> Handle(
        GetPurchaseOrderDashboardQuery request, CancellationToken cancellationToken)
    {
        var today = DateTimeOffset.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        var now = DateTimeOffset.UtcNow;

        var rows = await context.PurchaseOrders.AsNoTracking()
            .Include(p => p.Lines)
            .Where(p => p.TenantId == request.TenantId)
            .ToListAsync(cancellationToken);

        var dto = new PurchaseOrderDashboardDto(
            OrdersToday: rows.Count(p => p.OrderDate >= today && p.OrderDate < tomorrow),
            ApprovedCount: rows.Count(p => p.Status is PurchaseOrderStatus.Approved or PurchaseOrderStatus.SentToSupplier),
            AwaitingReceiptCount: rows.Count(p =>
                (p.Status is PurchaseOrderStatus.Approved or PurchaseOrderStatus.SentToSupplier or PurchaseOrderStatus.PartiallyReceived)
                && p.RemainingQuantity > 0),
            ClosedCount: rows.Count(p => p.Status is PurchaseOrderStatus.Closed or PurchaseOrderStatus.FullyReceived),
            OverdueCount: rows.Count(p =>
                p.ExpectedDeliveryDate.HasValue
                && p.ExpectedDeliveryDate.Value < now
                && p.Status is not (PurchaseOrderStatus.Closed or PurchaseOrderStatus.Cancelled
                    or PurchaseOrderStatus.FullyReceived or PurchaseOrderStatus.Rejected)),
            TotalValue: rows
                .Where(p => p.Status is not (PurchaseOrderStatus.Cancelled or PurchaseOrderStatus.Rejected))
                .Sum(p => p.TotalAmount));

        return Result<PurchaseOrderDashboardDto>.Success(dto);
    }
}

// ─── Recipe Handlers ──────────────────────────────────────────────────────────

public class GetRecipesQueryHandler : IRequestHandler<GetRecipesQuery, PagedResult<RecipeDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetRecipesQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<PagedResult<RecipeDto>> Handle(GetRecipesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Recipes.AsNoTracking().Where(r => r.TenantId == request.TenantId);

        var count = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<RecipeDto>.Success(_mapper.Map<List<RecipeDto>>(items), count, request.PageNumber, request.PageSize);
    }
}

public class GetRecipesByProductIdQueryHandler : IRequestHandler<GetRecipesByProductIdQuery, Result<List<RecipeDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetRecipesByProductIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<List<RecipeDto>>> Handle(GetRecipesByProductIdQuery request, CancellationToken cancellationToken)
    {
        var recipes = await _context.Recipes.AsNoTracking()
            .Include(r => r.Items)
            .Where(r => r.ProductId == request.ProductId)
            .OrderByDescending(r => r.Version)
            .ToListAsync(cancellationToken);

        return Result<List<RecipeDto>>.Success(_mapper.Map<List<RecipeDto>>(recipes));
    }
}

public class GetRecipeByIdQueryHandler : IRequestHandler<GetRecipeByIdQuery, Result<RecipeDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetRecipeByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<RecipeDto>> Handle(GetRecipeByIdQuery request, CancellationToken cancellationToken)
    {
        var recipe = await _context.Recipes.AsNoTracking()
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (recipe == null) return Result<RecipeDto>.Failure("RecipeNotFound", "Recipe not found.");
        return Result<RecipeDto>.Success(_mapper.Map<RecipeDto>(recipe));
    }
}

// ─── GoodsReceipt query handlers moved to GoodsReceiptQueryHandlers.cs ────────

// ─── GoodsIssue Handlers ──────────────────────────────────────────────────────

public class GetGoodsIssuesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetGoodsIssuesQuery, PagedResult<GoodsIssueDto>>
{
    public async Task<PagedResult<GoodsIssueDto>> Handle(GetGoodsIssuesQuery request, CancellationToken cancellationToken)
    {
        var page = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 200 ? 20 : request.PageSize;

        var query = context.GoodsIssues.AsNoTracking()
            .Include(g => g.Lines)
            .Where(g => g.TenantId == request.TenantId);

        if (request.Status.HasValue)
            query = query.Where(g => (byte)g.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(g =>
                g.IssueNumber.Contains(term) ||
                (g.Notes != null && g.Notes.Contains(term)));
        }

        if (request.From.HasValue)
            query = query.Where(g => g.IssueDate >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(g => g.IssueDate <= request.To.Value);

        var total = await query.CountAsync(cancellationToken);
        var rows = await query.OrderByDescending(g => g.IssueDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = new List<GoodsIssueDto>();
        foreach (var g in rows)
            items.Add(await GoodsIssueMapper.ToDtoAsync(context, g, cancellationToken));

        return PagedResult<GoodsIssueDto>.Success(items, page, pageSize, total);
    }
}

public class GetGoodsIssueByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetGoodsIssueByIdQuery, Result<GoodsIssueDto>>
{
    public async Task<Result<GoodsIssueDto>> Handle(GetGoodsIssueByIdQuery request, CancellationToken cancellationToken)
    {
        var g = await context.GoodsIssues.AsNoTracking()
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (g is null) return Result<GoodsIssueDto>.Failure("GoodsIssueNotFound", "Goods issue not found.");

        return Result<GoodsIssueDto>.Success(await GoodsIssueMapper.ToDtoAsync(context, g, cancellationToken));
    }
}

public class GetIssueDestinationsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetIssueDestinationsQuery, Result<IReadOnlyList<IssueDestinationDto>>>
{
    public async Task<Result<IReadOnlyList<IssueDestinationDto>>> Handle(
        GetIssueDestinationsQuery request, CancellationToken cancellationToken)
    {
        var query = context.IssueDestinations.AsNoTracking()
            .Where(d => d.TenantId == request.TenantId);

        if (request.ActiveOnly)
            query = query.Where(d => d.IsActive);

        if (request.DestinationType.HasValue)
            query = query.Where(d => (byte)d.DestinationType == request.DestinationType.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(d =>
                d.Code.Contains(term) ||
                d.NameAr.Contains(term) ||
                (d.NameEn != null && d.NameEn.Contains(term)) ||
                d.DestinationType.ToString().Contains(term));
        }

        var rows = await query.OrderBy(d => d.SortOrder).ThenBy(d => d.NameAr).ToListAsync(cancellationToken);
        var items = new List<IssueDestinationDto>();
        foreach (var d in rows)
            items.Add(await IssueDestinationMapper.ToDtoAsync(context, d, cancellationToken));

        return Result<IReadOnlyList<IssueDestinationDto>>.Success(items);
    }
}

public class GetIssueDestinationByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetIssueDestinationByIdQuery, Result<IssueDestinationDto>>
{
    public async Task<Result<IssueDestinationDto>> Handle(
        GetIssueDestinationByIdQuery request, CancellationToken cancellationToken)
    {
        var d = await context.IssueDestinations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (d is null) return Result<IssueDestinationDto>.Failure("IssueDestinationNotFound", "Issue destination not found.");

        return Result<IssueDestinationDto>.Success(await IssueDestinationMapper.ToDtoAsync(context, d, cancellationToken));
    }
}

// ─── OpeningBalance Handlers ──────────────────────────────────────────────────

public class GetOpeningBalancesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetOpeningBalancesQuery, PagedResult<OpeningBalanceDto>>
{
    public async Task<PagedResult<OpeningBalanceDto>> Handle(GetOpeningBalancesQuery request, CancellationToken cancellationToken)
    {
        var page = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 200 ? 20 : request.PageSize;

        var query = context.OpeningBalances.AsNoTracking()
            .Include(o => o.Lines)
            .Where(o => o.TenantId == request.TenantId);

        var total = await query.CountAsync(cancellationToken);
        var rows = await query.OrderByDescending(o => o.DocumentDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = new List<OpeningBalanceDto>();
        foreach (var o in rows)
            items.Add(await OpeningBalanceMapper.ToDtoAsync(context, o, cancellationToken));

        return PagedResult<OpeningBalanceDto>.Success(items, page, pageSize, total);
    }
}

public class GetOpeningBalanceByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetOpeningBalanceByIdQuery, Result<OpeningBalanceDto>>
{
    public async Task<Result<OpeningBalanceDto>> Handle(GetOpeningBalanceByIdQuery request, CancellationToken cancellationToken)
    {
        var o = await context.OpeningBalances.AsNoTracking()
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (o == null) return Result<OpeningBalanceDto>.Failure("OpeningBalanceNotFound", "Opening balance not found.");

        return Result<OpeningBalanceDto>.Success(await OpeningBalanceMapper.ToDtoAsync(context, o, cancellationToken));
    }
}

// ─── StockTransfer Handlers ───────────────────────────────────────────────────

public class GetStockTransfersQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetStockTransfersQuery, PagedResult<StockTransferDto>>
{
    public async Task<PagedResult<StockTransferDto>> Handle(GetStockTransfersQuery request, CancellationToken cancellationToken)
    {
        var page = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 200 ? 20 : request.PageSize;

        var query = context.StockTransfers.AsNoTracking()
            .Include(t => t.Lines)
            .Where(t => t.TenantId == request.TenantId);

        if (request.SourceWarehouseId.HasValue)
            query = query.Where(t => t.SourceWarehouseId == request.SourceWarehouseId.Value);
        if (request.DestinationWarehouseId.HasValue)
            query = query.Where(t => t.DestinationWarehouseId == request.DestinationWarehouseId.Value);
        if (request.Status.HasValue)
            query = query.Where(t => (byte)t.Status == request.Status.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(t =>
                t.TransferNumber.Contains(term) ||
                (t.Notes != null && t.Notes.Contains(term)));
        }
        if (request.From.HasValue)
            query = query.Where(t => t.TransferDate >= request.From.Value);
        if (request.To.HasValue)
            query = query.Where(t => t.TransferDate <= request.To.Value);

        var total = await query.CountAsync(cancellationToken);
        var rows = await query.OrderByDescending(t => t.TransferDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = new List<StockTransferDto>();
        foreach (var t in rows)
            items.Add(await StockTransferMapper.ToDtoAsync(context, t, cancellationToken));

        return PagedResult<StockTransferDto>.Success(items, page, pageSize, total);
    }
}

public class GetStockTransferByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetStockTransferByIdQuery, Result<StockTransferDto>>
{
    public async Task<Result<StockTransferDto>> Handle(GetStockTransferByIdQuery request, CancellationToken cancellationToken)
    {
        var t = await context.StockTransfers.AsNoTracking()
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (t is null) return Result<StockTransferDto>.Failure("StockTransferNotFound", "Stock transfer not found.");

        return Result<StockTransferDto>.Success(await StockTransferMapper.ToDtoAsync(context, t, cancellationToken));
    }
}

// ─── StockAdjustment Handlers ─────────────────────────────────────────────────

public class GetStockAdjustmentsQueryHandler : IRequestHandler<GetStockAdjustmentsQuery, PagedResult<StockAdjustmentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetStockAdjustmentsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<PagedResult<StockAdjustmentDto>> Handle(GetStockAdjustmentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.StockAdjustments.AsNoTracking()
            .Include(a => a.Lines)
            .Where(a => a.TenantId == request.TenantId);
        if (request.WarehouseId.HasValue)
            query = query.Where(a => a.WarehouseId == request.WarehouseId.Value);

        var total = await query.CountAsync(cancellationToken);
        var rows = await query.OrderByDescending(a => a.AdjustmentDate)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var warehouseIds = rows.Select(r => r.WarehouseId).Distinct().ToList();
        var itemIds = rows.SelectMany(r => r.Lines.Select(l => l.InventoryItemId)).Distinct().ToList();

        var warehouses = await _context.Warehouses.AsNoTracking()
            .Where(w => warehouseIds.Contains(w.Id))
            .ToDictionaryAsync(w => w.Id, w => w.NameAr, cancellationToken);
        var itemNames = await _context.InventoryItems.AsNoTracking()
            .Where(i => itemIds.Contains(i.Id))
            .ToDictionaryAsync(i => i.Id, i => i.NameAr, cancellationToken);

        var items = rows.Select(a =>
        {
            var line = a.Lines.FirstOrDefault();
            return new StockAdjustmentDto(
                a.Id,
                a.TenantId,
                a.WarehouseId,
                warehouses.TryGetValue(a.WarehouseId, out var wh) ? wh : string.Empty,
                line?.InventoryItemId ?? Guid.Empty,
                line != null && itemNames.TryGetValue(line.InventoryItemId, out var name) ? name : string.Empty,
                a.AdjustmentNumber,
                line?.AdjustmentQuantity ?? 0,
                line?.AdjustmentReasonId,
                a.Notes,
                a.AdjustmentDate,
                a.CreatedAt.UtcDateTime);
        }).ToList();

        return PagedResult<StockAdjustmentDto>.Success(items, total, request.PageNumber, request.PageSize);
    }
}

// ─── WasteRecord Handlers ─────────────────────────────────────────────────────

public class GetWasteRecordsQueryHandler : IRequestHandler<GetWasteRecordsQuery, PagedResult<WasteRecordDto>>
{
    private readonly IApplicationDbContext _context;

    public GetWasteRecordsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<PagedResult<WasteRecordDto>> Handle(GetWasteRecordsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.WasteRecords.AsNoTracking()
            .Include(w => w.Items)
            .Where(w => w.TenantId == request.TenantId);
        if (request.WarehouseId.HasValue)
            query = query.Where(w => w.WarehouseId == request.WarehouseId.Value);

        var total = await query.CountAsync(cancellationToken);
        var rows = await query.OrderByDescending(w => w.WasteDate)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var warehouseIds = rows.Select(r => r.WarehouseId).Distinct().ToList();
        var itemIds = rows.SelectMany(r => r.Items.Select(i => i.InventoryItemId)).Distinct().ToList();

        var warehouses = await _context.Warehouses.AsNoTracking()
            .Where(w => warehouseIds.Contains(w.Id))
            .ToDictionaryAsync(w => w.Id, w => w.NameAr, cancellationToken);
        var itemNames = await _context.InventoryItems.AsNoTracking()
            .Where(i => itemIds.Contains(i.Id))
            .ToDictionaryAsync(i => i.Id, i => i.NameAr, cancellationToken);

        var items = rows.Select(w =>
        {
            var line = w.Items.FirstOrDefault();
            return new WasteRecordDto(
                w.Id,
                w.TenantId,
                w.WarehouseId,
                warehouses.TryGetValue(w.WarehouseId, out var wh) ? wh : string.Empty,
                line?.InventoryItemId ?? Guid.Empty,
                line != null && itemNames.TryGetValue(line.InventoryItemId, out var name) ? name : string.Empty,
                w.RecordNumber,
                line?.Quantity ?? 0,
                line?.UnitCost ?? 0,
                line?.WasteReasonId,
                w.Notes,
                w.WasteDate,
                w.CreatedAt.UtcDateTime);
        }).ToList();

        return PagedResult<WasteRecordDto>.Success(items, total, request.PageNumber, request.PageSize);
    }
}

// ─── InventoryTransaction Handlers ───────────────────────────────────────────

public class GetInventoryTransactionsQueryHandler : IRequestHandler<GetInventoryTransactionsQuery, PagedResult<InventoryTransactionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetInventoryTransactionsQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<PagedResult<InventoryTransactionDto>> Handle(GetInventoryTransactionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.InventoryTransactions.AsNoTracking()
            .Include(t => t.Movements)
            .Where(t => t.TenantId == request.TenantId);

        if (request.WarehouseId.HasValue)
            query = query.Where(t => t.Movements.Any(m => m.WarehouseId == request.WarehouseId.Value));

        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(t => t.TransactionDate).Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToListAsync(cancellationToken);

        return PagedResult<InventoryTransactionDto>.Success(_mapper.Map<List<InventoryTransactionDto>>(items), total, request.PageNumber, request.PageSize);
    }
}

// ─── StockCount Handlers ──────────────────────────────────────────────────────

public class GetStockCountsQueryHandler : IRequestHandler<GetStockCountsQuery, PagedResult<StockCountDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetStockCountsQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<PagedResult<StockCountDto>> Handle(GetStockCountsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.StockCounts.AsNoTracking()
            .Include(s => s.Lines)
            .Where(s => s.TenantId == request.TenantId);

        if (request.WarehouseId.HasValue) query = query.Where(s => s.WarehouseId == request.WarehouseId.Value);
        if (request.Status.HasValue) query = query.Where(s => s.Status == request.Status.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(s => s.CountDate).Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToListAsync(cancellationToken);

        return PagedResult<StockCountDto>.Success(_mapper.Map<List<StockCountDto>>(items), total, request.PageNumber, request.PageSize);
    }
}

public class GetStockCountByIdQueryHandler : IRequestHandler<GetStockCountByIdQuery, Result<StockCountDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetStockCountByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<StockCountDto>> Handle(GetStockCountByIdQuery request, CancellationToken cancellationToken)
    {
        var stockCount = await _context.StockCounts.AsNoTracking()
            .Include(s => s.Lines)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (stockCount == null) return Result<StockCountDto>.Failure("StockCountNotFound", "Stock count not found.");

        return Result<StockCountDto>.Success(_mapper.Map<StockCountDto>(stockCount));
    }
}

// ─── PurchaseReturn query handlers moved to PurchaseReturnQueryHandlers.cs ────


// ─── InventoryReservation Handlers ────────────────────────────────────────────

public class GetInventoryReservationsQueryHandler : IRequestHandler<GetInventoryReservationsQuery, PagedResult<InventoryReservationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetInventoryReservationsQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<PagedResult<InventoryReservationDto>> Handle(GetInventoryReservationsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.InventoryReservations.AsNoTracking()
            .Where(r => r.TenantId == request.TenantId);

        if (request.WarehouseId.HasValue) query = query.Where(r => r.WarehouseId == request.WarehouseId.Value);
        if (request.InventoryItemId.HasValue) query = query.Where(r => r.InventoryItemId == request.InventoryItemId.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(r => r.ExpirationDate).Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToListAsync(cancellationToken);

        return PagedResult<InventoryReservationDto>.Success(_mapper.Map<List<InventoryReservationDto>>(items), total, request.PageNumber, request.PageSize);
    }
}

// ─── Dashboard (Phase F) ──────────────────────────────────────────────────────

public class GetInventoryDashboardQueryHandler : IRequestHandler<GetInventoryDashboardQuery, Result<InventoryDashboardDto>>
{
    private readonly IApplicationDbContext _context;

    public GetInventoryDashboardQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<InventoryDashboardDto>> Handle(GetInventoryDashboardQuery request, CancellationToken cancellationToken)
    {
        var tenantId = request.TenantId;

        var totalItems = await _context.InventoryItems.AsNoTracking().CountAsync(i => i.TenantId == tenantId, cancellationToken);
        var activeItems = await _context.InventoryItems.AsNoTracking().CountAsync(i => i.TenantId == tenantId && i.IsActive, cancellationToken);
        var inactiveItems = totalItems - activeItems;
        var categoryCount = await _context.InventoryCategories.AsNoTracking().CountAsync(c => c.TenantId == tenantId, cancellationToken);
        var lowStock = await _context.InventoryItems.AsNoTracking()
            .CountAsync(i => i.TenantId == tenantId && i.IsActive && i.ReorderLevel > 0, cancellationToken);

        var warehouseCount = await _context.Warehouses.AsNoTracking().CountAsync(w => w.TenantId == tenantId, cancellationToken);
        var activeWarehouses = await _context.Warehouses.AsNoTracking().CountAsync(w => w.TenantId == tenantId && w.IsActive, cancellationToken);

        var openTransfers = await _context.StockTransfers.AsNoTracking()
            .CountAsync(t => t.TenantId == tenantId && t.Status != Domain.Enums.StockTransferStatus.Completed && t.Status != Domain.Enums.StockTransferStatus.Cancelled, cancellationToken);

        var openCounts = await _context.StockCounts.AsNoTracking()
            .CountAsync(c => c.TenantId == tenantId && c.Status != Domain.Enums.StockCountStatus.Completed && c.Status != Domain.Enums.StockCountStatus.Cancelled, cancellationToken);

        var activeReservations = await _context.InventoryReservations.AsNoTracking()
            .CountAsync(r => r.TenantId == tenantId && r.Status == Domain.Enums.ReservationStatus.Active, cancellationToken);

        var draftGrn = await _context.GoodsReceipts.AsNoTracking()
            .CountAsync(g => g.TenantId == tenantId && g.Status == Domain.Enums.GoodsReceiptStatus.Draft, cancellationToken);

        var openWaste = await _context.WasteRecords.AsNoTracking()
            .CountAsync(w => w.TenantId == tenantId && !w.IsCompleted, cancellationToken);

        var warehouses = await _context.Warehouses.AsNoTracking()
            .Where(w => w.TenantId == tenantId)
            .OrderBy(w => w.NameAr)
            .Take(12)
            .Select(w => new InventoryDashboardWarehouseDto(w.Id, w.NameAr, w.NameEn, w.Code, w.WarehouseType, w.IsActive))
            .ToListAsync(cancellationToken);

        var recentTx = await _context.InventoryTransactions.AsNoTracking()
            .Where(t => t.TenantId == tenantId)
            .OrderByDescending(t => t.TransactionDate)
            .Take(8)
            .Select(t => new InventoryDashboardActivityDto(
                t.Id,
                t.TransactionType.ToString(),
                t.ReferenceDocumentNumber,
                t.TransactionDate,
                t.Notes))
            .ToListAsync(cancellationToken);

        // Fallback recent activity from operational documents when ledger is still empty (Phase E gap).
        var activities = recentTx;
        if (activities.Count == 0)
        {
            var transfers = await _context.StockTransfers.AsNoTracking()
                .Where(t => t.TenantId == tenantId)
                .OrderByDescending(t => t.TransferDate)
                .Take(4)
                .Select(t => new InventoryDashboardActivityDto(t.Id, "Transfer", t.TransferNumber, t.TransferDate, t.Notes))
                .ToListAsync(cancellationToken);
            var receipts = await _context.GoodsReceipts.AsNoTracking()
                .Where(g => g.TenantId == tenantId)
                .OrderByDescending(g => g.ReceiptDate)
                .Take(4)
                .Select(g => new InventoryDashboardActivityDto(g.Id, "GoodsReceipt", g.ReceiptNumber, g.ReceiptDate, g.Notes))
                .ToListAsync(cancellationToken);
            activities = transfers.Concat(receipts).OrderByDescending(a => a.OccurredAt).Take(8).ToList();
        }

        var alerts = new List<InventoryDashboardAlertDto>();
        if (lowStock > 0)
        {
            alerts.Add(new InventoryDashboardAlertDto(
                "LOW_STOCK",
                "warning",
                $"{lowStock} item(s) on reorder watchlist.",
                $"{lowStock} صنف/أصناف في قائمة مراقبة إعادة الطلب.",
                "/inventory/items"));
        }
        if (openTransfers > 0)
        {
            alerts.Add(new InventoryDashboardAlertDto(
                "OPEN_TRANSFERS",
                "info",
                $"{openTransfers} transfer(s) still open.",
                $"{openTransfers} تحويل/تحويلات مفتوحة.",
                "/inventory/transactions"));
        }
        if (openCounts > 0)
        {
            alerts.Add(new InventoryDashboardAlertDto(
                "OPEN_COUNTS",
                "info",
                $"{openCounts} stock count(s) in progress.",
                $"{openCounts} عملية/عمليات جرد قيد التنفيذ.",
                "/inventory/transactions"));
        }
        if (draftGrn > 0)
        {
            alerts.Add(new InventoryDashboardAlertDto(
                "DRAFT_GRN",
                "warning",
                $"{draftGrn} goods receipt(s) awaiting confirm.",
                $"{draftGrn} إذن/أذونات استلام بانتظار التأكيد.",
                "/inventory/transactions"));
        }

        var movementAgg = await _context.StockMovements.AsNoTracking()
            .Where(m => m.TenantId == tenantId)
            .GroupBy(m => m.InventoryItemId)
            .Select(g => new
            {
                ItemId = g.Key,
                InQty = g.Sum(x => x.QuantityChange > 0 ? x.QuantityChange : 0),
                OutQty = g.Sum(x => x.QuantityChange < 0 ? -x.QuantityChange : 0)
            })
            .OrderByDescending(x => x.InQty + x.OutQty)
            .Take(5)
            .ToListAsync(cancellationToken);

        var moverIds = movementAgg.Select(x => x.ItemId).ToList();
        var moverNames = await _context.InventoryItems.AsNoTracking()
            .Where(i => moverIds.Contains(i.Id))
            .ToDictionaryAsync(i => i.Id, i => new { i.NameAr, i.NameEn }, cancellationToken);

        var topMovers = movementAgg
            .Select(m =>
            {
                moverNames.TryGetValue(m.ItemId, out var name);
                return new InventoryDashboardTopMoverDto(
                    m.ItemId,
                    name?.NameAr ?? m.ItemId.ToString()[..8],
                    name?.NameEn,
                    m.InQty,
                    m.OutQty);
            })
            .ToList();

        var categoryGroups = await _context.InventoryItems.AsNoTracking()
            .Where(i => i.TenantId == tenantId)
            .GroupBy(i => i.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(8)
            .ToListAsync(cancellationToken);

        var catIds = categoryGroups.Select(c => c.CategoryId).Distinct().ToList();
        var catNames = await _context.InventoryCategories.AsNoTracking()
            .Where(c => catIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => new { c.NameAr, c.NameEn }, cancellationToken);

        var categoryDistribution = categoryGroups
            .Select(c =>
            {
                catNames.TryGetValue(c.CategoryId, out var name);
                return new InventoryDashboardCategorySliceDto(
                    c.CategoryId,
                    name?.NameAr ?? "—",
                    name?.NameEn,
                    c.Count);
            })
            .ToList();

        var dto = new InventoryDashboardDto(
            totalItems,
            activeItems,
            inactiveItems,
            categoryCount,
            lowStock,
            warehouseCount,
            activeWarehouses,
            openTransfers,
            openCounts,
            activeReservations,
            draftGrn,
            openWaste,
            warehouses,
            activities,
            alerts,
            topMovers,
            categoryDistribution);

        return Result<InventoryDashboardDto>.Success(dto);
    }
}
