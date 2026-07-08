using GastroErp.Domain.Common;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Menu;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;

namespace GastroErp.Domain.Entities.Menu;

/// <summary>
/// BranchMenu — ربط المنيو بالفرع (Entity)
/// <para>
/// يربط منيواً محدداً بفرع بعينه مع إمكانية تعيين مستوى سعر مختلف.
/// Links a specific Menu to a Branch with optional PriceLevel override.
/// </para>
/// </summary>
public sealed class BranchMenu : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid BranchId { get; private set; }
    public Guid MenuId { get; private set; }
    public Guid? PriceLevelId { get; private set; }
    public bool IsActive { get; private set; }
    public int SortOrder { get; private set; }

    private readonly List<MenuAvailability> _availabilities = [];
    public IReadOnlyCollection<MenuAvailability> Availabilities => _availabilities.AsReadOnly();

    private BranchMenu() { }

    public BranchMenu(Guid tenantId, Guid branchId, Guid menuId, Guid? priceLevelId = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (branchId == Guid.Empty) throw new ArgumentException("BranchId cannot be empty.", nameof(branchId));
        if (menuId == Guid.Empty) throw new ArgumentException("MenuId cannot be empty.", nameof(menuId));

        TenantId = tenantId;
        BranchId = branchId;
        MenuId = menuId;
        PriceLevelId = priceLevelId;
        IsActive = true;
        SortOrder = 0;

        RaiseDomainEvent(new MenuAssignedToBranchEvent(MenuId, BranchId, TenantId));
    }

    /// <summary>
    /// أضف جدول توفر لهذا المنيو في الفرع (يوم + وقت).
    /// Adds a day-specific availability window for this branch menu.
    /// </summary>
    public void SetAvailability(BusinessDayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime)
    {
        if (endTime <= startTime)
            throw new ArgumentException("وقت الانتهاء يجب أن يكون بعد وقت البداية. / EndTime must be after StartTime.");

        var existing = _availabilities.FirstOrDefault(a => a.DayOfWeek == dayOfWeek);
        if (existing is not null)
            existing.UpdateTimes(startTime, endTime);
        else
            _availabilities.Add(new MenuAvailability(TenantId, Id, dayOfWeek, startTime, endTime));
    }

    public void RemoveAvailability(BusinessDayOfWeek dayOfWeek)
    {
        var avail = _availabilities.FirstOrDefault(a => a.DayOfWeek == dayOfWeek);
        if (avail is not null) _availabilities.Remove(avail);
    }

    public void SetPriceLevel(Guid? priceLevelId) => PriceLevelId = priceLevelId;
    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
    public void SetSortOrder(int order) => SortOrder = order;

    /// <summary>هل المنيو متاح الآن بناءً على الوقت الحالي؟ / Is the menu available right now?</summary>
    public bool IsAvailableNow(BusinessDayOfWeek today, TimeOnly currentTime)
    {
        if (!IsActive) return false;
        if (!_availabilities.Any()) return true; // بدون قيود = متاح دائماً

        return _availabilities.Any(a =>
            a.DayOfWeek == today &&
            currentTime >= a.StartTime &&
            currentTime <= a.EndTime);
    }
}

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// MenuAvailability — توفر المنيو في يوم محدد
/// <para>
/// يحدد أوقات الفتح والإغلاق ليوم معين في جدول توفر المنيو.
/// Defines open/close times for a specific day in the menu schedule.
/// </para>
/// </summary>
public sealed class MenuAvailability
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid TenantId { get; private set; }
    public Guid BranchMenuId { get; private set; }
    public BusinessDayOfWeek DayOfWeek { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }

    private MenuAvailability() { }

    internal MenuAvailability(Guid tenantId, Guid branchMenuId,
                               BusinessDayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime)
    {
        TenantId = tenantId;
        BranchMenuId = branchMenuId;
        DayOfWeek = dayOfWeek;
        StartTime = startTime;
        EndTime = endTime;
    }

    internal void UpdateTimes(TimeOnly start, TimeOnly end)
    {
        StartTime = start;
        EndTime = end;
    }
}

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// ComboMeal — وجبة الكومبو (Aggregate Root)
/// <para>
/// تمثّل وجبة مجمّعة تتكون من عدة منتجات بسعر مخفّض.
/// Represents a bundled meal consisting of multiple products at a discounted price.
/// </para>
/// </summary>
public sealed class ComboMeal : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public string? DescriptionAr { get; private set; }
    public string? DescriptionEn { get; private set; }
    public decimal ComboPrice { get; private set; }
    public string Currency { get; private set; }
    public DateOnly? StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public string? ImageUrl { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<ComboItem> _items = [];
    public IReadOnlyCollection<ComboItem> Items => _items.AsReadOnly();

    private ComboMeal()
    {
        NameAr = string.Empty;
        Currency = "SAR";
    }

    public ComboMeal(Guid tenantId, string nameAr, decimal comboPrice,
                      string currency = "SAR", string? nameEn = null,
                      DateOnly? startDate = null, DateOnly? endDate = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        if (comboPrice < 0) throw new ArgumentException("السعر لا يمكن أن يكون سالباً. / Price cannot be negative.", nameof(comboPrice));
        if (endDate.HasValue && startDate.HasValue && endDate < startDate)
            throw new ArgumentException("تاريخ الانتهاء يجب أن يكون بعد تاريخ البداية.", nameof(endDate));

        TenantId = tenantId;
        NameAr = nameAr;
        NameEn = nameEn;
        ComboPrice = comboPrice;
        Currency = currency.ToUpperInvariant();
        StartDate = startDate;
        EndDate = endDate;
        IsActive = true;

        RaiseDomainEvent(new ComboMealCreatedEvent(Id, TenantId, NameAr, ComboPrice));
    }

    /// <summary>أضف عنصراً للكومبو / Adds an item to the combo meal.</summary>
    public void AddItem(Guid productId, int quantity = 1,
                         bool allowSubstitution = false, Guid? substitutionCategoryId = null)
    {
        if (productId == Guid.Empty) throw new ArgumentException("ProductId cannot be empty.", nameof(productId));
        if (quantity < 1) throw new ArgumentException("الكمية يجب أن تكون واحداً على الأقل. / Quantity must be at least 1.", nameof(quantity));

        if (_items.Any(i => i.ProductId == productId))
            throw new InvalidOperationException("المنتج موجود بالفعل في هذه الوجبة. / Product already exists in this combo.");

        _items.Add(new ComboItem(TenantId, Id, productId, quantity, allowSubstitution, substitutionCategoryId, _items.Count));
    }

    public void RemoveItem(Guid productId)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId)
            ?? throw new InvalidOperationException($"المنتج غير موجود في الوجبة. / Product not found in combo: {productId}");
        _items.Remove(item);
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0) throw new ArgumentException("السعر لا يمكن أن يكون سالباً. / Price cannot be negative.", nameof(newPrice));
        ComboPrice = newPrice;
    }

    public void SetImage(string? imageUrl) => ImageUrl = imageUrl;

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        RaiseDomainEvent(new ComboMealDeactivatedEvent(Id, TenantId));
    }

    public void Activate() => IsActive = true;

    public bool IsCurrentlyActive(DateOnly today) =>
        IsActive &&
        (!StartDate.HasValue || StartDate <= today) &&
        (!EndDate.HasValue || EndDate >= today);
}

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// ComboItem — عنصر داخل وجبة الكومبو
/// <para>
/// يحدد منتجاً ضمن الكومبو مع كميته وخيارات الاستبدال المسموحة.
/// Specifies a product within a combo with its quantity and substitution rules.
/// </para>
/// </summary>
public sealed class ComboItem
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid TenantId { get; private set; }
    public Guid ComboMealId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }

    /// <summary>هل يُسمح للعميل باختيار بديل؟ / Is the customer allowed to substitute?</summary>
    public bool AllowSubstitution { get; private set; }

    /// <summary>فئة المنتجات المسموح بالاستبدال منها / Category allowed for substitution.</summary>
    public Guid? SubstitutionCategoryId { get; private set; }
    public int SortOrder { get; private set; }

    private ComboItem() { }

    internal ComboItem(Guid tenantId, Guid comboMealId, Guid productId,
                        int quantity, bool allowSubstitution,
                        Guid? substitutionCategoryId, int sortOrder)
    {
        TenantId = tenantId;
        ComboMealId = comboMealId;
        ProductId = productId;
        Quantity = quantity;
        AllowSubstitution = allowSubstitution;
        SubstitutionCategoryId = substitutionCategoryId;
        SortOrder = sortOrder;
    }

    public void UpdateQuantity(int quantity)
    {
        if (quantity < 1) throw new ArgumentException("الكمية يجب أن تكون واحداً على الأقل.", nameof(quantity));
        Quantity = quantity;
    }
}
