using AutoMapper;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Inventory.DTOs;
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
        var items = await query.OrderBy(c => c.NameAr).Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToListAsync(cancellationToken);

        return PagedResult<InventoryCategoryDto>.Success(_mapper.Map<List<InventoryCategoryDto>>(items), total, request.PageNumber, request.PageSize);
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
        var items = await query.OrderBy(u => u.NameAr).ToListAsync(cancellationToken);
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

        return PagedResult<InventoryItemDto>.Success(_mapper.Map<List<InventoryItemDto>>(items), total, request.PageNumber, request.PageSize);
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
        return Result<InventoryItemDto>.Success(_mapper.Map<InventoryItemDto>(item));
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
        var query = _context.Warehouses.AsNoTracking().Include(w => w.Zones).Where(w => w.TenantId == request.TenantId);
        if (request.BranchId.HasValue) query = query.Where(w => w.BranchId == request.BranchId.Value);
        if (request.IsActive.HasValue) query = query.Where(w => w.IsActive == request.IsActive.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(w => w.NameAr).Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToListAsync(cancellationToken);

        return PagedResult<WarehouseDto>.Success(_mapper.Map<List<WarehouseDto>>(items), total, request.PageNumber, request.PageSize);
    }
}

public class GetWarehouseByIdQueryHandler : IRequestHandler<GetWarehouseByIdQuery, Result<WarehouseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetWarehouseByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<WarehouseDto>> Handle(GetWarehouseByIdQuery request, CancellationToken cancellationToken)
    {
        var wh = await _context.Warehouses.AsNoTracking().Include(w => w.Zones).FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);
        if (wh == null) return Result<WarehouseDto>.Failure("WarehouseNotFound", "Warehouse not found.");
        return Result<WarehouseDto>.Success(_mapper.Map<WarehouseDto>(wh));
    }
}

// ─── Supplier Handlers ────────────────────────────────────────────────────────

public class GetSuppliersQueryHandler : IRequestHandler<GetSuppliersQuery, PagedResult<SupplierDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetSuppliersQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<PagedResult<SupplierDto>> Handle(GetSuppliersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Suppliers.AsNoTracking().Include(s => s.Contacts).Where(s => s.TenantId == request.TenantId);
        if (request.IsActive.HasValue) query = query.Where(s => s.IsActive == request.IsActive.Value);
        if (request.IsPreferred.HasValue) query = query.Where(s => s.IsPreferred == request.IsPreferred.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(s => s.IsPreferred).ThenBy(s => s.NameAr).Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToListAsync(cancellationToken);

        return PagedResult<SupplierDto>.Success(_mapper.Map<List<SupplierDto>>(items), total, request.PageNumber, request.PageSize);
    }
}

public class GetSupplierByIdQueryHandler : IRequestHandler<GetSupplierByIdQuery, Result<SupplierDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetSupplierByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<SupplierDto>> Handle(GetSupplierByIdQuery request, CancellationToken cancellationToken)
    {
        var supplier = await _context.Suppliers.AsNoTracking().Include(s => s.Contacts).FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (supplier == null) return Result<SupplierDto>.Failure("SupplierNotFound", "Supplier not found.");
        return Result<SupplierDto>.Success(_mapper.Map<SupplierDto>(supplier));
    }
}

// ─── PurchaseOrder Handlers ───────────────────────────────────────────────────

public class GetPurchaseOrdersQueryHandler : IRequestHandler<GetPurchaseOrdersQuery, PagedResult<PurchaseOrderDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetPurchaseOrdersQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<PagedResult<PurchaseOrderDto>> Handle(GetPurchaseOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.PurchaseOrders.AsNoTracking().Include(p => p.Lines).Where(p => p.TenantId == request.TenantId);
        if (request.SupplierId.HasValue) query = query.Where(p => p.SupplierId == request.SupplierId.Value);
        if (request.Status.HasValue) query = query.Where(p => p.Status == request.Status.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(p => p.OrderDate).Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToListAsync(cancellationToken);

        return PagedResult<PurchaseOrderDto>.Success(_mapper.Map<List<PurchaseOrderDto>>(items), total, request.PageNumber, request.PageSize);
    }
}

public class GetPurchaseOrderByIdQueryHandler : IRequestHandler<GetPurchaseOrderByIdQuery, Result<PurchaseOrderDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetPurchaseOrderByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<PurchaseOrderDto>> Handle(GetPurchaseOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var po = await _context.PurchaseOrders.AsNoTracking().Include(p => p.Lines).FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (po == null) return Result<PurchaseOrderDto>.Failure("PurchaseOrderNotFound", "Purchase order not found.");
        return Result<PurchaseOrderDto>.Success(_mapper.Map<PurchaseOrderDto>(po));
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

// ─── PurchaseReturn Handlers ──────────────────────────────────────────────────

public class GetPurchaseReturnsQueryHandler : IRequestHandler<GetPurchaseReturnsQuery, PagedResult<PurchaseReturnDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetPurchaseReturnsQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<PagedResult<PurchaseReturnDto>> Handle(GetPurchaseReturnsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.PurchaseReturns.AsNoTracking()
            .Include(r => r.Lines)
            .Where(r => r.TenantId == request.TenantId);

        if (request.SupplierId.HasValue) query = query.Where(r => r.SupplierId == request.SupplierId.Value);
        if (request.WarehouseId.HasValue) query = query.Where(r => r.WarehouseId == request.WarehouseId.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(r => r.ReturnDate).Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToListAsync(cancellationToken);

        return PagedResult<PurchaseReturnDto>.Success(_mapper.Map<List<PurchaseReturnDto>>(items), total, request.PageNumber, request.PageSize);
    }
}

public class GetPurchaseReturnByIdQueryHandler : IRequestHandler<GetPurchaseReturnByIdQuery, Result<PurchaseReturnDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetPurchaseReturnByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<PurchaseReturnDto>> Handle(GetPurchaseReturnByIdQuery request, CancellationToken cancellationToken)
    {
        var purchaseReturn = await _context.PurchaseReturns.AsNoTracking()
            .Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (purchaseReturn == null) return Result<PurchaseReturnDto>.Failure("PurchaseReturnNotFound", "Purchase return not found.");

        return Result<PurchaseReturnDto>.Success(_mapper.Map<PurchaseReturnDto>(purchaseReturn));
    }
}

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
