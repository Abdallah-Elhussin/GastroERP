using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Inventory.Warehouse;

/// <summary>
/// المستودع (Aggregate Root)
/// </summary>
public sealed class Warehouse : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid? BranchId { get; private set; }
    public Guid? CompanyId { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public string? Code { get; private set; }
    public string? Address { get; private set; }
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public string? Notes { get; private set; }
    public WarehouseType WarehouseType { get; private set; }
    public Guid? WarehouseTypeId { get; private set; }
    public Guid? ParentWarehouseId { get; private set; }
    public Guid? ManagerUserId { get; private set; }
    public Guid? ResponsibleEmployeeId { get; private set; }

    public bool AllowPurchase { get; private set; }
    public bool AllowSales { get; private set; }
    public bool AllowTransfer { get; private set; }
    public bool AllowInventoryCount { get; private set; }
    public bool AllowManufacturing { get; private set; }
    public bool AllowNegativeStock { get; private set; }
    public bool AllowReservation { get; private set; }
    public bool AllowReceiving { get; private set; }
    public bool AllowIssue { get; private set; }
    public bool AllowAdjustment { get; private set; }
    public bool IsPosWarehouse { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsSystem { get; private set; }
    public bool UseBins { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<WarehouseZone> _zones = [];
    public IReadOnlyCollection<WarehouseZone> Zones => _zones.AsReadOnly();

    private Warehouse() { NameAr = string.Empty; }

    public Warehouse(
        Guid tenantId,
        string nameAr,
        string? nameEn = null,
        string? code = null,
        Guid? branchId = null,
        WarehouseType warehouseType = WarehouseType.Main,
        Guid? companyId = null,
        Guid? warehouseTypeId = null,
        Guid? parentWarehouseId = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        if (parentWarehouseId.HasValue && parentWarehouseId == Guid.Empty)
            throw new ArgumentException("ParentWarehouseId cannot be empty.", nameof(parentWarehouseId));

        TenantId = tenantId;
        BranchId = branchId;
        CompanyId = companyId;
        NameAr = nameAr.Trim();
        NameEn = nameEn?.Trim();
        Code = string.IsNullOrWhiteSpace(code) ? null : code.Trim().ToUpperInvariant();
        WarehouseType = warehouseType;
        WarehouseTypeId = warehouseTypeId;
        ParentWarehouseId = parentWarehouseId;
        AllowPurchase = true;
        AllowSales = true;
        AllowTransfer = true;
        AllowInventoryCount = true;
        AllowManufacturing = true;
        AllowNegativeStock = false;
        AllowReservation = true;
        AllowReceiving = true;
        AllowIssue = true;
        AllowAdjustment = true;
        IsPosWarehouse = warehouseType == WarehouseType.POS;
        IsDefault = false;
        IsSystem = false;
        UseBins = false;
        IsActive = true;
    }

    public WarehouseZone AddZone(string nameAr, string? nameEn = null, string? code = null)
    {
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        var zone = new WarehouseZone(TenantId, Id, nameAr, nameEn, code);
        _zones.Add(zone);
        return zone;
    }

    public void RemoveZone(Guid zoneId)
    {
        var zone = _zones.FirstOrDefault(z => z.Id == zoneId)
            ?? throw new BusinessException(ErrorCodes.ItemNotFound);
        _zones.Remove(zone);
    }

    public void UpdateInfo(
        string nameAr,
        string? nameEn,
        string? code,
        string? address,
        string? phone = null,
        string? email = null,
        string? notes = null,
        Guid? branchId = null,
        Guid? companyId = null)
    {
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        NameAr = nameAr.Trim();
        NameEn = nameEn?.Trim();
        Code = string.IsNullOrWhiteSpace(code) ? null : code.Trim().ToUpperInvariant();
        Address = address?.Trim();
        Phone = phone?.Trim();
        Email = email?.Trim();
        Notes = notes?.Trim();
        BranchId = branchId;
        CompanyId = companyId;
    }

    public void SetType(WarehouseType type) => WarehouseType = type;

    public void SetWarehouseType(Guid? warehouseTypeId, WarehouseType? legacyType = null)
    {
        WarehouseTypeId = warehouseTypeId;
        if (legacyType.HasValue)
            WarehouseType = legacyType.Value;
    }

    public void SetParent(Guid? parentWarehouseId)
    {
        if (parentWarehouseId == Id)
            throw new ArgumentException("Warehouse cannot be its own parent.");
        ParentWarehouseId = parentWarehouseId;
    }

    public void AssignStaff(Guid? managerUserId, Guid? responsibleEmployeeId)
    {
        ManagerUserId = managerUserId;
        ResponsibleEmployeeId = responsibleEmployeeId;
    }

    public void SetPermissions(
        bool allowPurchase,
        bool allowSales,
        bool allowTransfer,
        bool allowInventoryCount,
        bool allowManufacturing,
        bool allowNegativeStock = false,
        bool allowReservation = true,
        bool allowReceiving = true,
        bool allowIssue = true,
        bool allowAdjustment = true)
    {
        AllowPurchase = allowPurchase;
        AllowSales = allowSales;
        AllowTransfer = allowTransfer;
        AllowInventoryCount = allowInventoryCount;
        AllowManufacturing = allowManufacturing;
        AllowNegativeStock = allowNegativeStock;
        AllowReservation = allowReservation;
        AllowReceiving = allowReceiving;
        AllowIssue = allowIssue;
        AllowAdjustment = allowAdjustment;
    }

    public void SetFlags(bool isPosWarehouse, bool isDefault, bool useBins, bool isSystem = false)
    {
        IsPosWarehouse = isPosWarehouse;
        IsDefault = isDefault;
        UseBins = useBins;
        if (isSystem) IsSystem = true;
    }

    public void MarkAsDefault(bool isDefault) => IsDefault = isDefault;

    public void SoftDeleteWarehouse(string? deletedBy)
    {
        if (IsSystem)
            throw new BusinessException(ErrorCodes.CannotModifyApprovedDocument);
        SoftDelete(deletedBy);
        IsActive = false;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}

/// <summary>المنطقة (مثال: منطقة التبريد، منطقة المواد الجافة)</summary>
public sealed class WarehouseZone : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public string? Code { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<WarehouseShelf> _shelves = [];
    public IReadOnlyCollection<WarehouseShelf> Shelves => _shelves.AsReadOnly();

    private WarehouseZone() { NameAr = string.Empty; }

    internal WarehouseZone(Guid tenantId, Guid warehouseId, string nameAr, string? nameEn, string? code)
    {
        TenantId = tenantId;
        WarehouseId = warehouseId;
        NameAr = nameAr;
        NameEn = nameEn;
        Code = code;
        IsActive = true;
    }

    public WarehouseShelf AddShelf(string nameAr, string? nameEn = null, string? code = null)
    {
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        var shelf = new WarehouseShelf(TenantId, Id, nameAr, nameEn, code);
        _shelves.Add(shelf);
        return shelf;
    }

    public void RemoveShelf(Guid shelfId)
    {
        var shelf = _shelves.FirstOrDefault(s => s.Id == shelfId)
            ?? throw new BusinessException(ErrorCodes.ItemNotFound);
        _shelves.Remove(shelf);
    }
}

/// <summary>الرف داخل المنطقة</summary>
public sealed class WarehouseShelf : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid WarehouseZoneId { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public string? Code { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<WarehouseBin> _bins = [];
    public IReadOnlyCollection<WarehouseBin> Bins => _bins.AsReadOnly();

    private WarehouseShelf() { NameAr = string.Empty; }

    internal WarehouseShelf(Guid tenantId, Guid warehouseZoneId, string nameAr, string? nameEn, string? code)
    {
        TenantId = tenantId;
        WarehouseZoneId = warehouseZoneId;
        NameAr = nameAr;
        NameEn = nameEn;
        Code = code;
        IsActive = true;
    }

    public WarehouseBin AddBin(string nameAr, string? nameEn = null, string? code = null)
    {
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        var bin = new WarehouseBin(TenantId, Id, nameAr, nameEn, code);
        _bins.Add(bin);
        return bin;
    }

    public void RemoveBin(Guid binId)
    {
        var bin = _bins.FirstOrDefault(b => b.Id == binId)
            ?? throw new BusinessException(ErrorCodes.ItemNotFound);
        _bins.Remove(bin);
    }
}

/// <summary>الحاوية أو الدرج (أصغر وحدة تخزين)</summary>
public sealed class WarehouseBin : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid WarehouseShelfId { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public string? Code { get; private set; }
    public bool IsActive { get; private set; }

    private WarehouseBin() { NameAr = string.Empty; }

    internal WarehouseBin(Guid tenantId, Guid warehouseShelfId, string nameAr, string? nameEn, string? code)
    {
        TenantId = tenantId;
        WarehouseShelfId = warehouseShelfId;
        NameAr = nameAr;
        NameEn = nameEn;
        Code = code;
        IsActive = true;
    }
}
