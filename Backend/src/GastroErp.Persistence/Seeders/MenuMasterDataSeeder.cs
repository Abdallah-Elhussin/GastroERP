using GastroErp.Application.Common.Interfaces;
using GastroErp.Domain.Entities.Menu;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Persistence.Seeders;

/// <summary>تصنيفات المنيو، مستويات التسعير، منتجات نموذجية مع مقاسات/أحجام.</summary>
public sealed class MenuMasterDataSeeder : IDataSeeder
{
    private readonly ILogger<MenuMasterDataSeeder> _logger;

    public MenuMasterDataSeeder(ILogger<MenuMasterDataSeeder> logger) => _logger = logger;

    public int Order => 40;

    public async Task SeedAsync(Guid tenantId, IApplicationDbContext context, CancellationToken ct = default)
    {
        await SeedPriceLevelsAsync(tenantId, context, ct);
        var categoryMap = await SeedCategoriesAsync(tenantId, context, ct);
        await SeedSampleProductsAsync(tenantId, context, categoryMap, ct);
        _logger.LogInformation("Menu master data seeded for tenant {TenantId}", tenantId);
    }

    private static async Task SeedPriceLevelsAsync(Guid tenantId, IApplicationDbContext context, CancellationToken ct)
    {
        if (await context.PriceLevels.AnyAsync(p => p.TenantId == tenantId, ct))
            return;

        context.PriceLevels.Add(new PriceLevel(tenantId, "محلي", SalesChannel.DineIn, "Dine In", isDefault: true));
        context.PriceLevels.Add(new PriceLevel(tenantId, "سفري", SalesChannel.TakeAway, "Take Away"));
        context.PriceLevels.Add(new PriceLevel(tenantId, "توصيل", SalesChannel.Delivery, "Delivery"));
        await context.SaveChangesAsync(ct);
    }

    private static async Task<Dictionary<string, Guid>> SeedCategoriesAsync(
        Guid tenantId, IApplicationDbContext context, CancellationToken ct)
    {
        if (await context.Categories.AnyAsync(c => c.TenantId == tenantId, ct))
        {
            return await context.Categories.Where(c => c.TenantId == tenantId)
                .ToDictionaryAsync(c => c.NameAr, c => c.Id, ct);
        }

        var beverages = new Category(tenantId, "مشروبات", "Beverages", color: "#3498DB", icon: "coffee", sortOrder: 1);
        var mains = new Category(tenantId, "أطباق رئيسية", "Main Dishes", color: "#E74C3C", icon: "utensils", sortOrder: 2);
        var desserts = new Category(tenantId, "حلويات", "Desserts", color: "#9B59B6", icon: "cake", sortOrder: 3);
        var sides = new Category(tenantId, "إضافات", "Sides", color: "#F39C12", icon: "plus", sortOrder: 4);

        context.Categories.AddRange(beverages, mains, desserts, sides);
        await context.SaveChangesAsync(ct);

        return new Dictionary<string, Guid>
        {
            ["مشروبات"] = beverages.Id,
            ["أطباق رئيسية"] = mains.Id,
            ["حلويات"] = desserts.Id,
            ["إضافات"] = sides.Id
        };
    }

    private static async Task SeedSampleProductsAsync(
        Guid tenantId, IApplicationDbContext context, Dictionary<string, Guid> categories, CancellationToken ct)
    {
        if (await context.Products.AnyAsync(p => p.TenantId == tenantId, ct))
            return;

        SeedCoffee(tenantId, categories["مشروبات"], context);
        SeedBurger(tenantId, categories["أطباق رئيسية"], context);
        SeedJuice(tenantId, categories["مشروبات"], context);
        SeedDessert(tenantId, categories["حلويات"], context);

        await context.SaveChangesAsync(ct);
    }

    private static void SeedCoffee(Guid tenantId, Guid categoryId, IApplicationDbContext context)
    {
        var product = new Product(tenantId, categoryId, "قهوة عربية", 12m, nameEn: "Arabic Coffee", sku: "BEV-001");
        product.AddModifierGroup("الحجم", "Size", 1, 1, isRequired: true);
        var size = product.ModifierGroups.First();
        size.AddModifier("صغير", "Small", 0, isDefault: true);
        size.AddModifier("وسط", "Medium", 3m);
        size.AddModifier("كبير", "Large", 6m);
        product.AddOptionGroup("مستوى السكر", "Sugar Level", isRequired: false);
        product.OptionGroups.First().AddOption("بدون سكر", "No Sugar");
        product.OptionGroups.First().AddOption("قليل", "Light");
        product.OptionGroups.First().AddOption("عادي", "Regular", isDefault: true);
        context.Products.Add(product);
    }

    private static void SeedBurger(Guid tenantId, Guid categoryId, IApplicationDbContext context)
    {
        var product = new Product(tenantId, categoryId, "برجر كلاسيك", 28m, nameEn: "Classic Burger", sku: "MAIN-001");
        product.AddModifierGroup("الحجم", "Size", 1, 1, isRequired: true);
        var size = product.ModifierGroups.First();
        size.AddModifier("عادي", "Regular", 0, isDefault: true);
        size.AddModifier("دبل", "Double", 10m);
        size.AddModifier("ثلاثي", "Triple", 18m);
        product.AddModifierGroup("إضافات", "Extras", 0, 5, isRequired: false);
        var extras = product.ModifierGroups.Last();
        extras.AddModifier("جبنة إضافية", "Extra Cheese", 4m);
        extras.AddModifier("بيض", "Egg", 3m);
        extras.AddModifier("مخلل", "Pickles", 1m);
        context.Products.Add(product);
    }

    private static void SeedJuice(Guid tenantId, Guid categoryId, IApplicationDbContext context)
    {
        var product = new Product(tenantId, categoryId, "عصير برتقال طازج", 15m, nameEn: "Fresh Orange Juice", sku: "BEV-002");
        product.AddModifierGroup("الحجم", "Size", 1, 1, isRequired: true);
        var size = product.ModifierGroups.First();
        size.AddModifier("250 مل", "250ml", 0, isDefault: true);
        size.AddModifier("500 مل", "500ml", 5m);
        size.AddModifier("1 لتر", "1 Liter", 12m);
        context.Products.Add(product);
    }

    private static void SeedDessert(Guid tenantId, Guid categoryId, IApplicationDbContext context)
    {
        var product = new Product(tenantId, categoryId, "كنافة", 22m, nameEn: "Kunafa", sku: "DES-001");
        product.AddModifierGroup("الحجم", "Size", 1, 1, isRequired: true);
        var size = product.ModifierGroups.First();
        size.AddModifier("فردي", "Single", 0, isDefault: true);
        size.AddModifier("عائلي", "Family", 35m);
        context.Products.Add(product);
    }
}
