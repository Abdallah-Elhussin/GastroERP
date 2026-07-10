using GastroErp.Domain.Common;

namespace GastroErp.Domain.Entities.Catalog;

/// <summary>
/// سجل تغييرات أسعار المنتج في الكatalog.
/// </summary>
public sealed class ProductPriceHistory : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid CatalogDefinitionId { get; private set; }
    public Guid? ProductId { get; private set; }
    public Guid? PriceLevelId { get; private set; }
    public string? PriceLevelName { get; private set; }
    public decimal OldPrice { get; private set; }
    public decimal NewPrice { get; private set; }
    public string Currency { get; private set; }

    private ProductPriceHistory() { Currency = "SAR"; }

    public ProductPriceHistory(
        Guid tenantId,
        Guid catalogDefinitionId,
        Guid? productId,
        decimal oldPrice,
        decimal newPrice,
        string currency,
        Guid? priceLevelId = null,
        string? priceLevelName = null)
    {
        TenantId = tenantId;
        CatalogDefinitionId = catalogDefinitionId;
        ProductId = productId;
        OldPrice = oldPrice;
        NewPrice = newPrice;
        Currency = string.IsNullOrWhiteSpace(currency) ? "SAR" : currency.ToUpperInvariant();
        PriceLevelId = priceLevelId;
        PriceLevelName = priceLevelName;
    }
}
