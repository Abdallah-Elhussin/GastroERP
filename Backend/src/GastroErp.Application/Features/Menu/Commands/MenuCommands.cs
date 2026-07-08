using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Menu.DTOs;
using MediatR;

namespace GastroErp.Application.Features.Menu.Commands;

// ─── Category Commands ────────────────────────────────────────────────────────
public record CreateCategoryCommand(CreateCategoryDto Dto) : IRequest<Result<CategoryDto>>;
public record UpdateCategoryCommand(Guid Id, UpdateCategoryDto Dto) : IRequest<Result>;
public record SetCategoryImageCommand(Guid Id, string? ImageUrl) : IRequest<Result>;
public record DeactivateCategoryCommand(Guid Id) : IRequest<Result>;
public record ActivateCategoryCommand(Guid Id) : IRequest<Result>;

// ─── PriceLevel Commands ──────────────────────────────────────────────────────
public record CreatePriceLevelCommand(CreatePriceLevelDto Dto) : IRequest<Result<PriceLevelDto>>;
public record UpdatePriceLevelCommand(Guid Id, UpdatePriceLevelDto Dto) : IRequest<Result>;
public record SetPriceLevelAsDefaultCommand(Guid Id) : IRequest<Result>;
public record DeactivatePriceLevelCommand(Guid Id) : IRequest<Result>;
public record ActivatePriceLevelCommand(Guid Id) : IRequest<Result>;

// ─── Menu Commands ────────────────────────────────────────────────────────────
public record CreateMenuCommand(CreateMenuDto Dto) : IRequest<Result<MenuDto>>;
public record UpdateMenuCommand(Guid Id, UpdateMenuDto Dto) : IRequest<Result>;
public record ActivateMenuCommand(Guid Id) : IRequest<Result>;
public record DeactivateMenuCommand(Guid Id) : IRequest<Result>;

// ─── MenuSection Commands ─────────────────────────────────────────────────────
public record AddMenuSectionCommand(Guid MenuId, AddMenuSectionDto Dto) : IRequest<Result<MenuSectionDto>>;
public record UpdateMenuSectionCommand(Guid MenuId, Guid SectionId, UpdateMenuSectionDto Dto) : IRequest<Result>;
public record RemoveMenuSectionCommand(Guid MenuId, Guid SectionId) : IRequest<Result>;
public record DeactivateMenuSectionCommand(Guid SectionId) : IRequest<Result>;

// ─── MenuItem Commands ────────────────────────────────────────────────────────
public record AddMenuItemCommand(Guid SectionId, AddMenuItemDto Dto) : IRequest<Result>;
public record RemoveMenuItemCommand(Guid SectionId, Guid ProductId) : IRequest<Result>;
public record SetMenuItemOverridePriceCommand(Guid MenuItemId, decimal? Price) : IRequest<Result>;
public record MarkMenuItemOutOfStockCommand(Guid MenuItemId) : IRequest<Result>;
public record MarkMenuItemInStockCommand(Guid MenuItemId) : IRequest<Result>;
public record HideMenuItemCommand(Guid MenuItemId) : IRequest<Result>;
public record ShowMenuItemCommand(Guid MenuItemId) : IRequest<Result>;

// ─── BranchMenu Commands ──────────────────────────────────────────────────────
public record CreateBranchMenuCommand(CreateBranchMenuDto Dto) : IRequest<Result<BranchMenuDto>>;
public record SetBranchMenuPriceLevelCommand(Guid BranchMenuId, Guid? PriceLevelId) : IRequest<Result>;
public record ActivateBranchMenuCommand(Guid BranchMenuId) : IRequest<Result>;
public record DeactivateBranchMenuCommand(Guid BranchMenuId) : IRequest<Result>;
public record SetMenuAvailabilityCommand(Guid BranchMenuId, SetMenuAvailabilityDto Dto) : IRequest<Result>;

// ─── Combo Commands ───────────────────────────────────────────────────────────
public record CreateComboCommand(Guid TenantId, CreateComboDto Dto) : IRequest<Result<ComboDto>>;
public record UpdateComboCommand(Guid Id, UpdateComboDto Dto) : IRequest<Result>;
public record DeactivateComboCommand(Guid Id) : IRequest<Result>;
public record ActivateComboCommand(Guid Id) : IRequest<Result>;
public record AddComboItemCommand(Guid ComboMealId, AddComboItemDto Dto) : IRequest<Result>;
public record RemoveComboItemCommand(Guid ComboMealId, Guid ProductId) : IRequest<Result>;
public record UpdateComboItemCommand(Guid ComboMealId, Guid ProductId, UpdateComboItemDto Dto) : IRequest<Result>;

// ─── Modifier Commands ────────────────────────────────────────────────────────
public record CreateModifierCommand(Guid TenantId, CreateModifierDto Dto) : IRequest<Result<ModifierDto>>;
public record UpdateModifierCommand(Guid Id, UpdateModifierDto Dto) : IRequest<Result>;
