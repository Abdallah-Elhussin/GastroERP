using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Sales;

/// <summary>KitchenStation — محطة المطبخ (Aggregate Root)</summary>
public sealed class KitchenStation : AuditableBaseEntity, ITenantEntity, IBranchEntity
{
    public Guid TenantId { get; private set; }
    public Guid BranchId { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public KitchenStationType StationType { get; private set; }
    public Guid? DeviceId { get; private set; }
    public Guid? CategoryId { get; private set; }
    public bool IsActive { get; private set; }
    public int SortOrder { get; private set; }

    private KitchenStation() { NameAr = string.Empty; }

    public static KitchenStation Create(
        Guid tenantId, Guid branchId, string nameAr, KitchenStationType stationType,
        string? nameEn = null, Guid? deviceId = null, Guid? categoryId = null, int sortOrder = 0)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (branchId == Guid.Empty) throw new ArgumentException("BranchId cannot be empty.", nameof(branchId));
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);

        return new KitchenStation
        {
            TenantId = tenantId,
            BranchId = branchId,
            NameAr = nameAr,
            NameEn = nameEn,
            StationType = stationType,
            DeviceId = deviceId,
            CategoryId = categoryId,
            IsActive = true,
            SortOrder = sortOrder
        };
    }

    public void Update(string nameAr, string? nameEn, KitchenStationType stationType, Guid? deviceId, Guid? categoryId, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        NameAr = nameAr;
        NameEn = nameEn;
        StationType = stationType;
        DeviceId = deviceId;
        CategoryId = categoryId;
        SortOrder = sortOrder;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
