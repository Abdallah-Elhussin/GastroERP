using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Inventory.Catalog;

/// <summary>
/// نوع الصنف المخزني (Aggregate Root) — يحدد سلوك الصنف في المبيعات/المشتريات/الإنتاج/الوصفات.
/// </summary>
public sealed class InventoryItemType : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid? CompanyId { get; private set; }
    public string Code { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public string? Description { get; private set; }
    public InventoryItemTypeCategory Category { get; private set; }
    public int? CodeStart { get; private set; }
    public int? CodeEnd { get; private set; }
    public bool IsInventory { get; private set; }
    public bool CanSell { get; private set; }
    public bool CanPurchase { get; private set; }
    public bool IsRecipe { get; private set; }
    public bool IsProduction { get; private set; }
    public bool AllowNegativeStock { get; private set; }
    public string Color { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsSystem { get; private set; }
    public bool IsActive { get; private set; }

    private InventoryItemType()
    {
        Code = string.Empty;
        NameAr = string.Empty;
        Color = "#FFFFFF";
    }

    public InventoryItemType(
        Guid tenantId,
        string code,
        string nameAr,
        InventoryItemTypeCategory category,
        string? nameEn = null,
        string? description = null,
        int? codeStart = null,
        int? codeEnd = null,
        bool isInventory = true,
        bool canSell = false,
        bool canPurchase = false,
        bool isRecipe = false,
        bool isProduction = false,
        bool allowNegativeStock = false,
        string? color = null,
        int sortOrder = 0,
        bool isSystem = false,
        Guid? companyId = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);

        TenantId = tenantId;
        CompanyId = companyId;
        Code = code.Trim().ToUpperInvariant();
        NameAr = nameAr.Trim();
        NameEn = nameEn?.Trim();
        Description = description?.Trim();
        Category = category;
        CodeStart = codeStart;
        CodeEnd = codeEnd;
        IsInventory = isInventory;
        CanSell = canSell;
        CanPurchase = canPurchase;
        IsRecipe = isRecipe;
        IsProduction = isProduction;
        AllowNegativeStock = allowNegativeStock;
        Color = string.IsNullOrWhiteSpace(color) ? DefaultColor(category) : color.Trim();
        SortOrder = sortOrder;
        IsSystem = isSystem;
        IsActive = true;

        EnsureInvariants();
    }

    public void Update(
        string code,
        string nameAr,
        string? nameEn,
        string? description,
        InventoryItemTypeCategory category,
        int? codeStart,
        int? codeEnd,
        bool isInventory,
        bool canSell,
        bool canPurchase,
        bool isRecipe,
        bool isProduction,
        bool allowNegativeStock,
        string? color,
        int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);

        // System types keep their code; custom types may rename code.
        if (!IsSystem)
        {
            if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code is required.", nameof(code));
            Code = code.Trim().ToUpperInvariant();
        }

        NameAr = nameAr.Trim();
        NameEn = nameEn?.Trim();
        Description = description?.Trim();
        Category = category;
        CodeStart = codeStart;
        CodeEnd = codeEnd;
        IsInventory = isInventory;
        CanSell = canSell;
        CanPurchase = canPurchase;
        IsRecipe = isRecipe;
        IsProduction = isProduction;
        AllowNegativeStock = allowNegativeStock;
        Color = string.IsNullOrWhiteSpace(color) ? DefaultColor(category) : color.Trim();
        SortOrder = sortOrder;

        EnsureInvariants();
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    public void SoftDeleteType()
    {
        if (IsSystem)
            throw new BusinessException(ErrorCodes.CannotModifyApprovedDocument);
        SoftDelete("system");
    }

    private void EnsureInvariants()
    {
        if (CodeStart.HasValue && CodeEnd.HasValue && CodeStart.Value > CodeEnd.Value)
            throw new ArgumentException("CodeStart cannot be greater than CodeEnd.");

        if (CanSell)
        {
            var sellable = Category is InventoryItemTypeCategory.MenuItem
                or InventoryItemTypeCategory.FinishedProduct
                or InventoryItemTypeCategory.Bundle
                or InventoryItemTypeCategory.Service;
            if (!sellable)
                throw new BusinessException(ErrorCodes.InvalidStatusTransition);
        }

        if (IsRecipe && !IsInventory && !IsProduction)
            throw new BusinessException(ErrorCodes.InvalidStatusTransition);

        if (!IsInventory)
            AllowNegativeStock = false;
    }

    public static string DefaultColor(InventoryItemTypeCategory category) => category switch
    {
        InventoryItemTypeCategory.RawMaterial => "#FFF5E6",
        InventoryItemTypeCategory.PackagingMaterial => "#EAF8F5",
        InventoryItemTypeCategory.FinishedProduct => "#EEF6FF",
        InventoryItemTypeCategory.SemiFinishedProduct => "#FFF2CC",
        InventoryItemTypeCategory.RecipeComponent => "#FFF8E7",
        InventoryItemTypeCategory.MenuItem => "#E3F2FD",
        InventoryItemTypeCategory.Bundle => "#F5F0FF",
        InventoryItemTypeCategory.Service => "#F3F3F3",
        InventoryItemTypeCategory.FixedAsset => "#E8F5E9",
        InventoryItemTypeCategory.PromotionItem => "#FCE4EC",
        _ => "#FFFFFF"
    };
}
