using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;

namespace GastroErp.Domain.Entities.Inventory.Warehouse;
/// <summary>
/// المستودع (Aggregate Root)
/// </summary>
public sealed class Warehouse : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid? BranchId { get; private set; } // يمكن أن يكون مستودعاً مركزياً إذا كان null
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public string? Code { get; private set; }
    public string? Address { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<WarehouseZone> _zones = [];
    public IReadOnlyCollection<WarehouseZone> Zones => _zones.AsReadOnly();

    private Warehouse() { NameAr = string.Empty; }

    public Warehouse(Guid tenantId, string nameAr, string? nameEn = null, string? code = null, Guid? branchId = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);

        TenantId = tenantId;
        BranchId = branchId;
        NameAr = nameAr;
        NameEn = nameEn;
        Code = code;
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

    public void UpdateInfo(string nameAr, string? nameEn, string? code, string? address)
    {
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        NameAr = nameAr;
        NameEn = nameEn;
        Code = code;
        Address = address;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// المنطقة (مثال: منطقة التبريد، منطقة المواد الجافة)
/// </summary>
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

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// الرف داخل المنطقة
/// </summary>
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

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// الحاوية أو الدرج (أصغر وحدة تخزين)
/// </summary>
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
