using GastroErp.Domain.Common;

namespace GastroErp.Domain.Entities.Inventory.Catalog;

/// <summary>
/// معامل التحويل بين الوحدات (Entity)
/// يتم فصله عن الصنف لخدمة جميع الأصناف. يربط بين وحدتين بمعامل ضرب.
/// مثال: FromUnit=Carton(10kg), ToUnit=Kilogram, Factor=10
/// </summary>
public sealed class UnitConversion : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid FromUnitId { get; private set; }
    public Guid ToUnitId { get; private set; }

    /// <summary>معامل الضرب للتحويل من الوحدة الأولى للثانية (FromUnit * Factor = ToUnit)</summary>
    public decimal ConversionFactor { get; private set; }

    private UnitConversion() { }

    public UnitConversion(Guid tenantId, Guid fromUnitId, Guid toUnitId, decimal conversionFactor)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (fromUnitId == Guid.Empty) throw new ArgumentException("FromUnitId cannot be empty.", nameof(fromUnitId));
        if (toUnitId == Guid.Empty) throw new ArgumentException("ToUnitId cannot be empty.", nameof(toUnitId));
        if (conversionFactor <= 0) throw new ArgumentException("ConversionFactor must be greater than zero.", nameof(conversionFactor));
        if (fromUnitId == toUnitId) throw new ArgumentException("FromUnitId and ToUnitId cannot be the same.");

        TenantId = tenantId;
        FromUnitId = fromUnitId;
        ToUnitId = toUnitId;
        ConversionFactor = conversionFactor;
    }

    public void UpdateFactor(decimal newFactor)
    {
        if (newFactor <= 0) throw new ArgumentException("ConversionFactor must be greater than zero.", nameof(newFactor));
        ConversionFactor = newFactor;
    }
}
