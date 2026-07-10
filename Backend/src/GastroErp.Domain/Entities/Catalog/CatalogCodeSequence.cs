namespace GastroErp.Domain.Entities.Catalog;

/// <summary>تسلسل توليد أكواد الكatalog لكل tenant وبادئة</summary>
public sealed class CatalogCodeSequence
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid TenantId { get; private set; }
    public string Prefix { get; private set; }
    public int LastNumber { get; private set; }
    public int Padding { get; private set; }

    private CatalogCodeSequence() { Prefix = string.Empty; }

    public CatalogCodeSequence(Guid tenantId, string prefix, int padding = 6)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(prefix)) throw new ArgumentException("Prefix cannot be empty.", nameof(prefix));
        TenantId = tenantId;
        Prefix = prefix.ToUpperInvariant();
        Padding = padding;
        LastNumber = 0;
    }

    public string NextCode()
    {
        LastNumber++;
        return $"{Prefix}-{LastNumber.ToString().PadLeft(Padding, '0')}";
    }
}
