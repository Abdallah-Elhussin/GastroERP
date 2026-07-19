using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Catalog.DTOs;

public record ProductCatalogTypeDto(
    ProductCatalogType Type,
    string Code,
    string NameAr,
    string NameEn,
    string Prefix,
    bool RequiresInventory,
    bool RequiresRecipe,
    bool RequiresProduct,
    bool RequiresPricing,
    IReadOnlyList<string> WizardSteps
);

public record ProductCatalogDefinitionDto(
    Guid Id,
    Guid TenantId,
    ProductCatalogType CatalogType,
    string Code,
    string? Sku,
    string? Barcode,
    string NameAr,
    string? NameEn,
    string? ShortDescriptionAr,
    string? ShortDescriptionEn,
    string? LongDescriptionAr,
    string? LongDescriptionEn,
    string? Keywords,
    string? Brand,
    string? TagsJson,
    string? PrimaryImageUrl,
    ProductCatalogStatus Status,
    int WizardStepCompleted,
    Guid? MenuCategoryId,
    Guid? InventoryCategoryId,
    Guid? InventoryItemId,
    Guid? ProductId,
    Guid? RecipeId,
    Guid? BaseUnitId,
    Guid? DefaultPurchaseUnitId,
    Guid? DefaultRecipeUnitId,
    decimal MinStock,
    decimal MaxStock,
    decimal SafetyStock,
    decimal ReorderLevel,
    decimal ReorderQuantity,
    InventoryCostingMethod CostingMethod,
    bool TrackBatch,
    bool TrackSerial,
    bool TrackExpiry,
    bool AllowNegativeStock,
    decimal RecipeYield,
    decimal RecipeWastePercentage,
    int RecipePreparationTime,
    string? RecipeInstructions,
    IReadOnlyList<CatalogRecipeIngredientDto> RecipeIngredients,
    int PrepTimeMinutes,
    bool IsAvailableOnPos,
    bool IsFeaturedOnPos,
    Guid? KitchenStationId,
    decimal BasePrice,
    string Currency,
    IReadOnlyList<CatalogPriceLevelLineDto> PriceLevels,
    IReadOnlyList<Guid> SupplierIds,
    IReadOnlyList<string> MediaUrls,
    string? VariantAttributesJson,
    IReadOnlyList<CatalogRelationshipDto> RelatedProducts,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CatalogRecipeIngredientDto(
    Guid InventoryItemId,
    string? ItemNameAr,
    Guid UnitId,
    string? UnitNameAr,
    decimal Quantity,
    decimal WastePercentage
);

public record CatalogPriceLevelLineDto(Guid PriceLevelId, string? PriceLevelName, decimal Price);

public record CatalogRelationshipDto(Guid TargetCatalogId, string RelationshipType, string? TargetNameAr);

public record SaveCatalogRecipeDto(
    decimal Yield,
    decimal WastePercentage,
    int PreparationTime,
    string? Instructions,
    IReadOnlyList<CatalogRecipeIngredientDto> Ingredients
);

public record SaveCatalogPosDto(
    Guid MenuCategoryId,
    int PrepTimeMinutes,
    bool IsAvailableOnPos,
    bool IsFeaturedOnPos,
    Guid? KitchenStationId
);

public record SaveCatalogPricingDto(
    decimal BasePrice,
    string Currency,
    IReadOnlyList<CatalogPriceLevelLineDto> PriceLevels
);

public record SaveCatalogExtensionsDto(
    IReadOnlyList<Guid> SupplierIds,
    IReadOnlyList<string> MediaUrls,
    string? VariantAttributesJson
);

public record SaveCatalogRelationshipsDto(
    IReadOnlyList<CatalogRelationshipDto> RelatedProducts
);

public record CatalogAuditEntryDto(
    string EventType,
    string Description,
    DateTime OccurredAt,
    string? Actor
);

public record CatalogPriceHistoryDto(
    Guid Id,
    decimal PreviousPrice,
    decimal CurrentPrice,
    string Currency,
    string? PriceLevelName,
    DateTime EffectiveDate,
    string? Actor
);

public record CatalogImportRowDto(
    ProductCatalogType CatalogType,
    string NameAr,
    string? NameEn,
    string? Sku,
    string? Barcode,
    decimal BasePrice
);

public record CreateCatalogDraftDto(
    ProductCatalogType CatalogType,
    string NameAr,
    string? NameEn = null
);

public record UpdateCatalogGeneralInfoDto(
    string NameAr,
    string? NameEn,
    string? ShortDescriptionAr,
    string? ShortDescriptionEn,
    string? LongDescriptionAr,
    string? LongDescriptionEn,
    string? Keywords,
    string? Brand,
    string? TagsJson,
    string? Sku,
    string? Barcode,
    string? PrimaryImageUrl,
    Guid? MenuCategoryId,
    Guid? InventoryCategoryId
);

public record SaveCatalogInventoryDto(
    Guid BaseUnitId,
    Guid? DefaultPurchaseUnitId,
    Guid? DefaultRecipeUnitId,
    decimal MinStock,
    decimal MaxStock,
    decimal SafetyStock,
    decimal ReorderLevel,
    decimal ReorderQuantity,
    InventoryCostingMethod CostingMethod,
    bool TrackBatch,
    bool TrackSerial,
    bool TrackExpiry,
    bool AllowNegativeStock
);
