using GastroErp.Application.Common.Interfaces;
using GastroErp.Domain.Entities.Inventory.Catalog;
using GastroErp.Domain.Entities.Inventory.Counting;
using GastroErp.Domain.Entities.Inventory.Warehouse;
using GastroErp.Domain.Entities.Organization;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Persistence.Seeders;

public sealed class OrganizationMasterDataSeeder : IDataSeeder
{
    private readonly ILogger<OrganizationMasterDataSeeder> _logger;

    public OrganizationMasterDataSeeder(ILogger<OrganizationMasterDataSeeder> logger) => _logger = logger;

    public int Order => 10;

    public async Task SeedAsync(Guid tenantId, IApplicationDbContext context, CancellationToken ct = default)
    {
        // Company, branch, and warehouses are provisioned by the restaurant onboarding service.
        _logger.LogInformation("Organization master data seeded for tenant {TenantId}", tenantId);
        await Task.CompletedTask;
    }
}

public sealed class InventoryMasterDataSeeder : IDataSeeder
{
    private readonly ILogger<InventoryMasterDataSeeder> _logger;

    public InventoryMasterDataSeeder(ILogger<InventoryMasterDataSeeder> logger) => _logger = logger;

    public int Order => 30;

    public async Task SeedAsync(Guid tenantId, IApplicationDbContext context, CancellationToken ct = default)
    {
        await SeedUnitsAsync(tenantId, context, ct);
        await SeedCategoriesAsync(tenantId, context, ct);
        await SeedItemTypesAsync(tenantId, context, ct);
        await SeedWarehouseTypesAsync(tenantId, context, ct);
        await SeedValuationGroupsAsync(tenantId, context, ct);
        await SeedSalesPriceListsAsync(tenantId, context, ct);
        await SeedAdjustmentReasonsAsync(tenantId, context, ct);
        await SeedIssueDestinationsAsync(tenantId, context, ct);
        _logger.LogInformation("Inventory master data seeded for tenant {TenantId}", tenantId);
    }

    private static async Task SeedValuationGroupsAsync(Guid tenantId, IApplicationDbContext context, CancellationToken ct)
    {
        var defaults = new (string Code, string NameAr, string NameEn, string Description, int Sort)[]
        {
            ("VG-DAIRY", "الألبان والأجبان", "Dairy & Cheese", "مجموعة تقييم منتجات الألبان", 1),
            ("VG-PERSONAL", "العناية الشخصية", "Personal Care", "مجموعة تقييم منتجات العناية الشخصية", 2),
            ("VG-FROZEN", "المجمدات", "Frozen", "مجموعة تقييم المنتجات المجمدة", 3),
            ("VG-BAKERY", "المخبوزات", "Bakery", "مجموعة تقييم المخبوزات", 4),
            ("VG-BEVERAGE", "المشروبات", "Beverages", "مجموعة تقييم المشروبات", 5),
            ("VG-MEAT", "اللحوم والدواجن", "Meat & Poultry", "مجموعة تقييم اللحوم والدواجن", 6),
            ("VG-PRODUCE", "الخضار والفواكه", "Produce", "مجموعة تقييم الخضار والفواكه", 7),
            ("VG-DRY", "المواد الجافة", "Dry Goods", "مجموعة تقييم المواد الجافة", 8),
            ("VG-PACKAGING", "مواد التغليف", "Packaging", "مجموعة تقييم مواد التغليف", 9),
            ("VG-CLEANING", "مواد النظافة", "Cleaning", "مجموعة تقييم مواد النظافة", 10),
        };

        var existing = await context.InventoryValuationGroups
            .Where(t => t.TenantId == tenantId)
            .Select(t => t.Code)
            .ToListAsync(ct);
        var set = existing.ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var d in defaults)
        {
            if (set.Contains(d.Code)) continue;
            context.InventoryValuationGroups.Add(new InventoryValuationGroup(
                tenantId, d.Code, d.NameAr, d.NameEn, d.Description, null, d.Sort, isSystem: true));
        }

        await context.SaveChangesAsync(ct);
    }

    private static async Task SeedSalesPriceListsAsync(Guid tenantId, IApplicationDbContext context, CancellationToken ct)
    {
        var defaults = new (string Code, string NameAr, string NameEn, SalesChannel? Channel, int Sort, bool IsDefault)[]
        {
            ("RETAIL", "تجزئة", "Retail", SalesChannel.All, 1, true),
            ("DINEIN", "صالة", "Dine In", SalesChannel.DineIn, 2, false),
            ("TAKEAWAY", "سفري", "Take Away", SalesChannel.TakeAway, 3, false),
            ("DELIVERY", "توصيل", "Delivery", SalesChannel.Delivery, 4, false),
            ("ONLINE", "طلبات أونلاين", "Online Orders", SalesChannel.Online, 5, false),
            ("STAFF", "موظفين", "Staff", SalesChannel.All, 6, false),
            ("VIP", "VIP", "VIP", SalesChannel.All, 7, false),
            ("WHOLESALE", "جملة", "Wholesale", SalesChannel.All, 8, false),
            ("HAPPYHOUR", "ساعة سعيدة", "Happy Hour", SalesChannel.DineIn, 9, false),
            ("SEASONAL", "عرض موسمي", "Seasonal Promotion", SalesChannel.All, 10, false),
        };

        var existing = await context.SalesPriceLists
            .Where(t => t.TenantId == tenantId)
            .Select(t => t.Code)
            .ToListAsync(ct);
        var set = existing.ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var d in defaults)
        {
            if (set.Contains(d.Code)) continue;
            context.SalesPriceLists.Add(new GastroErp.Domain.Entities.Sales.Pricing.SalesPriceList(
                tenantId, d.Code, d.NameAr, d.NameEn, null, d.Channel, d.Sort, d.IsDefault, isSystem: true));
        }

        await context.SaveChangesAsync(ct);
    }

    private static async Task SeedWarehouseTypesAsync(Guid tenantId, IApplicationDbContext context, CancellationToken ct)
    {
        var defaults = new (string Code, string NameAr, string NameEn, int Sort)[]
        {
            ("MAIN", "رئيسي", "Main", 1),
            ("KITCHEN", "مطبخ", "Kitchen", 2),
            ("BEVERAGE", "مشروبات", "Beverage", 3),
            ("DRYSTORE", "مواد جافة", "Dry Store", 4),
            ("CHILLER", "تبريد", "Chiller", 5),
            ("FREEZER", "تجميد", "Freezer", 6),
            ("PACKAGING", "تغليف", "Packaging", 7),
            ("CLEANING", "نظافة", "Cleaning", 8),
            ("PRODUCTION", "إنتاج", "Production", 9),
            ("TRANSIT", "تحويلات", "Transit", 10),
            ("WASTE", "هدر", "Waste", 11),
            ("POS", "نقطة بيع", "POS", 12),
        };

        var existing = await context.WarehouseTypeDefinitions
            .Where(t => t.TenantId == tenantId)
            .Select(t => t.Code)
            .ToListAsync(ct);
        var set = existing.ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var d in defaults)
        {
            if (set.Contains(d.Code)) continue;
            context.WarehouseTypeDefinitions.Add(new WarehouseTypeDefinition(
                tenantId, d.Code, d.NameAr, d.NameEn, null, d.Sort, isSystem: true));
        }

        await context.SaveChangesAsync(ct);
    }

    private static async Task SeedItemTypesAsync(Guid tenantId, IApplicationDbContext context, CancellationToken ct)
    {
        if (await context.InventoryItemTypes.AnyAsync(t => t.TenantId == tenantId, ct))
            return;

        var defaults = new (string Code, string NameAr, string NameEn, InventoryItemTypeCategory Category,
            int Start, int End, bool Inv, bool Sell, bool Purch, bool Recipe, bool Prod, int Sort)[]
        {
            ("RAW", "مواد خام", "Raw Material", InventoryItemTypeCategory.RawMaterial, 1000, 1999, true, false, true, true, false, 1),
            ("PKG", "مواد تغليف", "Packaging Material", InventoryItemTypeCategory.PackagingMaterial, 2000, 2999, true, false, true, false, false, 2),
            ("FIN", "منتج تام", "Finished Product", InventoryItemTypeCategory.FinishedProduct, 3000, 3999, true, true, true, false, true, 3),
            ("SEMI", "منتج نصف مصنع", "Semi Finished Product", InventoryItemTypeCategory.SemiFinishedProduct, 4000, 4999, true, false, false, true, true, 4),
            ("RCMP", "مكون وصفة", "Recipe Component", InventoryItemTypeCategory.RecipeComponent, 5000, 5999, true, false, false, true, false, 5),
            ("MENU", "عنصر قائمة", "Menu Item", InventoryItemTypeCategory.MenuItem, 6000, 6999, false, true, false, false, false, 6),
            ("BNDL", "حزمة / كومبو", "Bundle", InventoryItemTypeCategory.Bundle, 7000, 7999, false, true, false, false, false, 7),
            ("SVC", "خدمة", "Service", InventoryItemTypeCategory.Service, 8000, 8099, false, true, false, false, false, 8),
            ("ASSET", "أصل ثابت", "Fixed Asset", InventoryItemTypeCategory.FixedAsset, 9000, 9999, true, false, true, false, false, 9),
            ("PROMO", "عرض ترويجي", "Promotion Item", InventoryItemTypeCategory.PromotionItem, 10000, 10999, false, false, false, false, false, 10),
        };

        foreach (var d in defaults)
        {
            context.InventoryItemTypes.Add(new InventoryItemType(
                tenantId,
                d.Code,
                d.NameAr,
                d.Category,
                d.NameEn,
                description: null,
                d.Start,
                d.End,
                d.Inv,
                d.Sell,
                d.Purch,
                d.Recipe,
                d.Prod,
                allowNegativeStock: false,
                color: InventoryItemType.DefaultColor(d.Category),
                sortOrder: d.Sort,
                isSystem: true));
        }

        await context.SaveChangesAsync(ct);
    }

    private static async Task SeedUnitsAsync(Guid tenantId, IApplicationDbContext context, CancellationToken ct)
    {
        var existing = await context.InventoryUnits.Where(u => u.TenantId == tenantId).ToListAsync(ct);
        var byCode = existing.ToDictionary(u => u.Code, StringComparer.OrdinalIgnoreCase);

        Guid Ensure(
            string code,
            string nameAr,
            string nameEn,
            string symbol,
            int number,
            InventoryUnitClassification classification,
            InventoryUnitType unitType = InventoryUnitType.Measured,
            string? baseCode = null,
            decimal factor = 1m,
            byte decimals = 2)
        {
            if (byCode.TryGetValue(code, out var found))
                return found.Id;

            Guid? baseId = null;
            if (!string.IsNullOrWhiteSpace(baseCode) && byCode.TryGetValue(baseCode, out var baseUnit))
                baseId = baseUnit.Id;

            var unit = new InventoryUnit(
                tenantId,
                nameAr,
                symbol,
                nameEn,
                nameAr,
                code,
                decimals,
                baseId,
                factor,
                unitType,
                classification,
                number);
            context.InventoryUnits.Add(unit);
            byCode[code] = unit;
            return unit.Id;
        }

        // Base units
        Ensure("KG", "كيلوجرام", "Kilogram", "kg", 1, InventoryUnitClassification.Weight, decimals: 3);
        Ensure("G", "جرام", "Gram", "g", 2, InventoryUnitClassification.Weight, decimals: 0);
        Ensure("L", "لتر", "Liter", "L", 3, InventoryUnitClassification.Volume, decimals: 3);
        Ensure("ML", "ملليلتر", "Milliliter", "ml", 4, InventoryUnitClassification.Volume, decimals: 0);
        Ensure("M", "متر", "Meter", "m", 5, InventoryUnitClassification.Length, decimals: 2);
        Ensure("PC", "حبة", "Piece", "pc", 6, InventoryUnitClassification.Count, InventoryUnitType.Count, decimals: 0);
        Ensure("BOX", "صندوق", "Box", "box", 7, InventoryUnitClassification.Packaging, InventoryUnitType.Count, decimals: 0);
        Ensure("TRAY", "صينية", "Tray", "tray", 8, InventoryUnitClassification.Packaging, InventoryUnitType.Count, decimals: 0);
        Ensure("PORTION", "حصة", "Portion", "portion", 9, InventoryUnitClassification.Count, InventoryUnitType.Count, decimals: 2);
        Ensure("BOTTLE", "زجاجة", "Bottle", "bottle", 10, InventoryUnitClassification.Packaging, InventoryUnitType.Count, decimals: 0);
        Ensure("CUP", "كوب", "Cup", "cup", 11, InventoryUnitClassification.Volume, decimals: 2);

        await context.SaveChangesAsync(ct);
        existing = await context.InventoryUnits.Where(u => u.TenantId == tenantId).ToListAsync(ct);
        byCode = existing.ToDictionary(u => u.Code, StringComparer.OrdinalIgnoreCase);

        // Measured pack sizes (from desktop catalog)
        Ensure("L1", "1 لتر", "1 Liter", "L1", 47, InventoryUnitClassification.Volume, baseCode: "L", factor: 1m);
        Ensure("L1_5", "1.5 لتر", "1.5 Liter", "L1_5", 48, InventoryUnitClassification.Volume, baseCode: "L", factor: 1.5m);
        Ensure("L18", "جركن 18 لتر", "18L Jerrycan", "L18", 49, InventoryUnitClassification.Volume, baseCode: "L", factor: 18m);
        Ensure("ML200", "200 مل", "200 ml", "ML200", 50, InventoryUnitClassification.Volume, baseCode: "ML", factor: 200m);
        Ensure("ML250", "250 مل", "250 ml", "ML250", 51, InventoryUnitClassification.Volume, baseCode: "ML", factor: 250m);
        Ensure("ML500", "500 مل", "500 ml", "ML500", 52, InventoryUnitClassification.Volume, baseCode: "ML", factor: 500m);
        Ensure("KG2", "2 كجم", "2 kg", "KG2", 37, InventoryUnitClassification.Weight, baseCode: "KG", factor: 2m);
        Ensure("KG5", "5 كجم", "5 kg", "KG5", 38, InventoryUnitClassification.Weight, baseCode: "KG", factor: 5m);
        Ensure("KG10", "10 كجم", "10 kg", "KG10", 39, InventoryUnitClassification.Weight, baseCode: "KG", factor: 10m);
        Ensure("KG25", "25 كجم", "25 kg", "KG25", 40, InventoryUnitClassification.Weight, baseCode: "KG", factor: 25m);
        Ensure("KG50", "50 كجم", "50 kg", "KG50", 41, InventoryUnitClassification.Weight, baseCode: "KG", factor: 50m);
        Ensure("G250", "250 جرام", "250 g", "G250", 42, InventoryUnitClassification.Weight, baseCode: "G", factor: 250m);
        Ensure("G500", "500 جرام", "500 g", "G500", 43, InventoryUnitClassification.Weight, baseCode: "G", factor: 500m);
        Ensure("M10", "10 متر", "10 m", "M10", 44, InventoryUnitClassification.Length, baseCode: "M", factor: 10m);
        Ensure("M100", "100 متر", "100 m", "M100", 45, InventoryUnitClassification.Length, baseCode: "M", factor: 100m);

        await context.SaveChangesAsync(ct);

        // Ensure classic conversions exist
        if (!await context.UnitConversions.AnyAsync(c => c.TenantId == tenantId, ct))
        {
            if (byCode.TryGetValue("G", out var g) && byCode.TryGetValue("KG", out var kg))
                context.UnitConversions.Add(new UnitConversion(tenantId, g.Id, kg.Id, 0.001m));
            if (byCode.TryGetValue("ML", out var ml) && byCode.TryGetValue("L", out var liter))
                context.UnitConversions.Add(new UnitConversion(tenantId, ml.Id, liter.Id, 0.001m));
            if (byCode.TryGetValue("BOX", out var box) && byCode.TryGetValue("PC", out var pc))
                context.UnitConversions.Add(new UnitConversion(tenantId, box.Id, pc.Id, 12m));
            await context.SaveChangesAsync(ct);
        }
    }

    private static async Task SeedCategoriesAsync(Guid tenantId, IApplicationDbContext context, CancellationToken ct)
    {
        // Idempotent restaurant catalog: create missing codes only.
        var existing = await context.InventoryCategories
            .Where(c => c.TenantId == tenantId)
            .ToListAsync(ct);
        var byCode = existing.ToDictionary(c => c.Code, StringComparer.OrdinalIgnoreCase);

        Guid Ensure(
            string code,
            string nameAr,
            string nameEn,
            int number,
            string? parentCode = null,
            string? descriptionAr = null)
        {
            if (byCode.TryGetValue(code, out var found))
                return found.Id;

            Guid? parentId = null;
            if (!string.IsNullOrWhiteSpace(parentCode) && byCode.TryGetValue(parentCode, out var parent))
                parentId = parent.Id;

            var cat = new InventoryCategory(tenantId, nameAr, nameEn, parentId, code);
            cat.UpdateInfo(nameAr, nameEn, descriptionAr, null, code);
            cat.SetSortOrder(number);
            context.InventoryCategories.Add(cat);
            byCode[code] = cat;
            return cat.Id;
        }

        // ── Parents ──────────────────────────────────────────────────────────
        Ensure("DAIRY", "الألبان والأجبان", "Dairy & Cheese", 5, null, "منتجات الألبان");
        Ensure("BABY", "مستلزمات الأطفال", "Baby Supplies", 10);
        Ensure("CLEAN", "مستلزمات التنظيف", "Cleaning Supplies", 15);
        Ensure("FOOD", "المواد الغذائية", "Food Supplies", 30);
        Ensure("FRESH", "المنتجات الطازجة", "Fresh Products", 36);
        Ensure("FROZEN", "المجمدات", "Frozen Products", 45);
        Ensure("BEVERAGE", "المشروبات", "Beverages", 50);
        Ensure("PACKAGING", "مواد التغليف", "Packaging", 55);
        Ensure("MEAT", "اللحوم", "Meat", 60);
        Ensure("SPICES", "البهارات", "Spices", 65);

        // Flush parents so children can resolve ParentCategoryId from tracked entities.
        await context.SaveChangesAsync(ct);
        existing = await context.InventoryCategories.Where(c => c.TenantId == tenantId).ToListAsync(ct);
        byCode = existing.ToDictionary(c => c.Code, StringComparer.OrdinalIgnoreCase);

        // ── Children (from restaurant desktop catalog) ────────────────────────
        Ensure("BABY-FOOD", "أغذية الأطفال", "Baby Food", 13, "BABY");
        Ensure("CLEAN-BAGS", "أكياس القمامة", "Garbage Bags", 20, "CLEAN");
        Ensure("DAIRY-CHEESE", "الأجبان", "Cheese", 25, "DAIRY");
        Ensure("DAIRY-EGGS", "البيض", "Eggs", 27, "DAIRY");
        Ensure("DAIRY-MILK", "الحليب", "Milk", 29, "DAIRY");
        Ensure("DAIRY-YOGURT", "الزبادي", "Yogurt", 28, "DAIRY");
        Ensure("FOOD-DATES", "التمور", "Dates", 32, "FOOD");
        Ensure("FOOD-OIL", "الزيوت والسمن", "Oils & Ghee", 33, "FOOD");
        Ensure("FOOD-RICE", "الأرز والحبوب", "Rice & Grains", 35, "FOOD");
        Ensure("FOOD-SPICE", "البهارات والتوابل", "Spices & Seasonings", 38, "FOOD");
        Ensure("FOOD-SUGAR", "السكر والحلويات", "Sugar & Sweets", 39, "FOOD");
        Ensure("FOOD-FLOUR", "الدقيق والنشا", "Flour & Starch", 34, "FOOD");
        Ensure("FOOD-CANNED", "المعلبات", "Canned Goods", 37, "FOOD");
        Ensure("FRESH-CHICKEN", "الدواجن", "Poultry", 40, "FRESH");
        Ensure("FRESH-FISH", "الأسماك", "Fish", 41, "FRESH");
        Ensure("FRESH-VEG", "الخضروات", "Vegetables", 44, "FRESH");
        Ensure("FRESH-FRUIT", "الفواكه", "Fruits", 43, "FRESH");
        Ensure("FRESH-HERBS", "الأعشاب الطازجة", "Fresh Herbs", 42, "FRESH");
        Ensure("FROZEN-FISH", "أسماك مجمدة", "Frozen Fish", 47, "FROZEN");
        Ensure("FROZEN-ICECREAM", "آيس كريم", "Ice Cream", 48, "FROZEN");
        Ensure("FROZEN-VEG", "خضروات مجمدة", "Frozen Vegetables", 46, "FROZEN");
        Ensure("FROZEN-MEAT", "لحوم مجمدة", "Frozen Meat", 49, "FROZEN");
        Ensure("BEV-JUICE", "عصائر", "Juices", 51, "BEVERAGE");
        Ensure("BEV-SOFT", "مشروبات غازية", "Soft Drinks", 52, "BEVERAGE");
        Ensure("BEV-WATER", "مياه", "Water", 53, "BEVERAGE");
        Ensure("PKG-BOX", "علب وصناديق", "Boxes", 56, "PACKAGING");
        Ensure("PKG-CUP", "أكواب وأغطية", "Cups & Lids", 57, "PACKAGING");
        Ensure("PKG-BAG", "أكياس تغليف", "Packaging Bags", 58, "PACKAGING");
        Ensure("MEAT-BEEF", "لحم بقري", "Beef", 61, "MEAT");
        Ensure("MEAT-LAMB", "لحم ضأن", "Lamb", 62, "MEAT");

        await context.SaveChangesAsync(ct);
    }

    private static async Task SeedIssueDestinationsAsync(Guid tenantId, IApplicationDbContext context, CancellationToken ct)
    {
        var defaults = new (string Code, string NameAr, string NameEn, IssueDestinationType Type, int Sort)[]
        {
            ("KIT", "المطبخ", "Kitchen", IssueDestinationType.Kitchen, 1),
            ("BAR", "البار", "Bar", IssueDestinationType.Kitchen, 2),
            ("PRD", "الإنتاج", "Production", IssueDestinationType.Production, 3),
            ("BRN", "فرع آخر", "Other Branch", IssueDestinationType.Branch, 4),
            ("ADM", "الإدارة", "Administration", IssueDestinationType.Administration, 5),
            ("MKT", "التسويق", "Marketing", IssueDestinationType.Marketing, 6),
            ("MNT", "الصيانة", "Maintenance", IssueDestinationType.Maintenance, 7),
            ("WST", "الهدر", "Waste", IssueDestinationType.Waste, 8),
            ("STM", "وجبات الموظفين", "Staff Meals", IssueDestinationType.StaffMeals, 9),
            ("CMP", "ضيافة مجانية", "Complimentary", IssueDestinationType.Complimentary, 10),
            ("AST", "الأصول", "Assets", IssueDestinationType.Assets, 11),
            ("OTH", "أخرى", "Other", IssueDestinationType.Other, 12),
        };

        var existing = await context.IssueDestinations
            .Where(d => d.TenantId == tenantId)
            .Select(d => d.Code)
            .ToListAsync(ct);
        var set = existing.ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var d in defaults)
        {
            if (set.Contains(d.Code)) continue;
            context.IssueDestinations.Add(new GastroErp.Domain.Entities.Inventory.Issuing.IssueDestination(
                tenantId,
                d.Code,
                d.NameAr,
                d.Type,
                d.NameEn,
                sortOrder: d.Sort,
                isSystem: true,
                allowDirectIssue: true));
        }

        await context.SaveChangesAsync(ct);
    }

    private static async Task SeedAdjustmentReasonsAsync(Guid tenantId, IApplicationDbContext context, CancellationToken ct)
    {
        if (await context.AdjustmentReasons.AnyAsync(r => r.TenantId == tenantId, ct))
            return;

        var reasons = new (string Ar, string En)[]
        {
            ("تلف", "Damage"),
            ("هدر", "Waste"),
            ("انتهاء صلاحية", "Expired"),
            ("تسوية جرد", "Adjustment")
        };

        foreach (var (ar, en) in reasons)
            context.AdjustmentReasons.Add(new AdjustmentReason(tenantId, ar, en));

        await context.SaveChangesAsync(ct);
    }
}
