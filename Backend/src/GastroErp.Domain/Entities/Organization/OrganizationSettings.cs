using GastroErp.Domain.Common;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Organization;

/// <summary>
/// OrganizationSettings — إعدادات المؤسسة (Aggregate Root)
/// يمثل إعدادات الشركة مثل الاسم، الرقم الضريبي، العملة الافتراضية، وغيرها.
/// </summary>
public sealed class OrganizationSettings : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public string CompanyName { get; private set; }
    public string? LegalName { get; private set; }
    public string? CommercialRegistration { get; private set; }
    public string? TaxNumber { get; private set; }
    public Guid? DefaultCurrencyId { get; private set; }
    public Guid? DefaultLanguageId { get; private set; }
    public Guid? DefaultTimezoneId { get; private set; }
    public string? DateFormat { get; private set; }
    public string? NumberFormat { get; private set; }
    public string? LogoUrl { get; private set; }
    public string? Theme { get; private set; }
    public string? Address { get; private set; }
    public string? ContactEmail { get; private set; }
    public string? ContactPhone { get; private set; }

    private OrganizationSettings()
    {
        CompanyName = string.Empty;
    }

    public OrganizationSettings(Guid tenantId, string companyName, string? legalName = null, 
        string? commercialRegistration = null, string? taxNumber = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(companyName)) throw new ArgumentException("CompanyName cannot be empty.", nameof(companyName));

        TenantId = tenantId;
        CompanyName = companyName;
        LegalName = legalName;
        CommercialRegistration = commercialRegistration;
        TaxNumber = taxNumber;
    }

    public void UpdateGeneralInfo(string companyName, string? legalName, string? commercialRegistration, string? taxNumber)
    {
        if (string.IsNullOrWhiteSpace(companyName)) throw new ArgumentException("CompanyName cannot be empty.", nameof(companyName));
        
        CompanyName = companyName;
        LegalName = legalName;
        CommercialRegistration = commercialRegistration;
        TaxNumber = taxNumber;
    }

    public void UpdateLocalization(Guid? currencyId, Guid? languageId, Guid? timezoneId, string? dateFormat, string? numberFormat)
    {
        DefaultCurrencyId = currencyId;
        DefaultLanguageId = languageId;
        DefaultTimezoneId = timezoneId;
        DateFormat = dateFormat;
        NumberFormat = numberFormat;
    }

    public void UpdateAppearance(string? logoUrl, string? theme)
    {
        LogoUrl = logoUrl;
        Theme = theme;
    }

    public void UpdateContactInfo(string? address, string? contactEmail, string? contactPhone)
    {
        Address = address;
        ContactEmail = contactEmail;
        ContactPhone = contactPhone;
    }
}

/// <summary>
/// BusinessHour — ساعات العمل (Entity)
/// تمثل ساعات الفتح والإغلاق ليوم محدد من الأسبوع لفرع معين.
/// </summary>
public sealed class BusinessHour : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid BranchId { get; private set; }
    public BusinessDayOfWeek DayOfWeek { get; private set; }
    public TimeOnly OpenTime { get; private set; }
    public TimeOnly CloseTime { get; private set; }
    public bool IsClosed { get; private set; }

    private BusinessHour() { }

    public BusinessHour(Guid tenantId, Guid branchId, BusinessDayOfWeek dayOfWeek,
                        TimeOnly openTime, TimeOnly closeTime)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (branchId == Guid.Empty) throw new ArgumentException("BranchId cannot be empty.", nameof(branchId));
        if (closeTime <= openTime)
            throw new ArgumentException("Close time must be after open time.", nameof(closeTime));

        TenantId = tenantId;
        BranchId = branchId;
        DayOfWeek = dayOfWeek;
        OpenTime = openTime;
        CloseTime = closeTime;
        IsClosed = false;
    }

    public void MarkAsClosed() => IsClosed = true;
    public void UpdateHours(TimeOnly open, TimeOnly close)
    {
        if (close <= open) throw new ArgumentException("Close time must be after open time.", nameof(close));
        OpenTime = open;
        CloseTime = close;
        IsClosed = false;
    }
}

/// <summary>
/// Holiday — العطلة الرسمية (Aggregate Root)
/// تمثل يوم عطلة رسمي على مستوى الشركة.
/// تستخدم في حسابات الرواتب وساعات العمل.
/// </summary>
public sealed class Holiday : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid CompanyId { get; private set; }
    public string Name { get; private set; }
    public string? NameAr { get; private set; }
    public DateOnly Date { get; private set; }
    public bool IsRecurringYearly { get; private set; }

    private Holiday()
    {
        Name = string.Empty;
    }

    public Holiday(Guid tenantId, Guid companyId, string name, DateOnly date,
                   string? nameAr = null, bool isRecurringYearly = false)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (companyId == Guid.Empty) throw new ArgumentException("CompanyId cannot be empty.", nameof(companyId));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Holiday name cannot be empty.", nameof(name));

        TenantId = tenantId;
        CompanyId = companyId;
        Name = name;
        NameAr = nameAr;
        Date = date;
        IsRecurringYearly = isRecurringYearly;
    }

    public void Update(string name, string? nameAr, DateOnly date, bool isRecurring)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty.", nameof(name));
        Name = name;
        NameAr = nameAr;
        Date = date;
        IsRecurringYearly = isRecurring;
    }
}

/// <summary>
/// WorkingShift — وردية العمل (Aggregate Root)
/// تمثل وردية عمل في الفرع (صباحية، مسائية، ليلية).
/// تستخدم في جدولة الموظفين وحسابات الرواتب.
/// </summary>
public sealed class WorkingShift : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid BranchId { get; private set; }
    public string Name { get; private set; }
    public string? NameAr { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public bool IsOvernight { get; private set; }
    public bool IsActive { get; private set; }

    private WorkingShift()
    {
        Name = string.Empty;
    }

    public WorkingShift(Guid tenantId, Guid branchId, string name,
                        TimeOnly startTime, TimeOnly endTime,
                        string? nameAr = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (branchId == Guid.Empty) throw new ArgumentException("BranchId cannot be empty.", nameof(branchId));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Shift name cannot be empty.", nameof(name));

        TenantId = tenantId;
        BranchId = branchId;
        Name = name;
        NameAr = nameAr;
        StartTime = startTime;
        EndTime = endTime;
        IsOvernight = endTime < startTime;
        IsActive = true;
    }

    public void UpdateHours(TimeOnly start, TimeOnly end)
    {
        StartTime = start;
        EndTime = end;
        IsOvernight = end < start;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;

    public TimeSpan Duration =>
        IsOvernight
            ? TimeSpan.FromHours(24) - (EndTime - StartTime)
            : EndTime - StartTime;
}

/// <summary>
/// EmployeePosition — المسمى الوظيفي (Aggregate Root)
/// يمثل المناصب الوظيفية داخل الشركة (مثل: كاشير، شيف، مدير فرع).
/// يستخدم في إدارة الموظفين وحسابات الرواتب.
/// </summary>
public sealed class EmployeePosition : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid CompanyId { get; private set; }
    public Guid? DepartmentId { get; private set; }
    public string Name { get; private set; }
    public string? NameAr { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    private EmployeePosition()
    {
        Name = string.Empty;
    }

    public EmployeePosition(Guid tenantId, Guid companyId, string name,
                             Guid? departmentId = null, string? nameAr = null,
                             string? description = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (companyId == Guid.Empty) throw new ArgumentException("CompanyId cannot be empty.", nameof(companyId));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Position name cannot be empty.", nameof(name));

        TenantId = tenantId;
        CompanyId = companyId;
        DepartmentId = departmentId;
        Name = name;
        NameAr = nameAr;
        Description = description;
        IsActive = true;
    }

    public void UpdateName(string name, string? nameAr)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be empty.", nameof(name));
        Name = name;
        NameAr = nameAr;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
