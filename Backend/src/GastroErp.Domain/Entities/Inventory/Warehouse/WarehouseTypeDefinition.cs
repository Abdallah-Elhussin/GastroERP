using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Inventory.Warehouse;

/// <summary>
/// نوع المستودع (Aggregate) — جدول مستقل قابل للتوسعة لكل مستأجر.
/// </summary>
public sealed class WarehouseTypeDefinition : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public string Code { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public string? Description { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsSystem { get; private set; }
    public bool IsActive { get; private set; }

    private WarehouseTypeDefinition()
    {
        Code = string.Empty;
        NameAr = string.Empty;
    }

    public WarehouseTypeDefinition(
        Guid tenantId,
        string code,
        string nameAr,
        string? nameEn = null,
        string? description = null,
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
        SortOrder = Math.Max(0, sortOrder);
        IsSystem = isSystem;
        IsActive = true;
    }

    public void Update(string nameAr, string? nameEn, string? description, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        NameAr = nameAr.Trim();
        NameEn = nameEn?.Trim();
        Description = description?.Trim();
        SortOrder = Math.Max(0, sortOrder);
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    public WarehouseType? ToLegacyEnum() => Code switch
    {
        "MAIN" => WarehouseType.Main,
        "POS" => WarehouseType.POS,
        "PRODUCTION" => WarehouseType.Production,
        "RAW" or "RAWMATERIAL" => WarehouseType.RawMaterial,
        "FINISHED" or "FINISHEDGOODS" => WarehouseType.FinishedGoods,
        "RETURNS" => WarehouseType.Returns,
        "DAMAGED" => WarehouseType.Damaged,
        "TRANSIT" => WarehouseType.Transit,
        "KITCHEN" => WarehouseType.Kitchen,
        "BEVERAGE" => WarehouseType.Beverage,
        "DRY" or "DRYSTORE" => WarehouseType.DryStore,
        "CHILLER" => WarehouseType.Chiller,
        "FREEZER" => WarehouseType.Freezer,
        "PACKAGING" => WarehouseType.Packaging,
        "CLEANING" => WarehouseType.Cleaning,
        "WASTE" => WarehouseType.Waste,
        _ => null
    };
}
