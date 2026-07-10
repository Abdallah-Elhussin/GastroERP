using GastroErp.Application.Features.Catalog.DTOs;
using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Catalog;

internal static class ProductCatalogTypeRegistry
{
    private static readonly string[] AllSteps =
        ["type", "general", "inventory", "recipe", "pos", "pricing", "review"];

    public static ProductCatalogTypeDto Describe(ProductCatalogType type) => type switch
    {
        ProductCatalogType.RawMaterial => new(type, "raw", "مادة خام", "Raw Material", "RAW", true, false, false, false, AllSteps),
        ProductCatalogType.SemiFinished => new(type, "semi", "نصف مصنع", "Semi Finished", "SEM", true, true, false, false, AllSteps),
        ProductCatalogType.FinishedProduct => new(type, "finished", "منتج نهائي", "Finished Product", "PRD", true, true, false, true, AllSteps),
        ProductCatalogType.MenuItem => new(type, "menu", "صنف قائمة", "Menu Item", "MEN", false, true, true, true, AllSteps),
        ProductCatalogType.Combo => new(type, "combo", "وجبة مركّبة", "Combo", "COM", false, false, true, true, Filter("recipe", "inventory")),
        ProductCatalogType.Modifier => new(type, "modifier", "إضافة", "Modifier", "MOD", false, false, true, true, Filter("recipe", "inventory")),
        ProductCatalogType.Bundle => new(type, "bundle", "باقة", "Bundle", "BND", false, false, true, true, Filter("recipe", "inventory")),
        ProductCatalogType.Service => new(type, "service", "خدمة", "Service", "SRV", false, false, true, true, Filter("inventory", "recipe")),
        ProductCatalogType.Voucher => new(type, "voucher", "قسيمة", "Voucher", "VCH", false, false, true, true, Filter("inventory", "recipe", "pos")),
        ProductCatalogType.GiftCard => new(type, "gift", "بطاقة هدايا", "Gift Card", "GFT", false, false, true, true, Filter("inventory", "recipe", "pos")),
        ProductCatalogType.Packaging => new(type, "packaging", "تغليف", "Packaging", "PKG", true, false, false, false, AllSteps),
        ProductCatalogType.Asset => new(type, "asset", "أصل", "Asset", "AST", false, false, false, false, Filter("recipe", "pos", "pricing")),
        ProductCatalogType.Expense => new(type, "expense", "مصروف", "Expense", "EXP", false, false, false, false, Filter("inventory", "recipe", "pos", "pricing")),
        _ => new(type, "unknown", "غير معروف", "Unknown", "UNK", false, false, false, false, AllSteps)
    };

    public static IReadOnlyList<ProductCatalogTypeDto> All() =>
        Enum.GetValues<ProductCatalogType>().Select(Describe).ToList();

    private static IReadOnlyList<string> Filter(params string[] excluded) =>
        AllSteps.Where(s => !excluded.Contains(s)).ToList();
}
