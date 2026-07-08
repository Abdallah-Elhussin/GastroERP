using GastroErp.Domain.Common;
using GastroErp.Domain.Events.Menu;
using GastroErp.Domain.ValueObjects;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;

namespace GastroErp.Domain.Entities.Menu;

/// <summary>
/// Product — المنتج (Aggregate Root)
/// <para>
/// يمثّل منتجاً واحداً في كتالوج الشركة (صنف أو طبق).
/// Represents a single product in the company catalog (dish or item).
/// </para>
/// <para>
/// الـ Product هو الجذر الذي يحتوي على:
/// - مجموعات الإضافات (ModifierGroups)
/// - مجموعات الخيارات (OptionGroups)
/// - الصور (ProductImages)
/// - أسعار متعددة (PriceLevel)
/// </para>
/// </summary>
public sealed class Product : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid CategoryId { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public string? DescriptionAr { get; private set; }
    public string? DescriptionEn { get; private set; }
    public string? SKU { get; private set; }
    public string? Barcode { get; private set; }
    public decimal BasePrice { get; private set; }
    public string Currency { get; private set; }
    public int? CaloriesMin { get; private set; }
    public int? CaloriesMax { get; private set; }
    public int PrepTimeMinutes { get; private set; }
    public bool IsAvailable { get; private set; }
    public bool IsFeatured { get; private set; }
    public int SortOrder { get; private set; }

    private readonly List<ModifierGroup> _modifierGroups = [];
    public IReadOnlyCollection<ModifierGroup> ModifierGroups => _modifierGroups.AsReadOnly();

    private readonly List<OptionGroup> _optionGroups = [];
    public IReadOnlyCollection<OptionGroup> OptionGroups => _optionGroups.AsReadOnly();

    private readonly List<ProductImage> _images = [];
    public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();

    // أسعار المستويات (ProductId + PriceLevelId → Price)
    private readonly List<ProductPriceLevel> _priceLevels = [];
    public IReadOnlyCollection<ProductPriceLevel> PriceLevels => _priceLevels.AsReadOnly();

    private Product()
    {
        NameAr = string.Empty;
        Currency = "SAR";
    }

    public Product(Guid tenantId, Guid categoryId, string nameAr, decimal basePrice,
                   string currency = "SAR", string? nameEn = null, string? sku = null)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId لا يمكن أن يكون فارغاً. / TenantId cannot be empty.", nameof(tenantId));
        if (categoryId == Guid.Empty)
            throw new ArgumentException("CategoryId لا يمكن أن يكون فارغاً. / CategoryId cannot be empty.", nameof(categoryId));
        if (string.IsNullOrWhiteSpace(nameAr))
            throw new BusinessException(ErrorCodes.NameArRequired);
        if (basePrice < 0)
            throw new ArgumentException("السعر لا يمكن أن يكون سالباً. / Price cannot be negative.", nameof(basePrice));

        TenantId = tenantId;
        CategoryId = categoryId;
        NameAr = nameAr;
        NameEn = nameEn;
        BasePrice = basePrice;
        Currency = currency.ToUpperInvariant();
        SKU = sku;
        IsAvailable = true;
        PrepTimeMinutes = 0;
        SortOrder = 0;

        RaiseDomainEvent(new ProductCreatedEvent(Id, TenantId, CategoryId, NameAr));
    }

    // ─── Modifier Groups ──────────────────────────────────────────────────────

    public void AddModifierGroup(string nameAr, string? nameEn, int minSelection, int maxSelection,
                                  bool isRequired = false)
    {
        if (maxSelection < minSelection)
            throw new ArgumentException("الحد الأقصى يجب أن يكون أكبر من أو يساوي الحد الأدنى. / MaxSelection must be >= MinSelection.");

        var group = new ModifierGroup(TenantId, Id, nameAr, nameEn, minSelection, maxSelection, isRequired);
        _modifierGroups.Add(group);
    }

    public void RemoveModifierGroup(Guid groupId)
    {
        var group = _modifierGroups.FirstOrDefault(g => g.Id == groupId)
            ?? throw new InvalidOperationException($"مجموعة الإضافات غير موجودة. / Modifier group not found: {groupId}");
        _modifierGroups.Remove(group);
    }

    // ─── Option Groups ────────────────────────────────────────────────────────

    public void AddOptionGroup(string nameAr, string? nameEn, bool isRequired = false)
    {
        var group = new OptionGroup(TenantId, Id, nameAr, nameEn, isRequired);
        _optionGroups.Add(group);
    }

    public void RemoveOptionGroup(Guid groupId)
    {
        var group = _optionGroups.FirstOrDefault(g => g.Id == groupId)
            ?? throw new InvalidOperationException($"مجموعة الخيارات غير موجودة. / Option group not found: {groupId}");
        _optionGroups.Remove(group);
    }

    // ─── Images ───────────────────────────────────────────────────────────────

    public void AddImage(string imageUrl, string? thumbnailUrl = null,
                          string? altText = null, bool isPrimary = false)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("رابط الصورة لا يمكن أن يكون فارغاً. / Image URL cannot be empty.", nameof(imageUrl));

        if (isPrimary)
        {
            // ازل علامة Primary من الصور الأخرى
            foreach (var img in _images) img.SetAsPrimary(false);
        }

        _images.Add(new ProductImage(TenantId, Id, imageUrl, thumbnailUrl, altText, isPrimary, _images.Count));
    }

    public void RemoveImage(Guid imageId)
    {
        var image = _images.FirstOrDefault(i => i.Id == imageId)
            ?? throw new InvalidOperationException("Image not found.");
        _images.Remove(image);
    }

    public void SetPrimaryImage(Guid imageId)
    {
        var image = _images.FirstOrDefault(i => i.Id == imageId)
            ?? throw new InvalidOperationException("Image not found.");
        foreach (var img in _images) img.SetAsPrimary(false);
        image.SetAsPrimary(true);
    }

    // ─── Price Levels ─────────────────────────────────────────────────────────

    public void SetPriceLevel(Guid priceLevelId, decimal price)
    {
        if (price < 0) throw new ArgumentException("السعر لا يمكن أن يكون سالباً. / Price cannot be negative.", nameof(price));

        var existing = _priceLevels.FirstOrDefault(p => p.PriceLevelId == priceLevelId);
        if (existing is not null)
            existing.UpdatePrice(price);
        else
            _priceLevels.Add(new ProductPriceLevel(Id, TenantId, priceLevelId, price));
    }

    // ─── Domain Methods ───────────────────────────────────────────────────────

    public void UpdateInfo(string nameAr, string? nameEn, string? descriptionAr, string? descriptionEn,
                            string? sku, string? barcode, int? caloriesMin, int? caloriesMax, int prepTime)
    {
        if (string.IsNullOrWhiteSpace(nameAr))
            throw new BusinessException(ErrorCodes.NameArRequired);

        NameAr = nameAr;
        NameEn = nameEn;
        DescriptionAr = descriptionAr;
        DescriptionEn = descriptionEn;
        SKU = sku;
        Barcode = barcode;
        CaloriesMin = caloriesMin;
        CaloriesMax = caloriesMax;
        PrepTimeMinutes = prepTime >= 0 ? prepTime : 0;
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0)
            throw new ArgumentException("السعر لا يمكن أن يكون سالباً. / Price cannot be negative.", nameof(newPrice));

        var old = BasePrice;
        BasePrice = newPrice;
        RaiseDomainEvent(new ProductPriceChangedEvent(Id, TenantId, old, newPrice));
    }

    public void MarkUnavailable(string reason)
    {
        if (!IsAvailable) return;
        IsAvailable = false;
        RaiseDomainEvent(new ProductUnavailableEvent(Id, TenantId, reason));
    }

    public void MarkAvailable() => IsAvailable = true;
    public void SetFeatured(bool featured) => IsFeatured = featured;
    public void SetCategory(Guid categoryId)
    {
        if (categoryId == Guid.Empty) throw new ArgumentException("CategoryId cannot be empty.", nameof(categoryId));
        CategoryId = categoryId;
    }
    public void SetSortOrder(int order) => SortOrder = order >= 0 ? order : 0;

    public bool HasModifiers => _modifierGroups.Count > 0;
}

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// ModifierGroup — مجموعة الإضافات
/// <para>
/// تحتوي على مجموعة من الإضافات (مثل: الحجم، المكونات الإضافية).
/// يتم تحديد ما إذا كانت الاختيارات إلزامية أو اختيارية عبر MinSelection.
/// </para>
/// </summary>
public sealed class ModifierGroup : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid ProductId { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public int MinSelection { get; private set; }
    public int MaxSelection { get; private set; }
    public bool IsRequired { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<Modifier> _modifiers = [];
    public IReadOnlyCollection<Modifier> Modifiers => _modifiers.AsReadOnly();

    private ModifierGroup() { NameAr = string.Empty; }

    internal ModifierGroup(Guid tenantId, Guid productId, string nameAr, string? nameEn,
                            int minSelection, int maxSelection, bool isRequired)
    {
        TenantId = tenantId;
        ProductId = productId;
        NameAr = nameAr;
        NameEn = nameEn;
        MinSelection = minSelection;
        MaxSelection = maxSelection;
        IsRequired = isRequired;
        IsActive = true;
    }

    public void AddModifier(string nameAr, string? nameEn, decimal extraPrice = 0, bool isDefault = false)
    {
        if (extraPrice < 0) throw new ArgumentException("السعر الإضافي لا يمكن أن يكون سالباً. / ExtraPrice cannot be negative.");
        if (isDefault && _modifiers.Any(m => m.IsDefault))
            throw new InvalidOperationException("يوجد بالفعل خيار افتراضي. / A default modifier already exists.");

        _modifiers.Add(new Modifier(TenantId, Id, nameAr, nameEn, extraPrice, isDefault, _modifiers.Count));
    }

    public void RemoveModifier(Guid modifierId)
    {
        var modifier = _modifiers.FirstOrDefault(m => m.Id == modifierId)
            ?? throw new InvalidOperationException($"الإضافة غير موجودة. / Modifier not found: {modifierId}");
        _modifiers.Remove(modifier);
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}

/// <summary>Modifier — الإضافة الواحدة داخل مجموعة الإضافات</summary>
public sealed class Modifier : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid ModifierGroupId { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public decimal ExtraPrice { get; private set; }
    public int? CaloriesExtra { get; private set; }
    public bool IsDefault { get; internal set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; }

    private Modifier() { NameAr = string.Empty; }

    internal Modifier(Guid tenantId, Guid groupId, string nameAr, string? nameEn,
                       decimal extraPrice, bool isDefault, int sortOrder)
    {
        TenantId = tenantId;
        ModifierGroupId = groupId;
        NameAr = nameAr;
        NameEn = nameEn;
        ExtraPrice = extraPrice;
        IsDefault = isDefault;
        SortOrder = sortOrder;
        IsActive = true;
    }

    public void UpdatePrice(decimal price)
    {
        if (price < 0) throw new ArgumentException("السعر لا يمكن أن يكون سالباً.", nameof(price));
        ExtraPrice = price;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// OptionGroup — مجموعة الخيارات
/// <para>
/// تمثّل خيارات طلب المنتج (مثل: درجة الحرارة، مستوى الحدة، ملاحظات).
/// الفرق عن ModifierGroup: الخيارات لا تؤثر في السعر في العادة وهي وصفية.
/// </para>
/// </summary>
public sealed class OptionGroup : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid ProductId { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public bool IsRequired { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<Option> _options = [];
    public IReadOnlyCollection<Option> Options => _options.AsReadOnly();

    private OptionGroup() { NameAr = string.Empty; }

    internal OptionGroup(Guid tenantId, Guid productId, string nameAr, string? nameEn, bool isRequired)
    {
        TenantId = tenantId;
        ProductId = productId;
        NameAr = nameAr;
        NameEn = nameEn;
        IsRequired = isRequired;
        IsActive = true;
    }

    public void AddOption(string nameAr, string? nameEn, decimal extraPrice = 0, bool isDefault = false)
    {
        _options.Add(new Option(TenantId, Id, nameAr, nameEn, extraPrice, isDefault, _options.Count));
    }

    public void RemoveOption(Guid optionId)
    {
        var option = _options.FirstOrDefault(o => o.Id == optionId)
            ?? throw new InvalidOperationException($"الخيار غير موجود. / Option not found: {optionId}");
        _options.Remove(option);
    }

    public void UpdateInfo(string nameAr, string? nameEn, bool isRequired)
    {
        if (string.IsNullOrWhiteSpace(nameAr))
            throw new ArgumentException("الاسم العربي مطلوب. / NameAr is required.", nameof(nameAr));
        NameAr = nameAr;
        NameEn = nameEn;
        IsRequired = isRequired;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}

/// <summary>Option — خيار واحد داخل مجموعة الخيارات</summary>
public sealed class Option : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid OptionGroupId { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public decimal ExtraPrice { get; private set; }
    public bool IsDefault { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; }

    private Option() { NameAr = string.Empty; }

    internal Option(Guid tenantId, Guid groupId, string nameAr, string? nameEn,
                     decimal extraPrice, bool isDefault, int sortOrder)
    {
        TenantId = tenantId;
        OptionGroupId = groupId;
        NameAr = nameAr;
        NameEn = nameEn;
        ExtraPrice = extraPrice;
        IsDefault = isDefault;
        SortOrder = sortOrder;
        IsActive = true;
    }

    public void UpdateInfo(string nameAr, string? nameEn, decimal extraPrice, bool isDefault)
    {
        if (string.IsNullOrWhiteSpace(nameAr))
            throw new ArgumentException("الاسم العربي مطلوب. / NameAr is required.", nameof(nameAr));
        if (extraPrice < 0)
            throw new ArgumentException("السعر لا يمكن أن يكون سالباً. / Price cannot be negative.", nameof(extraPrice));
        NameAr = nameAr;
        NameEn = nameEn;
        ExtraPrice = extraPrice;
        IsDefault = isDefault;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>ProductImage — صورة المنتج</summary>
public sealed class ProductImage
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid TenantId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ImageUrl { get; private set; }
    public string? ThumbnailUrl { get; private set; }
    public string? AltText { get; private set; }
    public bool IsPrimary { get; private set; }
    public int SortOrder { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; private set; }

    private ProductImage() { ImageUrl = string.Empty; }

    internal ProductImage(Guid tenantId, Guid productId, string imageUrl,
                           string? thumbnailUrl, string? altText, bool isPrimary, int sortOrder)
    {
        TenantId = tenantId;
        ProductId = productId;
        ImageUrl = imageUrl;
        ThumbnailUrl = thumbnailUrl;
        AltText = altText;
        IsPrimary = isPrimary;
        SortOrder = sortOrder;
    }

    internal void SetAsPrimary(bool isPrimary) => IsPrimary = isPrimary;
}

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>ProductPriceLevel — سعر المنتج لمستوى تسعير معين</summary>
public sealed class ProductPriceLevel
{
    public Guid ProductId { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid PriceLevelId { get; private set; }
    public decimal Price { get; private set; }

    private ProductPriceLevel() { }

    internal ProductPriceLevel(Guid productId, Guid tenantId, Guid priceLevelId, decimal price)
    {
        ProductId = productId;
        TenantId = tenantId;
        PriceLevelId = priceLevelId;
        Price = price;
    }

    internal void UpdatePrice(decimal price) => Price = price;
}
