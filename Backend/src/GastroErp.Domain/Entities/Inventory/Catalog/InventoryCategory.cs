using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;

namespace GastroErp.Domain.Entities.Inventory.Catalog;

/// <summary>
/// تصنيف المخزون (Aggregate Root) — يدعم مستويات غير محدودة.
/// </summary>
public sealed class InventoryCategory : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid? ParentCategoryId { get; private set; }
    public string Code { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public string? DescriptionAr { get; private set; }
    public string? DescriptionEn { get; private set; }
    public string? Icon { get; private set; }
    public string? ImageUrl { get; private set; }
    public string? Color { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<InventoryCategory> _subCategories = [];
    public IReadOnlyCollection<InventoryCategory> SubCategories => _subCategories.AsReadOnly();
    public InventoryCategory? ParentCategory { get; private set; }

    private InventoryCategory()
    {
        NameAr = string.Empty;
        Code = string.Empty;
    }

    public InventoryCategory(
        Guid tenantId,
        string nameAr,
        string? nameEn = null,
        Guid? parentCategoryId = null,
        string? code = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);

        TenantId = tenantId;
        NameAr = nameAr;
        NameEn = nameEn;
        ParentCategoryId = parentCategoryId;
        Code = string.IsNullOrWhiteSpace(code) ? GenerateTempCode(nameAr) : code.Trim().ToUpperInvariant();
        SortOrder = 0;
        IsActive = true;
    }

    public void UpdateInfo(
        string nameAr,
        string? nameEn,
        string? descriptionAr,
        string? descriptionEn,
        string? code = null)
    {
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        NameAr = nameAr;
        NameEn = nameEn;
        DescriptionAr = descriptionAr;
        DescriptionEn = descriptionEn;
        if (!string.IsNullOrWhiteSpace(code))
            Code = code.Trim().ToUpperInvariant();
    }

    public void SetParent(Guid? parentCategoryId)
    {
        if (parentCategoryId == Id)
            throw new ArgumentException("Category cannot be its own parent.");
        ParentCategoryId = parentCategoryId;
    }

    public void SetVisuals(string? icon, string? imageUrl, string? color)
    {
        Icon = icon;
        ImageUrl = imageUrl;
        Color = color;
    }

    public void SetSortOrder(int sortOrder) => SortOrder = Math.Max(0, sortOrder);

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;

    public void SoftDeleteCategory(string? deletedBy)
    {
        SoftDelete(deletedBy);
        IsActive = false;
    }

    private static string GenerateTempCode(string nameAr)
    {
        var slug = new string(nameAr.Where(char.IsLetterOrDigit).Take(6).ToArray()).ToUpperInvariant();
        return string.IsNullOrWhiteSpace(slug) ? $"CAT-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}" : $"CAT-{slug}";
    }
}
