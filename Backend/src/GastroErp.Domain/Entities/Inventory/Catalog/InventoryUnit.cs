using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Inventory.Catalog;

/// <summary>
/// وحدة قياس المخزون (Aggregate Root).
/// </summary>
public sealed class InventoryUnit : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public string Code { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public string Symbol { get; private set; }
    public string? SymbolAr { get; private set; }
    public byte DecimalPlaces { get; private set; }
    public Guid? BaseUnitId { get; private set; }
    public decimal ConversionFactor { get; private set; }
    public InventoryUnitType UnitType { get; private set; }
    public InventoryUnitClassification Classification { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; }

    private InventoryUnit()
    {
        NameAr = string.Empty;
        Symbol = string.Empty;
        Code = string.Empty;
        ConversionFactor = 1m;
        UnitType = InventoryUnitType.Measured;
        Classification = InventoryUnitClassification.Other;
    }

    public InventoryUnit(
        Guid tenantId,
        string nameAr,
        string symbol,
        string? nameEn = null,
        string? symbolAr = null,
        string? code = null,
        byte decimalPlaces = 2,
        Guid? baseUnitId = null,
        decimal conversionFactor = 1m,
        InventoryUnitType unitType = InventoryUnitType.Measured,
        InventoryUnitClassification classification = InventoryUnitClassification.Other,
        int sortOrder = 0)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        if (string.IsNullOrWhiteSpace(symbol)) throw new ArgumentException("Symbol cannot be empty.", nameof(symbol));
        if (decimalPlaces > 6) throw new ArgumentOutOfRangeException(nameof(decimalPlaces), "DecimalPlaces must be 0–6.");
        if (conversionFactor <= 0) throw new ArgumentOutOfRangeException(nameof(conversionFactor), "ConversionFactor must be positive.");

        TenantId = tenantId;
        NameAr = nameAr.Trim();
        NameEn = nameEn?.Trim();
        Symbol = symbol.Trim();
        SymbolAr = symbolAr?.Trim();
        Code = string.IsNullOrWhiteSpace(code) ? symbol.Trim().ToUpperInvariant() : code.Trim().ToUpperInvariant();
        DecimalPlaces = decimalPlaces;
        BaseUnitId = baseUnitId;
        ConversionFactor = conversionFactor;
        UnitType = unitType;
        Classification = classification;
        SortOrder = Math.Max(0, sortOrder);
        IsActive = true;
    }

    public void UpdateInfo(
        string nameAr,
        string symbol,
        string? nameEn,
        string? symbolAr,
        string? code = null,
        byte? decimalPlaces = null)
    {
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        if (string.IsNullOrWhiteSpace(symbol)) throw new ArgumentException("Symbol cannot be empty.", nameof(symbol));

        NameAr = nameAr.Trim();
        NameEn = nameEn?.Trim();
        Symbol = symbol.Trim();
        SymbolAr = symbolAr?.Trim();
        if (!string.IsNullOrWhiteSpace(code))
            Code = code.Trim().ToUpperInvariant();
        if (decimalPlaces.HasValue)
        {
            if (decimalPlaces.Value > 6) throw new ArgumentOutOfRangeException(nameof(decimalPlaces));
            DecimalPlaces = decimalPlaces.Value;
        }
    }

    public void SetBaseUnit(Guid? baseUnitId)
    {
        if (baseUnitId == Id)
            throw new ArgumentException("Unit cannot reference itself as base unit.");
        BaseUnitId = baseUnitId;
    }

    public void SetConversionFactor(decimal conversionFactor)
    {
        if (conversionFactor <= 0)
            throw new ArgumentOutOfRangeException(nameof(conversionFactor), "ConversionFactor must be positive.");
        ConversionFactor = conversionFactor;
    }

    public void SetMeasurementProfile(InventoryUnitType unitType, InventoryUnitClassification classification)
    {
        UnitType = unitType;
        Classification = classification;
    }

    public void SetSortOrder(int sortOrder) => SortOrder = Math.Max(0, sortOrder);

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;

    public void SoftDeleteUnit(string? deletedBy)
    {
        SoftDelete(deletedBy);
        IsActive = false;
    }
}
