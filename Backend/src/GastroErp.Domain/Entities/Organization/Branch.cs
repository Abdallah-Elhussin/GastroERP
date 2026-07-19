using GastroErp.Domain.Common;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Organization;
using GastroErp.Domain.ValueObjects;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;

namespace GastroErp.Domain.Entities.Organization;

/// <summary>
/// Branch — الفرع (Aggregate Root)
/// وحدة التشغيل الميدانية. كل فرع مستقل بمستودعاته وأجهزته ومنيوه.
/// يدعم Offline-First بشكل كامل.
/// </summary>
public sealed class Branch : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid CompanyId { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public string? Code { get; private set; }
    public BranchType BranchType { get; private set; }
    public BranchStatus Status { get; private set; }
    public bool IsDefault { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? Email { get; private set; }
    public Address Address { get; private set; }
    public GeoLocation? GeoLocation { get; private set; }
    public Guid? TimezoneId { get; private set; }
    public Guid? CurrencyId { get; private set; }
    public Guid? LanguageId { get; private set; }
    public bool AllowNegativeStock { get; private set; }
    public bool AllowOfflineSales { get; private set; }

    private Branch()
    {
        NameAr = string.Empty;
        Address = Address.Empty;
    }

    public Branch(Guid tenantId, Guid companyId, string nameAr, BranchType branchType,
                  string? nameEn = null, string? code = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (companyId == Guid.Empty) throw new ArgumentException("CompanyId cannot be empty.", nameof(companyId));
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);

        TenantId = tenantId;
        CompanyId = companyId;
        NameAr = nameAr;
        NameEn = nameEn;
        Code = code;
        BranchType = branchType;
        Status = BranchStatus.Active;
        Address = Address.Empty;
        AllowOfflineSales = true;
        AllowNegativeStock = false;

        RaiseDomainEvent(new BranchCreatedEvent(Id, CompanyId, TenantId, NameAr));
    }

    public void UpdateInfo(string nameAr, string? nameEn = null, string? code = null)
    {
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        NameAr = nameAr.Trim();
        NameEn = string.IsNullOrWhiteSpace(nameEn) ? null : nameEn.Trim();
        Code = string.IsNullOrWhiteSpace(code) ? Code : code.Trim();
    }

    public void UpdateContactInfo(string? email, string? phone)
    {
        Email = email;
        PhoneNumber = phone;
    }

    public void UpdateAddress(Address address)
    {
        ArgumentNullException.ThrowIfNull(address);
        Address = address;
        RaiseDomainEvent(new BranchAddressChangedEvent(Id, TenantId));
    }

    public void SetGeoLocation(decimal latitude, decimal longitude)
    {
        GeoLocation = new GeoLocation(latitude, longitude);
    }

    public void Deactivate()
    {
        if (Status == BranchStatus.Active)
        {
            Status = BranchStatus.Inactive;
            RaiseDomainEvent(new BranchDeactivatedEvent(Id, TenantId));
        }
    }

    public void SetUnderMaintenance() => Status = BranchStatus.UnderMaintenance;
    public void Activate() => Status = BranchStatus.Active;

    public void ConfigureSettings(bool allowNegativeStock, bool allowOfflineSales)
    {
        AllowNegativeStock = allowNegativeStock;
        AllowOfflineSales = allowOfflineSales;
    }

    public void SetLocalization(Guid? timezoneId, Guid? currencyId, Guid? languageId)
    {
        TimezoneId = timezoneId;
        CurrencyId = currencyId;
        LanguageId = languageId;
    }
    
    public void SetAsDefault()
    {
        IsDefault = true;
    }
    
    public void RemoveDefault()
    {
        IsDefault = false;
    }

    public void Archive()
    {
        if (Status != BranchStatus.Archived)
        {
            Status = BranchStatus.Archived;
            RaiseDomainEvent(new BranchArchivedEvent(Id, TenantId));
        }
    }

    public void RestoreFromArchive()
    {
        if (Status == BranchStatus.Archived)
        {
            Status = BranchStatus.Active;
            RaiseDomainEvent(new BranchRestoredEvent(Id, TenantId));
        }
    }

    public bool IsActive => Status == BranchStatus.Active;
}
