using GastroErp.Domain.Common;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Inventory.Settings;

/// <summary>
/// إعدادات المخزون للـ Tenant (Aggregate Root)
/// </summary>
public sealed class InventorySetting : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }

    /// <summary>معرف الفرع (اختياري - إذا كانت الإعدادات على مستوى الفرع)</summary>
    public Guid? BranchId { get; private set; }

    /// <summary>المستودع الافتراضي للاستلامات في حال لم يُحدد</summary>
    public Guid? DefaultWarehouseId { get; private set; }

    /// <summary>طريقة التقييم المحاسبي للمخزون (FIFO, WeightedAverage, StandardCost)</summary>
    public InventoryCostingMethod CostingMethod { get; private set; }

    /// <summary>السماح ببيع كميات غير متوفرة فعلياً (رصيد سالب)</summary>
    public bool AllowNegativeInventory { get; private set; }

    /// <summary>حجز الكمية تلقائياً عند إنشاء طلب بيع</summary>
    public bool AutoReserveStock { get; private set; }

    /// <summary>صرف الوصفة تلقائياً عند البيع</summary>
    public bool AutoIssueRecipe { get; private set; }

    /// <summary>إلزام تتبع التشغيلات (Lots/Batches) عند استلام المواد</summary>
    public bool RequireBatchTracking { get; private set; }

    /// <summary>إلزام تسجيل تواريخ الانتهاء</summary>
    public bool RequireExpiryTracking { get; private set; }

    /// <summary>التوليد التلقائي لأكواد SKU للمواد</summary>
    public bool AutoGenerateSku { get; private set; }

    private InventorySetting() { }

    public InventorySetting(Guid tenantId, Guid? branchId = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));

        TenantId = tenantId;
        BranchId = branchId;
        CostingMethod = InventoryCostingMethod.WeightedAverage; // الافتراضي
        AllowNegativeInventory = false;
        AutoReserveStock = true;
        AutoIssueRecipe = true;
        RequireBatchTracking = false;
        RequireExpiryTracking = false;
        AutoGenerateSku = true;
    }

    public void UpdateSettings(
        Guid? defaultWarehouseId,
        InventoryCostingMethod costingMethod,
        bool allowNegativeInventory,
        bool autoReserveStock,
        bool autoIssueRecipe,
        bool requireBatchTracking,
        bool requireExpiryTracking,
        bool autoGenerateSku)
    {
        DefaultWarehouseId = defaultWarehouseId;
        CostingMethod = costingMethod;
        AllowNegativeInventory = allowNegativeInventory;
        AutoReserveStock = autoReserveStock;
        AutoIssueRecipe = autoIssueRecipe;
        RequireBatchTracking = requireBatchTracking;
        RequireExpiryTracking = requireExpiryTracking;
        AutoGenerateSku = autoGenerateSku;
    }
}
