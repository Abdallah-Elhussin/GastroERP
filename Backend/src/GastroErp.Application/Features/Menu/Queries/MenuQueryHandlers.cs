using AutoMapper;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Menu.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Menu.Queries;

// ─── Category Query Handlers ──────────────────────────────────────────────────

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, Result<CategoryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetCategoryByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (category == null) return Result<CategoryDto>.Failure("CategoryNotFound", "Category not found.");
        return Result<CategoryDto>.Success(_mapper.Map<CategoryDto>(category));
    }
}

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, PagedResult<CategoryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetCategoriesQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<PagedResult<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Categories.AsNoTracking().Where(c => c.TenantId == request.TenantId);
        if (request.ParentCategoryId.HasValue) query = query.Where(c => c.ParentCategoryId == request.ParentCategoryId.Value);
        if (request.IsActive.HasValue) query = query.Where(c => c.IsActive == request.IsActive.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(c => c.SortOrder).ThenBy(c => c.NameAr)
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<CategoryDto>.Success(_mapper.Map<List<CategoryDto>>(items), total, request.PageNumber, request.PageSize);
    }
}

// ─── PriceLevel Query Handlers ────────────────────────────────────────────────

public class GetPriceLevelByIdQueryHandler : IRequestHandler<GetPriceLevelByIdQuery, Result<PriceLevelDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetPriceLevelByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<PriceLevelDto>> Handle(GetPriceLevelByIdQuery request, CancellationToken cancellationToken)
    {
        var pl = await _context.PriceLevels.AsNoTracking().FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (pl == null) return Result<PriceLevelDto>.Failure("PriceLevelNotFound", "Price level not found.");
        return Result<PriceLevelDto>.Success(_mapper.Map<PriceLevelDto>(pl));
    }
}

public class GetPriceLevelsQueryHandler : IRequestHandler<GetPriceLevelsQuery, PagedResult<PriceLevelDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetPriceLevelsQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<PagedResult<PriceLevelDto>> Handle(GetPriceLevelsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.PriceLevels.AsNoTracking().Where(p => p.TenantId == request.TenantId);
        if (request.SalesChannel.HasValue) query = query.Where(p => p.SalesChannel == request.SalesChannel.Value);
        if (request.IsActive.HasValue) query = query.Where(p => p.IsActive == request.IsActive.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(p => p.IsDefault).ThenBy(p => p.NameAr)
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<PriceLevelDto>.Success(_mapper.Map<List<PriceLevelDto>>(items), total, request.PageNumber, request.PageSize);
    }
}

// ─── Menu Query Handlers ──────────────────────────────────────────────────────

public class GetMenuByIdQueryHandler : IRequestHandler<GetMenuByIdQuery, Result<MenuDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetMenuByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<MenuDto>> Handle(GetMenuByIdQuery request, CancellationToken cancellationToken)
    {
        var menu = await _context.Menus.AsNoTracking()
            .Include(m => m.Sections)
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);
        if (menu == null) return Result<MenuDto>.Failure("MenuNotFound", "Menu not found.");
        return Result<MenuDto>.Success(_mapper.Map<MenuDto>(menu));
    }
}

public class GetMenusQueryHandler : IRequestHandler<GetMenusQuery, PagedResult<MenuDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetMenusQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<PagedResult<MenuDto>> Handle(GetMenusQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Menus.AsNoTracking().Include(m => m.Sections).Where(m => m.TenantId == request.TenantId);
        if (request.IsActive.HasValue) query = query.Where(m => m.IsActive == request.IsActive.Value);
        if (request.MenuType.HasValue) query = query.Where(m => m.MenuType == request.MenuType.Value);
        if (request.SalesChannel.HasValue) query = query.Where(m => m.SalesChannel == request.SalesChannel.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(m => m.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<MenuDto>.Success(_mapper.Map<List<MenuDto>>(items), total, request.PageNumber, request.PageSize);
    }
}

// ─── MenuSection Query Handlers ───────────────────────────────────────────────

public class GetMenuSectionsByMenuIdQueryHandler : IRequestHandler<GetMenuSectionsByMenuIdQuery, Result<List<MenuSectionDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetMenuSectionsByMenuIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<List<MenuSectionDto>>> Handle(GetMenuSectionsByMenuIdQuery request, CancellationToken cancellationToken)
    {
        var sections = await _context.MenuSections.AsNoTracking()
            .Include(s => s.Items)
            .Where(s => s.MenuId == request.MenuId)
            .OrderBy(s => s.SortOrder)
            .ToListAsync(cancellationToken);

        return Result<List<MenuSectionDto>>.Success(_mapper.Map<List<MenuSectionDto>>(sections));
    }
}

// ─── BranchMenu Query Handlers ────────────────────────────────────────────────

public class GetBranchMenusByBranchIdQueryHandler : IRequestHandler<GetBranchMenusByBranchIdQuery, Result<List<BranchMenuDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetBranchMenusByBranchIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<List<BranchMenuDto>>> Handle(GetBranchMenusByBranchIdQuery request, CancellationToken cancellationToken)
    {
        var query = _context.BranchMenus.AsNoTracking()
            .Include(b => b.Availabilities)
            .Where(b => b.BranchId == request.BranchId);
        if (request.IsActive.HasValue) query = query.Where(b => b.IsActive == request.IsActive.Value);

        var items = await query.OrderBy(b => b.SortOrder).ToListAsync(cancellationToken);
        return Result<List<BranchMenuDto>>.Success(_mapper.Map<List<BranchMenuDto>>(items));
    }
}

public class GetBranchMenusByMenuIdQueryHandler : IRequestHandler<GetBranchMenusByMenuIdQuery, Result<List<BranchMenuDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetBranchMenusByMenuIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<List<BranchMenuDto>>> Handle(GetBranchMenusByMenuIdQuery request, CancellationToken cancellationToken)
    {
        var items = await _context.BranchMenus.AsNoTracking()
            .Include(b => b.Availabilities)
            .Where(b => b.MenuId == request.MenuId)
            .ToListAsync(cancellationToken);

        return Result<List<BranchMenuDto>>.Success(_mapper.Map<List<BranchMenuDto>>(items));
    }
}

// ─── Product Query Handlers ───────────────────────────────────────────────────

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetProductByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.AsNoTracking()
            .Include(p => p.ModifierGroups).ThenInclude(g => g.Modifiers)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (product == null) return Result<ProductDto>.Failure("ProductNotFound", "Product not found.");
        return Result<ProductDto>.Success(_mapper.Map<ProductDto>(product));
    }
}

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PagedResult<ProductDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetProductsQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<PagedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Products.AsNoTracking()
            .Include(p => p.ModifierGroups)
            .Include(p => p.Images)
            .Where(p => p.TenantId == request.TenantId);

        if (request.CategoryId.HasValue) query = query.Where(p => p.CategoryId == request.CategoryId.Value);
        if (request.IsAvailable.HasValue) query = query.Where(p => p.IsAvailable == request.IsAvailable.Value);
        if (request.IsFeatured.HasValue) query = query.Where(p => p.IsFeatured == request.IsFeatured.Value);
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(p => p.NameAr.Contains(request.SearchTerm) || (p.NameEn != null && p.NameEn.Contains(request.SearchTerm)));

        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(p => p.SortOrder).ThenBy(p => p.NameAr)
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<ProductDto>.Success(_mapper.Map<List<ProductDto>>(items), total, request.PageNumber, request.PageSize);
    }
}

public class GetProductsByCategoryQueryHandler : IRequestHandler<GetProductsByCategoryQuery, Result<List<ProductDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetProductsByCategoryQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<List<ProductDto>>> Handle(GetProductsByCategoryQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Products.AsNoTracking()
            .Include(p => p.ModifierGroups)
            .Include(p => p.Images)
            .Where(p => p.CategoryId == request.CategoryId);
        if (request.IsAvailable.HasValue) query = query.Where(p => p.IsAvailable == request.IsAvailable.Value);

        var items = await query.OrderBy(p => p.SortOrder).ToListAsync(cancellationToken);
        return Result<List<ProductDto>>.Success(_mapper.Map<List<ProductDto>>(items));
    }
}

public class GetModifierGroupsByProductIdQueryHandler : IRequestHandler<GetModifierGroupsByProductIdQuery, Result<List<ModifierGroupDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetModifierGroupsByProductIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<List<ModifierGroupDto>>> Handle(GetModifierGroupsByProductIdQuery request, CancellationToken cancellationToken)
    {
        var groups = await _context.ModifierGroups.AsNoTracking()
            .Include(g => g.Modifiers)
            .Where(g => g.ProductId == request.ProductId)
            .OrderBy(g => g.SortOrder)
            .ToListAsync(cancellationToken);

        return Result<List<ModifierGroupDto>>.Success(_mapper.Map<List<ModifierGroupDto>>(groups));
    }
}

public class GetOptionGroupsByProductIdQueryHandler : IRequestHandler<GetOptionGroupsByProductIdQuery, Result<List<OptionGroupDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetOptionGroupsByProductIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<List<OptionGroupDto>>> Handle(GetOptionGroupsByProductIdQuery request, CancellationToken cancellationToken)
    {
        var groups = await _context.OptionGroups.AsNoTracking()
            .Include(g => g.Options)
            .Where(g => g.ProductId == request.ProductId)
            .ToListAsync(cancellationToken);

        return Result<List<OptionGroupDto>>.Success(_mapper.Map<List<OptionGroupDto>>(groups));
    }
}

// ─── Combo Handlers ───────────────────────────────────────────────────────────

public class GetCombosQueryHandler : IRequestHandler<GetCombosQuery, PagedResult<ComboDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetCombosQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<PagedResult<ComboDto>> Handle(GetCombosQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ComboMeals.AsNoTracking().Where(c => c.TenantId == request.TenantId);

        var count = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<ComboDto>.Success(_mapper.Map<List<ComboDto>>(items), count, request.PageNumber, request.PageSize);
    }
}

public class GetComboByIdQueryHandler : IRequestHandler<GetComboByIdQuery, Result<ComboDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetComboByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<ComboDto>> Handle(GetComboByIdQuery request, CancellationToken cancellationToken)
    {
        var combo = await _context.ComboMeals.AsNoTracking().FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (combo == null) return Result<ComboDto>.Failure("ComboNotFound", "Combo not found.");

        return Result<ComboDto>.Success(_mapper.Map<ComboDto>(combo));
    }
}

// ─── Modifier Handlers ────────────────────────────────────────────────────────

public class GetModifiersQueryHandler : IRequestHandler<GetModifiersQuery, PagedResult<ModifierDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetModifiersQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<PagedResult<ModifierDto>> Handle(GetModifiersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Modifiers.AsNoTracking().Where(m => m.TenantId == request.TenantId);

        var count = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<ModifierDto>.Success(_mapper.Map<List<ModifierDto>>(items), count, request.PageNumber, request.PageSize);
    }
}

public class GetModifierByIdQueryHandler : IRequestHandler<GetModifierByIdQuery, Result<ModifierDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetModifierByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        => (_context, _mapper) = (context, mapper);

    public async Task<Result<ModifierDto>> Handle(GetModifierByIdQuery request, CancellationToken cancellationToken)
    {
        var modifier = await _context.Modifiers.AsNoTracking().FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);
        if (modifier == null) return Result<ModifierDto>.Failure("ModifierNotFound", "Modifier not found.");

        return Result<ModifierDto>.Success(_mapper.Map<ModifierDto>(modifier));
    }
}
