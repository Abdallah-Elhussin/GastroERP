using GastroErp.Domain.Common;
using GastroErp.Domain.Events.Menu;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;

namespace GastroErp.Domain.Entities.Menu;

/// <summary>
/// Category — التصنيف (Aggregate Root)
/// <para>
/// يمثّل تصنيفاً رئيسياً أو فرعياً للمنتجات في النظام.
/// Represents a main or sub-category for products.
/// </para>
/// <para>
/// يدعم التدرج الهرمي (Self-Reference) حتى مستويين موصى بهما.
/// Supports self-referencing hierarchy (up to 2 levels recommended).
/// </para>
/// </summary>
public sealed class Category : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid? ParentCategoryId { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public string? DescriptionAr { get; private set; }
    public string? DescriptionEn { get; private set; }
    public string? ImageUrl { get; private set; }
    public string? Color { get; private set; }
    public string? Icon { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; }

    // Navigation (read-only من خارج الـ Aggregate)
    private readonly List<Category> _subCategories = [];
    public IReadOnlyCollection<Category> SubCategories => _subCategories.AsReadOnly();
    public Category? ParentCategory { get; private set; }

    private Category()
    {
        NameAr = string.Empty;
    }

    public Category(Guid tenantId, string nameAr, string? nameEn = null,
                    Guid? parentCategoryId = null, string? color = null,
                    string? icon = null, int sortOrder = 0)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId لا يمكن أن يكون فارغاً. / TenantId cannot be empty.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(nameAr))
            throw new BusinessException(ErrorCodes.NameArRequired);
        if (color is not null && (color.Length != 7 || !color.StartsWith('#')))
            throw new ArgumentException("اللون يجب أن يكون HEX صحيح (مثال: #FF5733). / Color must be a valid HEX (e.g. #FF5733).", nameof(color));

        TenantId = tenantId;
        NameAr = nameAr;
        NameEn = nameEn;
        ParentCategoryId = parentCategoryId;
        Color = color;
        Icon = icon;
        SortOrder = sortOrder;
        IsActive = true;

        RaiseDomainEvent(new CategoryCreatedEvent(Id, TenantId, NameAr));
    }

    public void UpdateInfo(string nameAr, string? nameEn, string? descriptionAr, string? descriptionEn, string? color, string? icon)
    {
        if (string.IsNullOrWhiteSpace(nameAr))
            throw new BusinessException(ErrorCodes.NameArRequired);

        NameAr = nameAr;
        NameEn = nameEn;
        DescriptionAr = descriptionAr;
        DescriptionEn = descriptionEn;
        Color = color;
        Icon = icon;
    }

    public void SetImage(string? imageUrl) => ImageUrl = imageUrl;

    public void SetSortOrder(int order)
    {
        if (order < 0) throw new ArgumentOutOfRangeException(nameof(order), "SortOrder يجب أن يكون موجباً. / SortOrder must be non-negative.");
        SortOrder = order;
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        RaiseDomainEvent(new CategoryDeactivatedEvent(Id, TenantId));
    }

    public void Activate() => IsActive = true;

    public bool IsSubCategory => ParentCategoryId.HasValue;
}
