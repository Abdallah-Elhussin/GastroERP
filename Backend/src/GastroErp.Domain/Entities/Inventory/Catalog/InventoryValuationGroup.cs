using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;

namespace GastroErp.Domain.Entities.Inventory.Catalog;

/// <summary>
/// مجموعة التقييم المخزني (Aggregate Root) — تجميع أصناف لأغراض التقييم المحاسبي وربط مركز التكلفة.
/// </summary>
public sealed class InventoryValuationGroup : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public string Code { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public string? Description { get; private set; }
    public Guid? CostCenterId { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsSystem { get; private set; }
    public bool IsActive { get; private set; }

    private InventoryValuationGroup()
    {
        Code = string.Empty;
        NameAr = string.Empty;
    }

    public InventoryValuationGroup(
        Guid tenantId,
        string code,
        string nameAr,
        string? nameEn = null,
        string? description = null,
        Guid? costCenterId = null,
        int sortOrder = 0,
        bool isSystem = false)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);

        TenantId = tenantId;
        Code = code.Trim().ToUpperInvariant();
        NameAr = nameAr.Trim();
        NameEn = nameEn?.Trim();
        Description = description?.Trim();
        CostCenterId = costCenterId;
        SortOrder = Math.Max(0, sortOrder);
        IsSystem = isSystem;
        IsActive = true;
    }

    public void Update(
        string nameAr,
        string? nameEn,
        string? description,
        Guid? costCenterId,
        int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        NameAr = nameAr.Trim();
        NameEn = nameEn?.Trim();
        Description = description?.Trim();
        CostCenterId = costCenterId;
        SortOrder = Math.Max(0, sortOrder);
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    public void SoftDeleteGroup(string? deletedBy)
    {
        if (IsSystem)
            throw new BusinessException(ErrorCodes.CannotModifyApprovedDocument);
        SoftDelete(deletedBy);
        IsActive = false;
    }
}
