using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;

namespace GastroErp.Domain.Entities.Finance;

public enum TaxpayerType
{
    Company = 1,
    Establishment = 2,
    Individual = 3,
    Government = 4,
    Association = 5,
    Organization = 6
}

public enum TaxRegistrationStatus
{
    Active = 1,
    Suspended = 2,
    Expired = 3,
    Cancelled = 4
}

/// <summary>
/// ملف تسجيل ضريبي للشركة/الفرع — يستخدم للفواتير والإقرارات والفوترة الإلكترونية.
/// </summary>
public sealed class TaxRegistrationProfile : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public int Number { get; private set; }
    public Guid CompanyId { get; private set; }
    public Guid? BranchId { get; private set; }
    public string VatNumber { get; private set; } = string.Empty;
    public string? BranchVatNumber { get; private set; }
    public string? TaxOffice { get; private set; }
    public TaxpayerType TaxpayerType { get; private set; } = TaxpayerType.Company;
    public string? ActivityCode { get; private set; }
    public string? ActivityNameAr { get; private set; }
    public string? ActivityNameEn { get; private set; }
    public decimal DefaultTaxRate { get; private set; } = 15m;
    public DateOnly? RegistrationDate { get; private set; }
    public DateOnly? ExpiryDate { get; private set; }
    public TaxRegistrationStatus Status { get; private set; } = TaxRegistrationStatus.Active;
    public string? Notes { get; private set; }
    public bool IsSystem { get; private set; }
    public int SortOrder { get; private set; }
    public bool HasBeenUsed { get; private set; }

    private readonly List<TaxRegistrationCertificate> _certificates = [];
    public IReadOnlyCollection<TaxRegistrationCertificate> Certificates => _certificates.AsReadOnly();

    public TaxRegistrationCertificate? CurrentCertificate
        => _certificates.OrderByDescending(c => c.Version).FirstOrDefault(c => c.IsCurrent);

    private TaxRegistrationProfile() { }

    public static TaxRegistrationProfile Create(
        Guid tenantId,
        int number,
        Guid companyId,
        string vatNumber,
        Guid? branchId = null,
        string? branchVatNumber = null,
        string? taxOffice = null,
        TaxpayerType taxpayerType = TaxpayerType.Company,
        string? activityCode = null,
        string? activityNameAr = null,
        string? activityNameEn = null,
        decimal defaultTaxRate = 15m,
        DateOnly? registrationDate = null,
        DateOnly? expiryDate = null,
        string? notes = null,
        int sortOrder = 0,
        bool isSystem = false)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId required.", nameof(tenantId));
        if (companyId == Guid.Empty) throw new BusinessException(ErrorCodes.RequiredField);
        if (string.IsNullOrWhiteSpace(vatNumber)) throw new BusinessException(ErrorCodes.RequiredField);
        if (defaultTaxRate is < 0 or > 100) throw new BusinessException(ErrorCodes.TaxRegistrationRateInvalid);
        if (registrationDate is DateOnly reg && expiryDate is DateOnly exp && exp < reg)
            throw new BusinessException(ErrorCodes.TaxRegistrationDatesInvalid);

        return new TaxRegistrationProfile
        {
            TenantId = tenantId,
            Number = number,
            CompanyId = companyId,
            BranchId = branchId == Guid.Empty ? null : branchId,
            VatNumber = vatNumber.Trim(),
            BranchVatNumber = Normalize(branchVatNumber),
            TaxOffice = Normalize(taxOffice),
            TaxpayerType = taxpayerType,
            ActivityCode = Normalize(activityCode),
            ActivityNameAr = Normalize(activityNameAr),
            ActivityNameEn = Normalize(activityNameEn),
            DefaultTaxRate = defaultTaxRate,
            RegistrationDate = registrationDate,
            ExpiryDate = expiryDate,
            Notes = Normalize(notes),
            SortOrder = sortOrder,
            IsSystem = isSystem,
            Status = TaxRegistrationStatus.Active
        };
    }

    public void Update(
        Guid companyId,
        Guid? branchId,
        string vatNumber,
        string? branchVatNumber,
        string? taxOffice,
        TaxpayerType taxpayerType,
        string? activityCode,
        string? activityNameAr,
        string? activityNameEn,
        decimal defaultTaxRate,
        DateOnly? registrationDate,
        DateOnly? expiryDate,
        string? notes,
        int sortOrder)
    {
        if (companyId == Guid.Empty) throw new BusinessException(ErrorCodes.RequiredField);
        if (string.IsNullOrWhiteSpace(vatNumber)) throw new BusinessException(ErrorCodes.RequiredField);
        if (defaultTaxRate is < 0 or > 100) throw new BusinessException(ErrorCodes.TaxRegistrationRateInvalid);
        if (registrationDate is DateOnly reg && expiryDate is DateOnly exp && exp < reg)
            throw new BusinessException(ErrorCodes.TaxRegistrationDatesInvalid);

        CompanyId = companyId;
        BranchId = branchId == Guid.Empty ? null : branchId;
        VatNumber = vatNumber.Trim();
        BranchVatNumber = Normalize(branchVatNumber);
        TaxOffice = Normalize(taxOffice);
        TaxpayerType = taxpayerType;
        ActivityCode = Normalize(activityCode);
        ActivityNameAr = Normalize(activityNameAr);
        ActivityNameEn = Normalize(activityNameEn);
        DefaultTaxRate = defaultTaxRate;
        RegistrationDate = registrationDate;
        ExpiryDate = expiryDate;
        Notes = Normalize(notes);
        SortOrder = sortOrder;
    }

    public void SetStatus(TaxRegistrationStatus status) => Status = status;

    public void Activate() => Status = TaxRegistrationStatus.Active;

    public void Suspend() => Status = TaxRegistrationStatus.Suspended;

    public TaxRegistrationCertificate AddCertificate(
        string fileName,
        string storagePath,
        string? contentType = null,
        string? documentNumber = null,
        DateOnly? issueDate = null,
        DateOnly? expiryDate = null,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(storagePath))
            throw new BusinessException(ErrorCodes.RequiredField);

        foreach (var existing in _certificates.Where(c => c.IsCurrent))
            existing.ClearCurrent();

        var version = _certificates.Count == 0 ? 1 : _certificates.Max(c => c.Version) + 1;
        var cert = TaxRegistrationCertificate.Create(
            Id, version, fileName, storagePath, contentType, documentNumber, issueDate, expiryDate, notes);
        _certificates.Add(cert);
        return cert;
    }

    public void MarkUsed() => HasBeenUsed = true;

    public void EnsureCanDelete()
    {
        if (IsSystem) throw new BusinessException(ErrorCodes.TaxRegistrationProtected);
        if (HasBeenUsed) throw new BusinessException(ErrorCodes.TaxRegistrationInUse);
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

/// <summary>إصدار شهادة تسجيل ضريبي (مع versioning).</summary>
public sealed class TaxRegistrationCertificate
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid TaxRegistrationProfileId { get; private set; }
    public int Version { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string StoragePath { get; private set; } = string.Empty;
    public string? ContentType { get; private set; }
    public string? DocumentNumber { get; private set; }
    public DateOnly? IssueDate { get; private set; }
    public DateOnly? ExpiryDate { get; private set; }
    public string? Notes { get; private set; }
    public bool IsCurrent { get; private set; } = true;
    public DateTimeOffset UploadedAt { get; private set; } = DateTimeOffset.UtcNow;

    private TaxRegistrationCertificate() { }

    public static TaxRegistrationCertificate Create(
        Guid profileId,
        int version,
        string fileName,
        string storagePath,
        string? contentType,
        string? documentNumber,
        DateOnly? issueDate,
        DateOnly? expiryDate,
        string? notes)
    {
        return new TaxRegistrationCertificate
        {
            TaxRegistrationProfileId = profileId,
            Version = version,
            FileName = fileName.Trim(),
            StoragePath = storagePath.Trim(),
            ContentType = string.IsNullOrWhiteSpace(contentType) ? null : contentType.Trim(),
            DocumentNumber = string.IsNullOrWhiteSpace(documentNumber) ? null : documentNumber.Trim(),
            IssueDate = issueDate,
            ExpiryDate = expiryDate,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            IsCurrent = true,
            UploadedAt = DateTimeOffset.UtcNow
        };
    }

    internal void ClearCurrent() => IsCurrent = false;
}
