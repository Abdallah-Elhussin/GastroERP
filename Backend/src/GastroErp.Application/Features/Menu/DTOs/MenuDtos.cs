using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Menu.DTOs;

// ─── Category DTOs ───────────────────────────────────────────────────────────

public record CategoryDto(
    Guid Id,
    Guid TenantId,
    Guid? ParentCategoryId,
    string NameAr,
    string? NameEn,
    string? DescriptionAr,
    string? DescriptionEn,
    string? ImageUrl,
    string? Color,
    string? Icon,
    int SortOrder,
    bool IsActive,
    DateTime CreatedAt
);

public record CreateCategoryDto(
    Guid TenantId,
    string NameAr,
    string? NameEn = null,
    Guid? ParentCategoryId = null,
    string? Color = null,
    string? Icon = null,
    int SortOrder = 0
);

public record UpdateCategoryDto(
    string NameAr,
    string? NameEn,
    string? DescriptionAr,
    string? DescriptionEn,
    string? Color,
    string? Icon
);

// ─── PriceLevel DTOs ─────────────────────────────────────────────────────────

public record PriceLevelDto(
    Guid Id,
    Guid TenantId,
    string NameAr,
    string? NameEn,
    SalesChannel SalesChannel,
    bool IsDefault,
    bool IsActive,
    DateTime CreatedAt
);

public record CreatePriceLevelDto(
    Guid TenantId,
    string NameAr,
    SalesChannel SalesChannel,
    string? NameEn = null,
    bool IsDefault = false
);

public record UpdatePriceLevelDto(
    string NameAr,
    string? NameEn,
    SalesChannel SalesChannel
);

// ─── Menu DTOs ───────────────────────────────────────────────────────────────

public record MenuDto(
    Guid Id,
    Guid TenantId,
    string NameAr,
    string? NameEn,
    string? DescriptionAr,
    string? DescriptionEn,
    MenuType MenuType,
    SalesChannel SalesChannel,
    DateOnly? StartDate,
    DateOnly? EndDate,
    bool IsActive,
    int SectionCount,
    DateTime CreatedAt
);

public record CreateMenuDto(
    Guid TenantId,
    string NameAr,
    MenuType MenuType,
    SalesChannel SalesChannel,
    string? NameEn = null,
    DateOnly? StartDate = null,
    DateOnly? EndDate = null
);

public record UpdateMenuDto(
    string NameAr,
    string? NameEn,
    string? DescriptionAr,
    string? DescriptionEn,
    DateOnly? StartDate,
    DateOnly? EndDate
);

// ─── MenuSection DTOs ────────────────────────────────────────────────────────

public record MenuSectionDto(
    Guid Id,
    Guid TenantId,
    Guid MenuId,
    string NameAr,
    string? NameEn,
    string? DescriptionAr,
    string? DescriptionEn,
    string? ImageUrl,
    int SortOrder,
    bool IsActive,
    int ItemCount,
    DateTime CreatedAt
);

public record AddMenuSectionDto(
    string NameAr,
    string? NameEn = null,
    int SortOrder = 0
);

public record UpdateMenuSectionDto(
    string NameAr,
    string? NameEn,
    string? DescriptionAr,
    string? DescriptionEn
);

// ─── MenuItem DTOs ───────────────────────────────────────────────────────────

public record MenuItemDto(
    Guid Id,
    Guid TenantId,
    Guid MenuSectionId,
    Guid ProductId,
    string ProductNameAr,
    string? ProductNameEn,
    decimal? OverridePrice,
    int SortOrder,
    bool IsVisible,
    bool IsOutOfStock
);

public record AddMenuItemDto(
    Guid ProductId,
    decimal? OverridePrice = null,
    int SortOrder = 0
);

// ─── BranchMenu DTOs ─────────────────────────────────────────────────────────

public record BranchMenuDto(
    Guid Id,
    Guid TenantId,
    Guid BranchId,
    Guid MenuId,
    Guid? PriceLevelId,
    bool IsActive,
    int SortOrder,
    int AvailabilityCount,
    DateTime CreatedAt
);

public record CreateBranchMenuDto(
    Guid TenantId,
    Guid BranchId,
    Guid MenuId,
    Guid? PriceLevelId = null
);

public record MenuAvailabilityDto(
    Guid Id,
    BusinessDayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime
);

public record SetMenuAvailabilityDto(
    BusinessDayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime
);

public record ComboDto(
    Guid Id,
    string NameAr,
    string? NameEn,
    string? DescriptionAr,
    string? DescriptionEn,
    decimal ComboPrice,
    string Currency,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? ImageUrl,
    bool IsActive
);

public record CreateComboDto(
    string NameAr,
    decimal ComboPrice,
    string Currency = "SAR",
    string? NameEn = null,
    DateOnly? StartDate = null,
    DateOnly? EndDate = null
);

public record UpdateComboDto(
    string NameAr,
    decimal ComboPrice,
    string Currency = "SAR",
    string? NameEn = null,
    DateOnly? StartDate = null,
    DateOnly? EndDate = null
);

public record CreateModifierDto(
    Guid ModifierGroupId,
    string NameAr,
    string? NameEn,
    decimal ExtraPrice,
    bool IsDefault
);

public record UpdateModifierDto(
    string NameAr,
    string? NameEn,
    decimal ExtraPrice,
    bool IsDefault
);

public record ComboItemDto(
    Guid Id,
    Guid TenantId,
    Guid ComboMealId,
    Guid ProductId,
    string ProductNameAr,
    string? ProductNameEn,
    int Quantity,
    bool AllowSubstitution,
    Guid? SubstitutionCategoryId
);

public record AddComboItemDto(
    Guid ProductId,
    int Quantity,
    bool AllowSubstitution,
    Guid? SubstitutionCategoryId = null
);

public record UpdateComboItemDto(
    int Quantity
);
