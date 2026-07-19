using GastroErp.Domain.Common;
using GastroErp.Domain.Events.Inventory;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Inventory.Catalog;

/// <summary>
/// الصنف المخزني (Aggregate Root)
/// يمثّل المادة الخام أو المكون القابل للتخزين.
/// </summary>
public sealed class InventoryItem : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid CategoryId { get; private set; }

    /// <summary>نوع الصنف التشغيلي (مواد خام، منتج تام، عنصر قائمة…).</summary>
    public Guid? ItemTypeId { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public string? DescriptionAr { get; private set; }
    public string? DescriptionEn { get; private set; }
    public string? Sku { get; private set; }
    public string? Barcode { get; private set; }

    /// <summary>رابط صورة الصنف</summary>
    public string? ImageUrl { get; private set; }

    /// <summary>خام أو مصنع (مركّب / وصفة)</summary>
    public InventoryItemKind ItemKind { get; private set; }

    /// <summary>وحدة القياس الأساسية للمخزون (التي يتم التقييم المحاسبي بناءً عليها)</summary>
    public Guid BaseUnitId { get; private set; }

    /// <summary>وحدة الشراء الافتراضية</summary>
    public Guid? DefaultPurchaseUnitId { get; private set; }

    /// <summary>وحدة الوصفة الافتراضية (للربط مع قوائم الطعام)</summary>
    public Guid? DefaultRecipeUnitId { get; private set; }

    /// <summary>حد إعادة الطلب (Reorder Level)</summary>
    public decimal ReorderLevel { get; private set; }

    /// <summary>الكمية المثالية للطلب (Economic Order Quantity)</summary>
    public decimal ReorderQuantity { get; private set; }

    public bool IsActive { get; private set; }

    private InventoryItem() { NameAr = string.Empty; }

    public InventoryItem(Guid tenantId, Guid categoryId, string nameAr, Guid baseUnitId,
                         string? nameEn = null, string? sku = null, string? barcode = null,
                         InventoryItemKind itemKind = InventoryItemKind.Raw, string? imageUrl = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (categoryId == Guid.Empty) throw new ArgumentException("CategoryId cannot be empty.", nameof(categoryId));
        if (baseUnitId == Guid.Empty) throw new ArgumentException("BaseUnitId cannot be empty.", nameof(baseUnitId));
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);

        TenantId = tenantId;
        CategoryId = categoryId;
        NameAr = nameAr;
        NameEn = nameEn;
        BaseUnitId = baseUnitId;
        Sku = sku;
        Barcode = barcode;
        ItemKind = itemKind;
        ImageUrl = imageUrl;
        ReorderLevel = 0;
        ReorderQuantity = 0;
        IsActive = true;

        RaiseDomainEvent(new InventoryItemCreatedEvent(Id, TenantId, NameAr));
    }

    public void UpdateInfo(string nameAr, string? nameEn, string? descriptionAr, string? descriptionEn, string? sku, string? barcode,
                           InventoryItemKind? itemKind = null, string? imageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        NameAr = nameAr;
        NameEn = nameEn;
        DescriptionAr = descriptionAr;
        DescriptionEn = descriptionEn;
        Sku = sku;
        Barcode = barcode;
        if (itemKind.HasValue) ItemKind = itemKind.Value;
        if (imageUrl != null) ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl;
    }

    public void SetImageUrl(string? imageUrl) => ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl;

    public void SetItemKind(InventoryItemKind itemKind) => ItemKind = itemKind;

    public void SetCategory(Guid categoryId)
    {
        if (categoryId == Guid.Empty) throw new ArgumentException("CategoryId cannot be empty.", nameof(categoryId));
        CategoryId = categoryId;
    }

    public void SetItemType(Guid? itemTypeId) => ItemTypeId = itemTypeId;

    public void SetBaseUnit(Guid baseUnitId)
    {
        if (baseUnitId == Guid.Empty) throw new ArgumentException("BaseUnitId cannot be empty.", nameof(baseUnitId));
        BaseUnitId = baseUnitId;
    }

    public void SetUnits(Guid? purchaseUnitId, Guid? recipeUnitId)
    {
        DefaultPurchaseUnitId = purchaseUnitId;
        DefaultRecipeUnitId = recipeUnitId;
    }

    public void SetReorderInfo(decimal reorderLevel, decimal reorderQuantity)
    {
        if (reorderLevel < 0) throw new ArgumentException("ReorderLevel cannot be negative.");
        if (reorderQuantity < 0) throw new ArgumentException("ReorderQuantity cannot be negative.");

        ReorderLevel = reorderLevel;
        ReorderQuantity = reorderQuantity;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
