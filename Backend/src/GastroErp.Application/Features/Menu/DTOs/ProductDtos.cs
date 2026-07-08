namespace GastroErp.Application.Features.Menu.DTOs;

// ─── Product DTOs ────────────────────────────────────────────────────────────

public record ProductDto(
    Guid Id,
    Guid TenantId,
    Guid CategoryId,
    string CategoryNameAr,
    string? CategoryNameEn,
    string NameAr,
    string? NameEn,
    string? DescriptionAr,
    string? DescriptionEn,
    string? SKU,
    string? Barcode,
    decimal BasePrice,
    string Currency,
    int? CaloriesMin,
    int? CaloriesMax,
    int PrepTimeMinutes,
    bool IsAvailable,
    bool IsFeatured,
    int SortOrder,
    bool HasModifiers,
    int ModifierGroupCount,
    int ImageCount,
    DateTime CreatedAt
);

public record CreateProductDto(
    Guid TenantId,
    Guid CategoryId,
    string NameAr,
    decimal BasePrice,
    string Currency = "SAR",
    string? NameEn = null,
    string? SKU = null
);

public record UpdateProductDto(
    string NameAr,
    string? NameEn,
    string? DescriptionAr,
    string? DescriptionEn,
    string? SKU,
    string? Barcode,
    int? CaloriesMin,
    int? CaloriesMax,
    int PrepTimeMinutes
);

public record UpdateProductPriceDto(decimal NewPrice);

// ─── ModifierGroup DTOs ──────────────────────────────────────────────────────

public record ModifierGroupDto(
    Guid Id,
    Guid TenantId,
    Guid ProductId,
    string NameAr,
    string? NameEn,
    int MinSelection,
    int MaxSelection,
    bool IsRequired,
    int SortOrder,
    bool IsActive,
    List<ModifierDto> Modifiers
);

public record CreateModifierGroupDto(
    Guid ProductId,
    string NameAr,
    string? NameEn,
    int MinSelection,
    int MaxSelection,
    bool IsRequired = false
);

public record ModifierDto(
    Guid Id,
    string NameAr,
    string? NameEn,
    decimal ExtraPrice,
    bool IsDefault,
    bool IsActive
);

public record AddModifierDto(
    string NameAr,
    string? NameEn,
    decimal ExtraPrice = 0,
    bool IsDefault = false
);

// ─── OptionGroup DTOs ────────────────────────────────────────────────────────

public record OptionGroupDto(
    Guid Id,
    Guid TenantId,
    Guid ProductId,
    string NameAr,
    string? NameEn,
    bool IsRequired,
    bool IsActive,
    List<OptionDto> Options
);

public record CreateOptionGroupDto(
    Guid ProductId,
    string NameAr,
    string? NameEn,
    bool IsRequired = false
);

public record OptionDto(
    Guid Id,
    string NameAr,
    string? NameEn,
    decimal ExtraPrice,
    bool IsDefault,
    bool IsActive
);

public record AddOptionDto(
    string NameAr,
    string? NameEn,
    decimal ExtraPrice = 0,
    bool IsDefault = false
);

public record UpdateOptionGroupDto(
    string NameAr,
    string? NameEn,
    bool IsRequired
);

public record UpdateOptionDto(
    string NameAr,
    string? NameEn,
    decimal ExtraPrice,
    bool IsDefault
);

public record ProductPriceLevelDto(
    Guid ProductId,
    Guid PriceLevelId,
    decimal Price
);

public record ProductImageDto(
    Guid Id,
    Guid ProductId,
    string ImageUrl,
    string? ThumbnailUrl,
    string? AltText,
    bool IsPrimary,
    int SortOrder
);
