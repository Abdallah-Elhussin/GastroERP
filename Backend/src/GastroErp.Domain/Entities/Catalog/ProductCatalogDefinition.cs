using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Catalog;

/// <summary>
/// تعريف كatalog المنتج — نقطة الدخول الموحّدة دون دمج InventoryItem و Product.
/// يربط لاحقاً بالكيانات في bounded contexts المناسبة.
/// </summary>
public sealed class ProductCatalogDefinition : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public ProductCatalogType CatalogType { get; private set; }
    public string Code { get; private set; }
    public string? Sku { get; private set; }
    public string? Barcode { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public string? ShortDescriptionAr { get; private set; }
    public string? ShortDescriptionEn { get; private set; }
    public string? LongDescriptionAr { get; private set; }
    public string? LongDescriptionEn { get; private set; }
    public string? Keywords { get; private set; }
    public string? Brand { get; private set; }
    public string? TagsJson { get; private set; }
    public string? PrimaryImageUrl { get; private set; }
    public ProductCatalogStatus Status { get; private set; }
    public int WizardStepCompleted { get; private set; }

    public Guid? MenuCategoryId { get; private set; }
    public Guid? InventoryCategoryId { get; private set; }
    public Guid? InventoryItemId { get; private set; }
    public Guid? ProductId { get; private set; }
    public Guid? RecipeId { get; private set; }

    public Guid? BaseUnitId { get; private set; }
    public Guid? DefaultPurchaseUnitId { get; private set; }
    public Guid? DefaultRecipeUnitId { get; private set; }
    public decimal MinStock { get; private set; }
    public decimal MaxStock { get; private set; }
    public decimal SafetyStock { get; private set; }
    public decimal ReorderLevel { get; private set; }
    public decimal ReorderQuantity { get; private set; }
    public InventoryCostingMethod CostingMethod { get; private set; }
    public bool TrackBatch { get; private set; }
    public bool TrackSerial { get; private set; }
    public bool TrackExpiry { get; private set; }
    public bool AllowNegativeStock { get; private set; }

    public decimal RecipeYield { get; private set; } = 1;
    public decimal RecipeWastePercentage { get; private set; }
    public int RecipePreparationTime { get; private set; }
    public string? RecipeInstructions { get; private set; }

    public int PrepTimeMinutes { get; private set; }
    public bool IsAvailableOnPos { get; private set; } = true;
    public bool IsFeaturedOnPos { get; private set; }
    public Guid? KitchenStationId { get; private set; }

    public decimal BasePrice { get; private set; }
    public string Currency { get; private set; } = "SAR";
    public string? PriceLevelsJson { get; private set; }

    public string? SupplierIdsJson { get; private set; }
    public string? MediaUrlsJson { get; private set; }
    public string? VariantAttributesJson { get; private set; }
    public string? RelatedProductsJson { get; private set; }

    private ProductCatalogDefinition() { Code = string.Empty; NameAr = string.Empty; Currency = "SAR"; }

    public ProductCatalogDefinition(Guid tenantId, ProductCatalogType catalogType, string code, string nameAr)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code cannot be empty.", nameof(code));
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);

        TenantId = tenantId;
        CatalogType = catalogType;
        Code = code;
        NameAr = nameAr;
        Status = ProductCatalogStatus.Draft;
        WizardStepCompleted = 1;
    }

    public void UpdateGeneralInfo(
        string nameAr,
        string? nameEn,
        string? shortDescriptionAr,
        string? shortDescriptionEn,
        string? longDescriptionAr,
        string? longDescriptionEn,
        string? keywords,
        string? brand,
        string? tagsJson,
        string? sku,
        string? barcode,
        string? primaryImageUrl,
        Guid? menuCategoryId,
        Guid? inventoryCategoryId)
    {
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        NameAr = nameAr;
        NameEn = nameEn;
        ShortDescriptionAr = shortDescriptionAr;
        ShortDescriptionEn = shortDescriptionEn;
        LongDescriptionAr = longDescriptionAr;
        LongDescriptionEn = longDescriptionEn;
        Keywords = keywords;
        Brand = brand;
        TagsJson = tagsJson;
        Sku = sku;
        Barcode = barcode;
        PrimaryImageUrl = primaryImageUrl;
        MenuCategoryId = menuCategoryId;
        InventoryCategoryId = inventoryCategoryId;
        if (WizardStepCompleted < 2) WizardStepCompleted = 2;
    }

    public void LinkInventoryItem(Guid inventoryItemId)
    {
        if (inventoryItemId == Guid.Empty) throw new ArgumentException("InventoryItemId cannot be empty.");
        InventoryItemId = inventoryItemId;
        SetWizardStep(3);
    }

    public void UpdateInventoryInfo(
        Guid baseUnitId,
        Guid? defaultPurchaseUnitId,
        Guid? defaultRecipeUnitId,
        decimal minStock,
        decimal maxStock,
        decimal safetyStock,
        decimal reorderLevel,
        decimal reorderQuantity,
        InventoryCostingMethod costingMethod,
        bool trackBatch,
        bool trackSerial,
        bool trackExpiry,
        bool allowNegativeStock)
    {
        if (baseUnitId == Guid.Empty) throw new ArgumentException("BaseUnitId cannot be empty.", nameof(baseUnitId));
        BaseUnitId = baseUnitId;
        DefaultPurchaseUnitId = defaultPurchaseUnitId;
        DefaultRecipeUnitId = defaultRecipeUnitId;
        MinStock = minStock;
        MaxStock = maxStock;
        SafetyStock = safetyStock;
        ReorderLevel = reorderLevel;
        ReorderQuantity = reorderQuantity;
        CostingMethod = costingMethod;
        TrackBatch = trackBatch;
        TrackSerial = trackSerial;
        TrackExpiry = trackExpiry;
        AllowNegativeStock = allowNegativeStock;
        SetWizardStep(3);
    }

    public InventoryItemKind ResolveInventoryItemKind() => CatalogType switch
    {
        ProductCatalogType.RawMaterial or ProductCatalogType.Packaging => InventoryItemKind.Raw,
        _ => InventoryItemKind.Manufactured
    };

    public void LinkProduct(Guid productId)
    {
        if (productId == Guid.Empty) throw new ArgumentException("ProductId cannot be empty.");
        ProductId = productId;
    }

    public void LinkRecipe(Guid recipeId)
    {
        if (recipeId == Guid.Empty) throw new ArgumentException("RecipeId cannot be empty.");
        RecipeId = recipeId;
        SetWizardStep(4);
    }

    public void UpdateRecipeInfo(decimal yield, decimal wastePercentage, int preparationTime, string? instructions)
    {
        if (yield <= 0) throw new ArgumentException("Yield must be greater than zero.", nameof(yield));
        RecipeYield = yield;
        RecipeWastePercentage = wastePercentage;
        RecipePreparationTime = preparationTime;
        RecipeInstructions = instructions;
        SetWizardStep(4);
    }

    public void UpdatePosInfo(int prepTimeMinutes, bool isAvailableOnPos, bool isFeaturedOnPos, Guid? kitchenStationId, Guid? menuCategoryId)
    {
        PrepTimeMinutes = prepTimeMinutes >= 0 ? prepTimeMinutes : 0;
        IsAvailableOnPos = isAvailableOnPos;
        IsFeaturedOnPos = isFeaturedOnPos;
        KitchenStationId = kitchenStationId;
        if (menuCategoryId.HasValue) MenuCategoryId = menuCategoryId;
        SetWizardStep(5);
    }

    public void UpdatePricingInfo(decimal basePrice, string currency, string? priceLevelsJson)
    {
        if (basePrice < 0) throw new ArgumentException("BasePrice cannot be negative.", nameof(basePrice));
        BasePrice = basePrice;
        Currency = string.IsNullOrWhiteSpace(currency) ? "SAR" : currency.ToUpperInvariant();
        PriceLevelsJson = priceLevelsJson;
        SetWizardStep(6);
    }

    public void UpdateExtensions(string? supplierIdsJson, string? mediaUrlsJson, string? variantAttributesJson)
    {
        SupplierIdsJson = supplierIdsJson;
        MediaUrlsJson = mediaUrlsJson;
        VariantAttributesJson = variantAttributesJson;
    }

    public void UpdateRelationships(string? relatedProductsJson) => RelatedProductsJson = relatedProductsJson;

    public void SetWizardStep(int step) => WizardStepCompleted = Math.Max(WizardStepCompleted, step);

    public void Activate()
    {
        Status = ProductCatalogStatus.Active;
        SetWizardStep(7);
    }
    public void Archive() => Status = ProductCatalogStatus.Archived;
}
