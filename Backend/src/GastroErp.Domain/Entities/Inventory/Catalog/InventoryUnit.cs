using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
namespace GastroErp.Domain.Entities.Inventory.Catalog;

/// <summary>
/// وحدة قياس المخزون (Aggregate Root)
/// يمثّل الوحدات الأساسية مثل الكيلوجرام، اللتر، الحبة.
/// </summary>
public sealed class InventoryUnit : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public string Symbol { get; private set; }
    public string? SymbolAr { get; private set; }
    public bool IsActive { get; private set; }

    private InventoryUnit()
    {
        NameAr = string.Empty;
        Symbol = string.Empty;
    }

    public InventoryUnit(Guid tenantId, string nameAr, string symbol, string? nameEn = null, string? symbolAr = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        if (string.IsNullOrWhiteSpace(symbol)) throw new ArgumentException("Symbol cannot be empty.", nameof(symbol));

        TenantId = tenantId;
        NameAr = nameAr;
        NameEn = nameEn;
        Symbol = symbol;
        SymbolAr = symbolAr;
        IsActive = true;
    }

    public void UpdateInfo(string nameAr, string symbol, string? nameEn, string? symbolAr)
    {
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        if (string.IsNullOrWhiteSpace(symbol)) throw new ArgumentException("Symbol cannot be empty.", nameof(symbol));

        NameAr = nameAr;
        NameEn = nameEn;
        Symbol = symbol;
        SymbolAr = symbolAr;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
