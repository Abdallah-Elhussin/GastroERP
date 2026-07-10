using GastroErp.Application.Common.Interfaces;
using GastroErp.Domain.Entities.Menu;
using GastroErp.Domain.Entities.Organization;
using GastroErp.Domain.Entities.Sales;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Persistence.Seeders;

public sealed class KitchenOperationalSeeder : IDataSeeder
{
    private readonly ILogger<KitchenOperationalSeeder> _logger;

    public KitchenOperationalSeeder(ILogger<KitchenOperationalSeeder> logger) => _logger = logger;

    public int Order => 45;

    public async Task SeedAsync(Guid tenantId, IApplicationDbContext context, CancellationToken ct = default)
    {
        var branch = await EnsureBranchAsync(tenantId, context, ct);
        if (branch is null)
        {
            _logger.LogWarning("Kitchen operational seed skipped — no branch for tenant {TenantId}", tenantId);
            return;
        }

        await EnsureDevicesAsync(tenantId, branch, context, ct);
        await EnsureKitchenStationsAsync(tenantId, branch, context, ct);
        _logger.LogInformation("Kitchen operational data seeded for tenant {TenantId}", tenantId);
    }

    private static async Task<Branch?> EnsureBranchAsync(
        Guid tenantId,
        IApplicationDbContext context,
        CancellationToken ct)
    {
        var branch = await context.Branches.FirstOrDefaultAsync(b => b.TenantId == tenantId, ct);
        if (branch is not null)
            return branch;

        var company = await context.Companies.FirstOrDefaultAsync(c => c.TenantId == tenantId, ct);
        if (company is null)
        {
            company = new Company(tenantId, "مجموعة جاسترو للضيافة", "1010456789", "Gastro Hospitality Group");
            context.Companies.Add(company);
            await context.SaveChangesAsync(ct);
        }

        branch = new Branch(tenantId, company.Id, "الفرع الرئيسي", BranchType.Restaurant, "Main Branch", "BR1");
        branch.SetAsDefault();
        context.Branches.Add(branch);
        await context.SaveChangesAsync(ct);
        return branch;
    }

    private static async Task EnsureDevicesAsync(
        Guid tenantId,
        Branch branch,
        IApplicationDbContext context,
        CancellationToken ct)
    {
        if (!await context.Devices.AnyAsync(d => d.TenantId == tenantId && d.DeviceType == DeviceType.POSTerminal, ct))
        {
            var pos = new Device(tenantId, "كاشير POS", DeviceType.POSTerminal, nameEn: "POS Terminal");
            pos.Activate(branch.Id);
            context.Devices.Add(pos);
            context.BranchDevices.Add(new BranchDevice(branch.Id, pos.Id, tenantId));
        }

        if (!await context.Devices.AnyAsync(d => d.TenantId == tenantId && d.DeviceType == DeviceType.KitchenDisplay, ct))
        {
            var kds = new Device(tenantId, "شاشة المطبخ", DeviceType.KitchenDisplay, nameEn: "Kitchen Display");
            kds.Activate(branch.Id);
            context.Devices.Add(kds);
            context.BranchDevices.Add(new BranchDevice(branch.Id, kds.Id, tenantId));
        }

        await context.SaveChangesAsync(ct);
    }

    private static async Task EnsureKitchenStationsAsync(
        Guid tenantId,
        Branch branch,
        IApplicationDbContext context,
        CancellationToken ct)
    {
        if (await context.KitchenStations.AnyAsync(s => s.TenantId == tenantId && s.BranchId == branch.Id, ct))
            return;

        Guid? mainsCategoryId = await context.Categories
            .Where(c => c.TenantId == tenantId && c.NameAr == "أطباق رئيسية")
            .Select(c => (Guid?)c.Id)
            .FirstOrDefaultAsync(ct);

        Guid? dessertsCategoryId = await context.Categories
            .Where(c => c.TenantId == tenantId && c.NameAr == "حلويات")
            .Select(c => (Guid?)c.Id)
            .FirstOrDefaultAsync(ct);

        Guid? beveragesCategoryId = await context.Categories
            .Where(c => c.TenantId == tenantId && c.NameAr == "مشروبات")
            .Select(c => (Guid?)c.Id)
            .FirstOrDefaultAsync(ct);

        var stations = new[]
        {
            KitchenStation.Create(tenantId, branch.Id, "محطة التحضير", KitchenStationType.Hot, "Line Station", categoryId: mainsCategoryId, sortOrder: 1),
            KitchenStation.Create(tenantId, branch.Id, "محطة البارد", KitchenStationType.Cold, "Cold Station", categoryId: beveragesCategoryId, sortOrder: 2),
            KitchenStation.Create(tenantId, branch.Id, "محطة الحلويات", KitchenStationType.Dessert, "Pastry Station", categoryId: dessertsCategoryId, sortOrder: 3),
            KitchenStation.Create(tenantId, branch.Id, "محطة عامة", KitchenStationType.General, "General Station", sortOrder: 99)
        };

        context.KitchenStations.AddRange(stations);
        await context.SaveChangesAsync(ct);
    }
}
