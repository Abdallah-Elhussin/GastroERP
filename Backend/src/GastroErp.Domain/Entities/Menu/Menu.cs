using GastroErp.Domain.Common;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Menu;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;

namespace GastroErp.Domain.Entities.Menu;

/// <summary>
/// PriceLevel — مستوى التسعير (Aggregate Root)
/// <para>
/// يمثّل مستوى سعر مرتبطاً بقناة بيع معينة (محلي، سفري، توصيل...).
/// Represents a pricing tier linked to a specific sales channel.
/// </para>
/// </summary>
public sealed class PriceLevel : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public SalesChannel SalesChannel { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; }

    private PriceLevel() { NameAr = string.Empty; }

    public PriceLevel(Guid tenantId, string nameAr, SalesChannel salesChannel,
                       string? nameEn = null, bool isDefault = false)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(nameAr))
            throw new BusinessException(ErrorCodes.NameArRequired);

        TenantId = tenantId;
        NameAr = nameAr;
        NameEn = nameEn;
        SalesChannel = salesChannel;
        IsDefault = isDefault;
        IsActive = true;
    }

    public void SetAsDefault() => IsDefault = true;
    public void UnsetDefault() => IsDefault = false;
    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;

    public void UpdateInfo(string nameAr, string? nameEn, SalesChannel salesChannel)
    {
        if (string.IsNullOrWhiteSpace(nameAr))
            throw new ArgumentException("NameAr is required.");
        NameAr = nameAr;
        NameEn = nameEn;
        SalesChannel = salesChannel;
    }
}

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Menu — المنيو (Aggregate Root)
/// <para>
/// يمثّل منيواً كاملاً يحتوي على أقسام وعناصر.
/// يمكن أن يكون قياسياً، موسمياً، أو خاصاً بقناة بيع معينة.
/// </para>
/// </summary>
public sealed class Menu : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public string? DescriptionAr { get; private set; }
    public string? DescriptionEn { get; private set; }
    public MenuType MenuType { get; private set; }
    public SalesChannel SalesChannel { get; private set; }
    public DateOnly? StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<MenuSection> _sections = [];
    public IReadOnlyCollection<MenuSection> Sections => _sections.AsReadOnly();

    private Menu() { NameAr = string.Empty; }

    public Menu(Guid tenantId, string nameAr, MenuType menuType, SalesChannel salesChannel,
                string? nameEn = null, DateOnly? startDate = null, DateOnly? endDate = null)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(nameAr))
            throw new BusinessException(ErrorCodes.NameArRequired);
        if (endDate.HasValue && startDate.HasValue && endDate < startDate)
            throw new ArgumentException("تاريخ الانتهاء يجب أن يكون بعد تاريخ البداية. / EndDate must be after StartDate.", nameof(endDate));

        TenantId = tenantId;
        NameAr = nameAr;
        NameEn = nameEn;
        MenuType = menuType;
        SalesChannel = salesChannel;
        StartDate = startDate;
        EndDate = endDate;
        IsActive = true;

        RaiseDomainEvent(new MenuCreatedEvent(Id, TenantId, NameAr));
    }

    // ─── Sections ─────────────────────────────────────────────────────────────

    public MenuSection AddSection(string nameAr, string? nameEn = null, int sortOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(nameAr))
            throw new BusinessException(ErrorCodes.NameArRequired);

        var section = new MenuSection(TenantId, Id, nameAr, nameEn, sortOrder);
        _sections.Add(section);
        return section;
    }

    public void RemoveSection(Guid sectionId)
    {
        var section = _sections.FirstOrDefault(s => s.Id == sectionId)
            ?? throw new InvalidOperationException($"القسم غير موجود. / Section not found: {sectionId}");
        _sections.Remove(section);
    }

    // ─── Lifecycle ────────────────────────────────────────────────────────────

    public void Activate()
    {
        if (IsActive) return;
        IsActive = true;
        RaiseDomainEvent(new MenuActivatedEvent(Id, TenantId));
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        RaiseDomainEvent(new MenuDeactivatedEvent(Id, TenantId));
    }

    public void UpdateDates(DateOnly? startDate, DateOnly? endDate)
    {
        if (endDate.HasValue && startDate.HasValue && endDate < startDate)
            throw new ArgumentException("تاريخ الانتهاء يجب أن يكون بعد تاريخ البداية. / EndDate must be after StartDate.");
        StartDate = startDate;
        EndDate = endDate;
    }

    public bool IsCurrentlyActive(DateOnly today) =>
        IsActive &&
        (!StartDate.HasValue || StartDate <= today) &&
        (!EndDate.HasValue || EndDate >= today);
}

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// MenuSection — قسم المنيو
/// <para>
/// قسم فرعي داخل المنيو يحتوي على عدة عناصر منتجات.
/// A subsection within a menu containing multiple product items.
/// </para>
/// </summary>
public sealed class MenuSection : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid MenuId { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public string? DescriptionAr { get; private set; }
    public string? DescriptionEn { get; private set; }
    public string? ImageUrl { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<MenuItem> _items = [];
    public IReadOnlyCollection<MenuItem> Items => _items.AsReadOnly();

    private MenuSection() { NameAr = string.Empty; }

    internal MenuSection(Guid tenantId, Guid menuId, string nameAr, string? nameEn, int sortOrder)
    {
        TenantId = tenantId;
        MenuId = menuId;
        NameAr = nameAr;
        NameEn = nameEn;
        SortOrder = sortOrder;
        IsActive = true;
    }

    public void AddItem(Guid productId, decimal? overridePrice = null, int sortOrder = 0)
    {
        if (_items.Any(i => i.ProductId == productId))
            throw new InvalidOperationException("المنتج موجود بالفعل في هذا القسم. / Product already exists in this section.");

        _items.Add(new MenuItem(TenantId, Id, productId, overridePrice, sortOrder));
    }

    public void RemoveItem(Guid productId)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId)
            ?? throw new InvalidOperationException($"المنتج غير موجود في القسم. / Product not found in section: {productId}");
        _items.Remove(item);
    }

    public void UpdateInfo(string nameAr, string? nameEn, string? descriptionAr, string? descriptionEn) { NameAr = nameAr; NameEn = nameEn; DescriptionAr = descriptionAr; DescriptionEn = descriptionEn; }
    public void SetImage(string? imageUrl) => ImageUrl = imageUrl;
    public void SetSortOrder(int order) => SortOrder = order;
    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// MenuItem — عنصر المنيو
/// <para>
/// يربط منتجاً بقسم في المنيو مع إمكانية تجاوز السعر الأساسي.
/// Links a product to a menu section with optional price override.
/// </para>
/// </summary>
public sealed class MenuItem : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid MenuSectionId { get; private set; }
    public Guid ProductId { get; private set; }

    /// <summary>سعر بديل للمنيو فقط (NULL = استخدام السعر الأساسي) / Override price (NULL = use BasePrice)</summary>
    public decimal? OverridePrice { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsVisible { get; private set; }
    public bool IsOutOfStock { get; private set; }

    private MenuItem() { }

    internal MenuItem(Guid tenantId, Guid sectionId, Guid productId, decimal? overridePrice, int sortOrder)
    {
        TenantId = tenantId;
        MenuSectionId = sectionId;
        ProductId = productId;
        OverridePrice = overridePrice;
        SortOrder = sortOrder;
        IsVisible = true;
        IsOutOfStock = false;
    }

    public void SetOverridePrice(decimal? price)
    {
        if (price.HasValue && price < 0)
            throw new ArgumentException("السعر البديل لا يمكن أن يكون سالباً. / Override price cannot be negative.", nameof(price));
        OverridePrice = price;
    }

    public void Hide() => IsVisible = false;
    public void Show() => IsVisible = true;
    public void MarkOutOfStock() => IsOutOfStock = true;
    public void MarkInStock() => IsOutOfStock = false;
    public void SetSortOrder(int order) => SortOrder = order;
}
