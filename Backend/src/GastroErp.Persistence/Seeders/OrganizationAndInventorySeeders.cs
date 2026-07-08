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
        Company? company = await context.Companies.FirstOrDefaultAsync(c => c.TenantId == tenantId, ct);
        if (company is null)
        {
            company = new Company(tenantId, "مطعم جاسترو الرئيسي", "300000000000003", "Gastro Main Restaurant");
            context.Companies.Add(company);
            await context.SaveChangesAsync(ct);
        }

        if (!await context.Branches.AnyAsync(b => b.TenantId == tenantId, ct))
        {
            var branch = new Branch(tenantId, company.Id, "الفرع الرئيسي", BranchType.Restaurant, "Main Branch", "BR-001");
            branch.SetAsDefault();
            context.Branches.Add(branch);
            await context.SaveChangesAsync(ct);

            if (!await context.Warehouses.AnyAsync(w => w.TenantId == tenantId, ct))
            {
                var warehouse = new Warehouse(tenantId, "المستودع الرئيسي", "Main Warehouse", "WH-001", branch.Id);
                warehouse.AddZone("منطقة التبريد", "Cold Zone", "COLD");
                warehouse.AddZone("منطقة جافة", "Dry Zone", "DRY");
                context.Warehouses.Add(warehouse);
            }
        }

        await context.SaveChangesAsync(ct);
        _logger.LogInformation("Organization master data seeded for tenant {TenantId}", tenantId);
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
        await SeedAdjustmentReasonsAsync(tenantId, context, ct);
        _logger.LogInformation("Inventory master data seeded for tenant {TenantId}", tenantId);
    }

    private static async Task SeedUnitsAsync(Guid tenantId, IApplicationDbContext context, CancellationToken ct)
    {
        if (await context.InventoryUnits.AnyAsync(u => u.TenantId == tenantId, ct))
            return;

        var units = new (string Symbol, string NameAr, string NameEn)[]
        {
            ("kg", "كيلوجرام", "Kilogram"),
            ("g", "جرام", "Gram"),
            ("L", "لتر", "Liter"),
            ("ml", "مليلتر", "Milliliter"),
            ("pc", "حبة", "Piece"),
            ("box", "صندوق", "Box"),
            ("tray", "صينية", "Tray"),
            ("portion", "حصة", "Portion")
        };

        var map = new Dictionary<string, Guid>();
        foreach (var (symbol, nameAr, nameEn) in units)
        {
            var unit = new InventoryUnit(tenantId, nameAr, symbol, nameEn, nameAr);
            context.InventoryUnits.Add(unit);
            map[symbol] = unit.Id;
        }
        await context.SaveChangesAsync(ct);

        // Re-load IDs after save
        var saved = await context.InventoryUnits.Where(u => u.TenantId == tenantId).ToListAsync(ct);
        var bySymbol = saved.ToDictionary(u => u.Symbol, u => u.Id);

        if (!await context.UnitConversions.AnyAsync(c => c.TenantId == tenantId, ct))
        {
            context.UnitConversions.Add(new UnitConversion(tenantId, bySymbol["g"], bySymbol["kg"], 0.001m));
            context.UnitConversions.Add(new UnitConversion(tenantId, bySymbol["ml"], bySymbol["L"], 0.001m));
            context.UnitConversions.Add(new UnitConversion(tenantId, bySymbol["box"], bySymbol["pc"], 12m));
            await context.SaveChangesAsync(ct);
        }
    }

    private static async Task SeedCategoriesAsync(Guid tenantId, IApplicationDbContext context, CancellationToken ct)
    {
        if (await context.InventoryCategories.AnyAsync(c => c.TenantId == tenantId, ct))
            return;

        var meats = new InventoryCategory(tenantId, "لحوم", "Meats");
        var vegetables = new InventoryCategory(tenantId, "خضروات", "Vegetables");
        var dairy = new InventoryCategory(tenantId, "ألبان", "Dairy");
        var packaging = new InventoryCategory(tenantId, "تغليف", "Packaging");
        var beverages = new InventoryCategory(tenantId, "مشروبات خام", "Beverage Ingredients");

        context.InventoryCategories.AddRange(meats, vegetables, dairy, packaging, beverages);
        await context.SaveChangesAsync(ct);
    }

    private static async Task SeedAdjustmentReasonsAsync(Guid tenantId, IApplicationDbContext context, CancellationToken ct)
    {
        if (await context.AdjustmentReasons.AnyAsync(r => r.TenantId == tenantId, ct))
            return;

        var reasons = new (string Ar, string En)[]
        {
            ("فقدان", "Loss"),
            ("تلف", "Damage"),
            ("خطأ يدوي", "Manual Error"),
            ("فرق جرد", "Stock Count Variance"),
            ("انتهاء صلاحية", "Expiry"),
            ("هدر مطبخ", "Kitchen Waste")
        };

        foreach (var (ar, en) in reasons)
            context.AdjustmentReasons.Add(new AdjustmentReason(tenantId, ar, en));

        await context.SaveChangesAsync(ct);
    }
}
