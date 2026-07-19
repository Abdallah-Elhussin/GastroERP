namespace GastroErp.Domain.Enums;

/// <summary>فئة نوع الصنف للمطاعم — تحدد السلوك الافتراضي في العمليات.</summary>
public enum InventoryItemTypeCategory : byte
{
    RawMaterial = 1,
    PackagingMaterial = 2,
    FinishedProduct = 3,
    SemiFinishedProduct = 4,
    RecipeComponent = 5,
    MenuItem = 6,
    Bundle = 7,
    Service = 8,
    FixedAsset = 9,
    PromotionItem = 10
}
