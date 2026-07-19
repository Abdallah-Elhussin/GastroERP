using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Sales.Pricing;

/// <summary>
/// قائمة أسعار البيع (Aggregate Root) — مستقلة عن بيانات المنتج.
/// تدعم عدداً غير محدود من القوائم (تجزئة، توصيل، موظفين، VIP…).
/// </summary>
public sealed class SalesPriceList : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public string Code { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public string? Description { get; private set; }
    public SalesChannel? DefaultSalesChannel { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsSystem { get; private set; }
    public bool IsActive { get; private set; }

    private SalesPriceList()
    {
        Code = string.Empty;
        NameAr = string.Empty;
    }

    public SalesPriceList(
        Guid tenantId,
        string code,
        string nameAr,
        string? nameEn = null,
        string? description = null,
        SalesChannel? defaultSalesChannel = null,
        int sortOrder = 0,
        bool isDefault = false,
        bool isSystem = false)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);

        TenantId = tenantId;
        Code = code.Trim().ToUpperInvariant();
        NameAr = nameAr.Trim();
        NameEn = nameEn?.Trim();
        Description = description?.Trim();
        DefaultSalesChannel = defaultSalesChannel;
        SortOrder = Math.Max(0, sortOrder);
        IsDefault = isDefault;
        IsSystem = isSystem;
        IsActive = true;
    }

    public void Update(
        string nameAr,
        string? nameEn,
        string? description,
        SalesChannel? defaultSalesChannel,
        int sortOrder,
        bool isDefault)
    {
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        NameAr = nameAr.Trim();
        NameEn = nameEn?.Trim();
        Description = description?.Trim();
        DefaultSalesChannel = defaultSalesChannel;
        SortOrder = Math.Max(0, sortOrder);
        IsDefault = isDefault;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    public void SoftDeleteList(string? deletedBy)
    {
        if (IsSystem)
            throw new BusinessException(ErrorCodes.CannotModifyApprovedDocument);
        SoftDelete(deletedBy);
        IsActive = false;
        IsDefault = false;
    }
}
