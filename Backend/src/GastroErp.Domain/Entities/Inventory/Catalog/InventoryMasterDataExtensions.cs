using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Inventory.Catalog;

/// <summary>العلامة التجارية للمخزون (Aggregate Root).</summary>
public sealed class InventoryBrand : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public string Code { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public bool IsActive { get; private set; }

    private InventoryBrand()
    {
        NameAr = string.Empty;
        Code = string.Empty;
    }

    public InventoryBrand(Guid tenantId, string nameAr, string? nameEn = null, string? code = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);

        TenantId = tenantId;
        NameAr = nameAr.Trim();
        NameEn = nameEn?.Trim();
        Code = string.IsNullOrWhiteSpace(code)
            ? GenerateCode("BR", nameAr)
            : code.Trim().ToUpperInvariant();
        IsActive = true;
    }

    public void UpdateInfo(string nameAr, string? nameEn, string? code = null)
    {
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        NameAr = nameAr.Trim();
        NameEn = nameEn?.Trim();
        if (!string.IsNullOrWhiteSpace(code))
            Code = code.Trim().ToUpperInvariant();
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    internal static string GenerateCode(string prefix, string nameAr)
    {
        var slug = new string(nameAr.Where(char.IsLetterOrDigit).Take(6).ToArray()).ToUpperInvariant();
        return string.IsNullOrWhiteSpace(slug)
            ? $"{prefix}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}"
            : $"{prefix}-{slug}";
    }
}

/// <summary>المصنّع (Aggregate Root).</summary>
public sealed class InventoryManufacturer : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public string Code { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public string? Country { get; private set; }
    public bool IsActive { get; private set; }

    private InventoryManufacturer()
    {
        NameAr = string.Empty;
        Code = string.Empty;
    }

    public InventoryManufacturer(
        Guid tenantId,
        string nameAr,
        string? nameEn = null,
        string? code = null,
        string? country = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);

        TenantId = tenantId;
        NameAr = nameAr.Trim();
        NameEn = nameEn?.Trim();
        Country = country?.Trim();
        Code = string.IsNullOrWhiteSpace(code)
            ? InventoryBrand.GenerateCode("MFG", nameAr)
            : code.Trim().ToUpperInvariant();
        IsActive = true;
    }

    public void UpdateInfo(string nameAr, string? nameEn, string? code = null, string? country = null)
    {
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        NameAr = nameAr.Trim();
        NameEn = nameEn?.Trim();
        Country = country?.Trim();
        if (!string.IsNullOrWhiteSpace(code))
            Code = code.Trim().ToUpperInvariant();
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}

/// <summary>سمة صنف مخزني (Aggregate Root) مع قيم اختيارية للقوائم.</summary>
public sealed class InventoryAttribute : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public string Code { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public InventoryAttributeDataType DataType { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<InventoryAttributeValue> _values = [];
    public IReadOnlyCollection<InventoryAttributeValue> Values => _values.AsReadOnly();

    private InventoryAttribute()
    {
        NameAr = string.Empty;
        Code = string.Empty;
    }

    public InventoryAttribute(
        Guid tenantId,
        string nameAr,
        InventoryAttributeDataType dataType = InventoryAttributeDataType.Text,
        string? nameEn = null,
        string? code = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);

        TenantId = tenantId;
        NameAr = nameAr.Trim();
        NameEn = nameEn?.Trim();
        DataType = dataType;
        Code = string.IsNullOrWhiteSpace(code)
            ? InventoryBrand.GenerateCode("ATTR", nameAr)
            : code.Trim().ToUpperInvariant();
        IsActive = true;
    }

    public void UpdateInfo(
        string nameAr,
        string? nameEn,
        InventoryAttributeDataType dataType,
        string? code = null)
    {
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        NameAr = nameAr.Trim();
        NameEn = nameEn?.Trim();
        DataType = dataType;
        if (!string.IsNullOrWhiteSpace(code))
            Code = code.Trim().ToUpperInvariant();
    }

    public InventoryAttributeValue AddValue(string valueAr, string? valueEn = null, int sortOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(valueAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        if (DataType != InventoryAttributeDataType.List)
            throw new BusinessException(ErrorCodes.InvalidStatusTransition);

        var value = new InventoryAttributeValue(TenantId, Id, valueAr, valueEn, sortOrder);
        _values.Add(value);
        return value;
    }

    public void RemoveValue(Guid valueId)
    {
        var value = _values.FirstOrDefault(v => v.Id == valueId)
            ?? throw new BusinessException(ErrorCodes.ItemNotFound);
        _values.Remove(value);
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}

public sealed class InventoryAttributeValue : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid AttributeId { get; private set; }
    public string ValueAr { get; private set; }
    public string? ValueEn { get; private set; }
    public int SortOrder { get; private set; }

    private InventoryAttributeValue() { ValueAr = string.Empty; }

    internal InventoryAttributeValue(
        Guid tenantId,
        Guid attributeId,
        string valueAr,
        string? valueEn,
        int sortOrder)
    {
        TenantId = tenantId;
        AttributeId = attributeId;
        ValueAr = valueAr.Trim();
        ValueEn = valueEn?.Trim();
        SortOrder = Math.Max(0, sortOrder);
    }
}

/// <summary>قائمة أسعار مخزنية (Aggregate Root).</summary>
public sealed class InventoryPriceList : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public string Code { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public string Currency { get; private set; }
    public DateTimeOffset? ValidFrom { get; private set; }
    public DateTimeOffset? ValidTo { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<InventoryPriceListLine> _lines = [];
    public IReadOnlyCollection<InventoryPriceListLine> Lines => _lines.AsReadOnly();

    private InventoryPriceList()
    {
        NameAr = string.Empty;
        Code = string.Empty;
        Currency = "SAR";
    }

    public InventoryPriceList(
        Guid tenantId,
        string nameAr,
        string? nameEn = null,
        string? code = null,
        string currency = "SAR",
        DateTimeOffset? validFrom = null,
        DateTimeOffset? validTo = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);

        TenantId = tenantId;
        NameAr = nameAr.Trim();
        NameEn = nameEn?.Trim();
        Currency = string.IsNullOrWhiteSpace(currency) ? "SAR" : currency.Trim().ToUpperInvariant();
        ValidFrom = validFrom;
        ValidTo = validTo;
        Code = string.IsNullOrWhiteSpace(code)
            ? InventoryBrand.GenerateCode("PL", nameAr)
            : code.Trim().ToUpperInvariant();
        IsActive = true;
    }

    public void UpdateInfo(
        string nameAr,
        string? nameEn,
        string? code,
        string currency,
        DateTimeOffset? validFrom,
        DateTimeOffset? validTo)
    {
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        NameAr = nameAr.Trim();
        NameEn = nameEn?.Trim();
        Currency = string.IsNullOrWhiteSpace(currency) ? Currency : currency.Trim().ToUpperInvariant();
        ValidFrom = validFrom;
        ValidTo = validTo;
        if (!string.IsNullOrWhiteSpace(code))
            Code = code.Trim().ToUpperInvariant();
    }

    public InventoryPriceListLine UpsertLine(Guid inventoryItemId, decimal unitPrice, Guid? unitId = null)
    {
        if (inventoryItemId == Guid.Empty) throw new ArgumentException("InventoryItemId is required.", nameof(inventoryItemId));
        if (unitPrice < 0) throw new ArgumentOutOfRangeException(nameof(unitPrice));

        var existing = _lines.FirstOrDefault(l => l.InventoryItemId == inventoryItemId);
        if (existing is not null)
        {
            existing.Update(unitPrice, unitId);
            return existing;
        }

        var line = new InventoryPriceListLine(TenantId, Id, inventoryItemId, unitPrice, unitId);
        _lines.Add(line);
        return line;
    }

    public void RemoveLine(Guid lineId)
    {
        var line = _lines.FirstOrDefault(l => l.Id == lineId)
            ?? throw new BusinessException(ErrorCodes.ItemNotFound);
        _lines.Remove(line);
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}

public sealed class InventoryPriceListLine : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid PriceListId { get; private set; }
    public Guid InventoryItemId { get; private set; }
    public Guid? UnitId { get; private set; }
    public decimal UnitPrice { get; private set; }

    private InventoryPriceListLine() { }

    internal InventoryPriceListLine(
        Guid tenantId,
        Guid priceListId,
        Guid inventoryItemId,
        decimal unitPrice,
        Guid? unitId)
    {
        TenantId = tenantId;
        PriceListId = priceListId;
        InventoryItemId = inventoryItemId;
        UnitPrice = unitPrice;
        UnitId = unitId;
    }

    internal void Update(decimal unitPrice, Guid? unitId)
    {
        if (unitPrice < 0) throw new ArgumentOutOfRangeException(nameof(unitPrice));
        UnitPrice = unitPrice;
        UnitId = unitId;
    }
}
