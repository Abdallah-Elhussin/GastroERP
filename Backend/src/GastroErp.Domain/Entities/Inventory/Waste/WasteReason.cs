using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
namespace GastroErp.Domain.Entities.Inventory.Waste;

/// <summary>
/// سبب الهدر (Entity)
/// مثال: منتهي الصلاحية، تالف، خطأ تحضير، مسروق.
/// </summary>
public sealed class WasteReason : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public string? DescriptionAr { get; private set; }
    public string? DescriptionEn { get; private set; }
    public bool IsActive { get; private set; }

    private WasteReason() { NameAr = string.Empty; }

    public WasteReason(Guid tenantId, string nameAr, string? nameEn = null, string? descriptionAr = null, string? descriptionEn = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);

        TenantId = tenantId;
        NameAr = nameAr;
        NameEn = nameEn;
        DescriptionAr = descriptionAr;
        DescriptionEn = descriptionEn;
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
