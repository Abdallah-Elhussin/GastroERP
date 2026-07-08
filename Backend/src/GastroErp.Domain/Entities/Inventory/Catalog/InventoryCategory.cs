using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
namespace GastroErp.Domain.Entities.Inventory.Catalog;

/// <summary>
/// تصنيف المخزون (Aggregate Root)
/// يمثّل تصنيف المواد الخام (مثلاً: لحوم، خضروات، مواد تغليف)
/// </summary>
public sealed class InventoryCategory : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid? ParentCategoryId { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public string? DescriptionAr { get; private set; }
    public string? DescriptionEn { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<InventoryCategory> _subCategories = [];
    public IReadOnlyCollection<InventoryCategory> SubCategories => _subCategories.AsReadOnly();
    public InventoryCategory? ParentCategory { get; private set; }

    private InventoryCategory() { NameAr = string.Empty; }

    public InventoryCategory(Guid tenantId, string nameAr, string? nameEn = null, Guid? parentCategoryId = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);

        TenantId = tenantId;
        NameAr = nameAr;
        NameEn = nameEn;
        ParentCategoryId = parentCategoryId;
        IsActive = true;
    }

    public void UpdateInfo(string nameAr, string? nameEn, string? descriptionAr, string? descriptionEn)
    {
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        NameAr = nameAr;
        NameEn = nameEn;
        DescriptionAr = descriptionAr;
        DescriptionEn = descriptionEn;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
