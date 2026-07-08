using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Inventory.Recipe;

/// <summary>
/// الوصفة (Aggregate Root)
/// تحدد المكونات (BOM) لمنتج من المنيو وطريقة تحضيره ونسبة الهدر المئوية.
/// </summary>
public sealed class Recipe : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }

    /// <summary>المنتج النهائي الذي تنتجه هذه الوصفة (من وحدة المنيو)</summary>
    public Guid ProductId { get; private set; }

    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public string? Instructions { get; private set; }

    /// <summary>الكمية الناتجة عن هذه الوصفة (مثلاً: 1 حبة، أو 10 لتر للوصفات المجمعة)</summary>
    public decimal Yield { get; private set; }

    /// <summary>نسبة الهدر المئوية المتوقعة (مثلاً: 5%)</summary>
    public decimal WastePercentage { get; private set; }

    /// <summary>وقت التحضير بالدقائق</summary>
    public int PreparationTime { get; private set; }

    /// <summary>رقم الإصدار (لتتبع تحديثات الوصفات دون حذف التاريخ)</summary>
    public int Version { get; private set; }

    public RecipeStatus Status { get; private set; }

    private readonly List<RecipeItem> _items = [];
    public IReadOnlyCollection<RecipeItem> Items => _items.AsReadOnly();

    private Recipe() { NameAr = string.Empty; }

    public Recipe(Guid tenantId, Guid productId, string nameAr, string? nameEn, decimal yield,
                  decimal wastePercentage = 0, int preparationTime = 0)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (productId == Guid.Empty) throw new ArgumentException("ProductId cannot be empty.", nameof(productId));
        if (string.IsNullOrWhiteSpace(nameAr)) throw new GastroErp.Domain.Common.Exceptions.BusinessException(GastroErp.Domain.Common.Localization.ErrorCodes.NameArRequired);
        if (yield <= 0) throw new ArgumentException("Yield must be greater than zero.", nameof(yield));
        if (wastePercentage < 0 || wastePercentage >= 100) throw new ArgumentException("WastePercentage must be between 0 and 99.", nameof(wastePercentage));
        if (preparationTime < 0) throw new ArgumentException("PreparationTime cannot be negative.", nameof(preparationTime));

        TenantId = tenantId;
        ProductId = productId;
        NameAr = nameAr;
        NameEn = nameEn;
        Yield = yield;
        WastePercentage = wastePercentage;
        PreparationTime = preparationTime;
        Version = 1;
        Status = RecipeStatus.Draft;
    }

    public void AddItem(Guid inventoryItemId, Guid unitId, decimal quantity, decimal itemWastePercentage = 0)
    {
        if (_items.Any(i => i.InventoryItemId == inventoryItemId))
            throw new BusinessException(ErrorCodes.ItemAlreadyAdded);

        _items.Add(new RecipeItem(TenantId, Id, inventoryItemId, unitId, quantity, itemWastePercentage));
    }

    public void RemoveItem(Guid inventoryItemId)
    {
        var item = _items.FirstOrDefault(i => i.InventoryItemId == inventoryItemId)
            ?? throw new BusinessException(ErrorCodes.ItemNotFound);
        _items.Remove(item);
    }

    public void UpdateInstructions(string? instructions) => Instructions = instructions;

    public void Activate() => Status = RecipeStatus.Active;
    public void MarkObsolete() => Status = RecipeStatus.Obsolete;

    public void CreateNewVersion(decimal newYield, decimal newWastePercentage, int newPrepTime)
    {
        Yield = newYield;
        WastePercentage = newWastePercentage;
        PreparationTime = newPrepTime;
        Version++;
    }
}

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// عنصر الوصفة (المكون الخام)
/// </summary>
public sealed class RecipeItem : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid RecipeId { get; private set; }
    public Guid InventoryItemId { get; private set; }
    public Guid UnitId { get; private set; }
    public decimal Quantity { get; private set; }

    /// <summary>نسبة الهدر المخصصة لهذا المكون بعينه (مثلاً: تنظيف اللحم)</summary>
    public decimal WastePercentage { get; private set; }

    private RecipeItem() { }

    internal RecipeItem(Guid tenantId, Guid recipeId, Guid inventoryItemId, Guid unitId, decimal quantity, decimal wastePercentage)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        if (wastePercentage < 0 || wastePercentage >= 100) throw new ArgumentException("WastePercentage must be between 0 and 99.", nameof(wastePercentage));

        TenantId = tenantId;
        RecipeId = recipeId;
        InventoryItemId = inventoryItemId;
        UnitId = unitId;
        Quantity = quantity;
        WastePercentage = wastePercentage;
    }

    public void UpdateQuantity(decimal quantity, decimal wastePercentage)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        if (wastePercentage < 0 || wastePercentage >= 100) throw new ArgumentException("WastePercentage must be between 0 and 99.", nameof(wastePercentage));

        Quantity = quantity;
        WastePercentage = wastePercentage;
    }
}
