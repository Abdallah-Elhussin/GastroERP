using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;

namespace GastroErp.Domain.Entities.Finance;

/// <summary>تصنيف حساب تفصيلي يُستخدم عند إنشاء الحسابات في دليل الحسابات.</summary>
public sealed class AccountClassification : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public int Number { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string NameAr { get; private set; } = string.Empty;
    public string NameEn { get; private set; } = string.Empty;
    public Guid MainClassificationId { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsSystem { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int SortOrder { get; private set; }

    private AccountClassification() { }

    public static AccountClassification Create(
        Guid tenantId,
        int number,
        string code,
        string nameAr,
        string nameEn,
        Guid mainClassificationId,
        int sortOrder = 0,
        bool isDefault = false,
        bool isSystem = false)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId required.", nameof(tenantId));
        if (mainClassificationId == Guid.Empty) throw new BusinessException(ErrorCodes.AccountMainClassificationRequired);
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        if (string.IsNullOrWhiteSpace(nameEn)) throw new BusinessException(ErrorCodes.NameEnRequired);

        var normalizedCode = string.IsNullOrWhiteSpace(code)
            ? Slugify(nameEn)
            : code.Trim().ToLowerInvariant().Replace(' ', '_');

        return new AccountClassification
        {
            TenantId = tenantId,
            Number = number,
            Code = normalizedCode,
            NameAr = nameAr.Trim(),
            NameEn = nameEn.Trim(),
            MainClassificationId = mainClassificationId,
            SortOrder = sortOrder,
            IsDefault = isDefault,
            IsSystem = isSystem,
            IsActive = true
        };
    }

    public void Update(string nameAr, string nameEn, Guid mainClassificationId)
    {
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        if (string.IsNullOrWhiteSpace(nameEn)) throw new BusinessException(ErrorCodes.NameEnRequired);
        if (mainClassificationId == Guid.Empty) throw new BusinessException(ErrorCodes.AccountMainClassificationRequired);

        NameAr = nameAr.Trim();
        NameEn = nameEn.Trim();
        MainClassificationId = mainClassificationId;
    }

    public void EnsureCanDelete()
    {
        if (IsDefault || IsSystem)
            throw new BusinessException(ErrorCodes.AccountClassificationProtected);
    }

    private static string Slugify(string value)
    {
        var chars = value.Trim().ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '_')
            .ToArray();
        var slug = new string(chars);
        while (slug.Contains("__", StringComparison.Ordinal))
            slug = slug.Replace("__", "_", StringComparison.Ordinal);
        return slug.Trim('_');
    }
}
