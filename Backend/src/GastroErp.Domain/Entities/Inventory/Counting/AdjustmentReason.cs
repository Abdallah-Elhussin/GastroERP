using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
namespace GastroErp.Domain.Entities.Inventory.Counting;

/// <summary>
/// سبب التسوية (Entity)
/// مثال: فقدان، تلف، خطأ يدوي، فرق جرد.
/// </summary>
public sealed class AdjustmentReason : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public string? DescriptionAr { get; private set; }
    public string? DescriptionEn { get; private set; }
    public bool IsActive { get; private set; }

    private AdjustmentReason() { NameAr = string.Empty; }

    public AdjustmentReason(Guid tenantId, string nameAr, string? nameEn = null, string? descriptionAr = null, string? descriptionEn = null)
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
