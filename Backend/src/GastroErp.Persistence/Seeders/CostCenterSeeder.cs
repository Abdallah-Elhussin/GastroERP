using GastroErp.Application.Common.Interfaces;
using GastroErp.Domain.Entities.Finance;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Persistence.Seeders;

/// <summary>مراكز تكلفة افتراضية لمطعم.</summary>
public sealed class CostCenterSeeder : IDataSeeder
{
    private readonly ILogger<CostCenterSeeder> _logger;
    public CostCenterSeeder(ILogger<CostCenterSeeder> logger) => _logger = logger;
    public int Order => 22;

    public async Task SeedAsync(Guid tenantId, IApplicationDbContext context, CancellationToken ct = default)
    {
        if (await context.CostCenters.AnyAsync(c => c.TenantId == tenantId, ct))
            return;

        var branchId = await context.Branches.AsNoTracking()
            .Where(b => b.TenantId == tenantId)
            .Select(b => (Guid?)b.Id)
            .FirstOrDefaultAsync(ct);

        if (branchId is null)
        {
            _logger.LogWarning("No branch found for tenant {TenantId}; skipping cost center seed.", tenantId);
            return;
        }

        var n = 1;
        Guid Add(string code, string ar, string en, CostCenterType type, Guid? parent = null)
        {
            var center = CostCenter.Create(
                tenantId, branchId.Value, n, code, ar, type, parent,
                nameEn: en, sortOrder: n, isSystem: true);
            context.CostCenters.Add(center);
            n++;
            return center.Id;
        }

        // الإدارة
        var admin = Add("CC-ADM", "الإدارة العامة", "General Administration", CostCenterType.Administrative);
        Add("CC-HR", "الموارد البشرية", "Human Resources", CostCenterType.Administrative, admin);
        Add("CC-FIN", "المالية", "Finance", CostCenterType.Administrative, admin);
        Add("CC-IT", "تقنية المعلومات", "Information Technology", CostCenterType.Administrative, admin);

        // التشغيل / الإنتاج
        var kitchen = Add("CC-KIT", "المطبخ", "Kitchen", CostCenterType.Production);
        Add("CC-HOT", "المطبخ الساخن", "Hot Kitchen", CostCenterType.Production, kitchen);
        Add("CC-BAK", "المخبز", "Bakery", CostCenterType.Production, kitchen);
        Add("CC-PAS", "الحلويات", "Pastry", CostCenterType.Production, kitchen);
        Add("CC-BEV", "المشروبات", "Beverages", CostCenterType.Production);

        // الخدمة
        Add("CC-DIN", "صالة الطعام", "Dining Hall", CostCenterType.Service);
        Add("CC-CSH", "الكاشير", "Cashier", CostCenterType.Service);
        Add("CC-CS", "خدمة العملاء", "Customer Service", CostCenterType.Service);

        // التوصيل / تشغيلي
        Add("CC-DEL", "التوصيل", "Delivery", CostCenterType.Operational);
        Add("CC-APP", "تطبيقات التوصيل", "Delivery Apps", CostCenterType.Operational);

        // المخزون
        var wh = Add("CC-WH", "المستودع الرئيسي", "Main Warehouse", CostCenterType.Operational);
        Add("CC-DRY", "مستودع المواد الجافة", "Dry Store", CostCenterType.Operational, wh);
        Add("CC-CLD", "المستودع المبرد", "Chilled Store", CostCenterType.Operational, wh);
        Add("CC-FRZ", "المستودع المجمد", "Frozen Store", CostCenterType.Operational, wh);

        // الصيانة
        var mnt = Add("CC-MNT", "الصيانة", "Maintenance", CostCenterType.Operational);
        Add("CC-EQP", "المعدات", "Equipment", CostCenterType.Operational, mnt);
        Add("CC-VEH", "المركبات", "Vehicles", CostCenterType.Operational, mnt);

        // التسويق
        var mkt = Add("CC-MKT", "التسويق", "Marketing", CostCenterType.Administrative);
        Add("CC-PRO", "العروض", "Promotions", CostCenterType.Administrative, mkt);
        Add("CC-LOY", "الولاء", "Loyalty", CostCenterType.Administrative, mkt);

        // مشترك / مبيعات (screenshot samples)
        Add("CC-SAL", "المبيعات", "Sales", CostCenterType.Operational);
        Add("CC-SHR", "المصروفات المشتركة", "Shared Expenses", CostCenterType.Administrative);

        await context.SaveChangesAsync(ct);
        _logger.LogInformation("Cost centers seeded for tenant {TenantId}", tenantId);
    }
}
