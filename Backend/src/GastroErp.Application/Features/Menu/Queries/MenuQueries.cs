using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Menu.DTOs;
using GastroErp.Domain.Enums;
using MediatR;

namespace GastroErp.Application.Features.Menu.Queries;

// ─── Category Queries ─────────────────────────────────────────────────────────
public record GetCategoryByIdQuery(Guid Id) : IRequest<Result<CategoryDto>>;
public record GetCategoriesQuery(Guid TenantId, Guid? ParentCategoryId = null, bool? IsActive = null, int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<CategoryDto>>;

// ─── PriceLevel Queries ───────────────────────────────────────────────────────
public record GetPriceLevelByIdQuery(Guid Id) : IRequest<Result<PriceLevelDto>>;
public record GetPriceLevelsQuery(Guid TenantId, SalesChannel? SalesChannel = null, bool? IsActive = null, int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<PriceLevelDto>>;

// ─── Menu Queries ─────────────────────────────────────────────────────────────
public record GetMenuByIdQuery(Guid Id) : IRequest<Result<MenuDto>>;
public record GetMenusQuery(Guid TenantId, bool? IsActive = null, MenuType? MenuType = null, SalesChannel? SalesChannel = null, int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<MenuDto>>;

// ─── MenuSection Queries ──────────────────────────────────────────────────────
public record GetMenuSectionsByMenuIdQuery(Guid MenuId) : IRequest<Result<List<MenuSectionDto>>>;

// ─── BranchMenu Queries ───────────────────────────────────────────────────────
public record GetBranchMenusByBranchIdQuery(Guid BranchId, bool? IsActive = null) : IRequest<Result<List<BranchMenuDto>>>;
public record GetBranchMenusByMenuIdQuery(Guid MenuId) : IRequest<Result<List<BranchMenuDto>>>;

// ─── Product Queries ──────────────────────────────────────────────────────────
public record GetProductByIdQuery(Guid Id) : IRequest<Result<ProductDto>>;
public record GetProductsQuery(Guid TenantId, Guid? CategoryId = null, bool? IsAvailable = null, bool? IsFeatured = null, string? SearchTerm = null, int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<ProductDto>>;
public record GetProductsByCategoryQuery(Guid CategoryId, bool? IsAvailable = null) : IRequest<Result<List<ProductDto>>>;

// ─── ModifierGroup Queries ────────────────────────────────────────────────────
public record GetModifierGroupsByProductIdQuery(Guid ProductId) : IRequest<Result<List<ModifierGroupDto>>>;

// ─── OptionGroup Queries ──────────────────────────────────────────────────────
public record GetOptionGroupsByProductIdQuery(Guid ProductId) : IRequest<Result<List<OptionGroupDto>>>;

// ─── Combo Queries ────────────────────────────────────────────────────────────
public record GetCombosQuery(Guid TenantId, int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<ComboDto>>;
public record GetComboByIdQuery(Guid Id) : IRequest<Result<ComboDto>>;

// ─── Modifier Queries ─────────────────────────────────────────────────────────
public record GetModifiersQuery(Guid TenantId, int PageNumber = 1, int PageSize = 20) : IRequest<PagedResult<ModifierDto>>;
public record GetModifierByIdQuery(Guid Id) : IRequest<Result<ModifierDto>>;
