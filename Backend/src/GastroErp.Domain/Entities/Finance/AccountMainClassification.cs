using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Finance;

/// <summary>التصنيف الرئيسي للحسابات (الأصول، الخصوم، …) — يُحمَّل من قاعدة البيانات.</summary>
public sealed class AccountMainClassification : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string NameAr { get; private set; } = string.Empty;
    public string NameEn { get; private set; } = string.Empty;
    public AccountType AccountType { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsSystem { get; private set; }
    public bool IsActive { get; private set; } = true;

    private AccountMainClassification() { }

    public static AccountMainClassification Create(
        Guid tenantId, string code, string nameAr, string nameEn, AccountType accountType, int sortOrder, bool isSystem = true)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId required.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(code)) throw new BusinessException(ErrorCodes.NameArRequired);
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        if (string.IsNullOrWhiteSpace(nameEn)) throw new BusinessException(ErrorCodes.NameEnRequired);

        return new AccountMainClassification
        {
            TenantId = tenantId,
            Code = code.Trim().ToLowerInvariant(),
            NameAr = nameAr.Trim(),
            NameEn = nameEn.Trim(),
            AccountType = accountType,
            SortOrder = sortOrder,
            IsSystem = isSystem,
            IsActive = true
        };
    }
}
