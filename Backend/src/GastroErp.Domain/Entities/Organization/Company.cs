using GastroErp.Domain.Common;
using GastroErp.Domain.Events.Organization;
using GastroErp.Domain.ValueObjects;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;

namespace GastroErp.Domain.Entities.Organization;

/// <summary>
/// Company — الشركة (Aggregate Root)
/// <para>
/// الكيان القانوني التابع للمستأجر. مستأجر واحد يمكنه امتلاك عدة شركات
/// (مثلاً: شركة مطاعم + شركة توصيل).
/// The legal entity belonging to a tenant. One tenant can own multiple companies.
/// </para>
/// <para>
/// قواعد العمل / Business Rules:
/// - الرقم الضريبي (TaxNumber) فريد داخل نفس المستأجر.
/// - رقم ضريبة القيمة المضافة (VatNumber) يتبع معيار ZATCA (15 رقماً، يبدأ وينتهي بـ 3).
/// - لا يمكن حذف شركة لديها فروع نشطة أو سجلات مالية.
/// - الحذف منطقي فقط (Soft Delete).
/// </para>
/// <remarks>
/// TODO (EF Core Configuration): أضف Optimistic Concurrency في IEntityTypeConfiguration لهذا الكيان:
/// <code>builder.Property&lt;byte[]&gt;("RowVersion").IsRowVersion();</code>
/// لا يُعرَّف RowVersion داخل الـ Domain Entity بل في طبقة Infrastructure فقط.
/// </remarks>
/// </summary>
public sealed class Company : AuditableBaseEntity
{
    // ─── Properties ────────────────────────────────────────────────────────────

    /// <summary>معرّف المستأجر المالك / Owning tenant identifier.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>الاسم الرسمي للشركة بالعربي / Official company name in Arabic.</summary>
    public string NameAr { get; private set; }

    /// <summary>الاسم الرسمي للشركة بالإنجليزي / Official company name in English (optional).</summary>
    public string? NameEn { get; private set; }
    public string TaxNumber { get; private set; }

    /// <summary>
    /// رقم ضريبة القيمة المضافة (ZATCA) / ZATCA VAT registration number.
    /// Value Object: 15 رقماً، يبدأ وينتهي بـ '3'.
    /// </summary>
    public VatNumber? VatNumber { get; private set; }

    /// <summary>رقم السجل التجاري / Commercial registration number.</summary>
    public string? CommercialRegister { get; private set; }

    /// <summary>الموقع الإلكتروني / Company website URL.</summary>
    public string? Website { get; private set; }

    /// <summary>رابط الشعار / Logo image URL.</summary>
    public string? LogoUrl { get; private set; }

    /// <summary>عنوان الشركة / Company address (Value Object).</summary>
    public Address Address { get; private set; }

    /// <summary>
    /// البريد الإلكتروني الرسمي / Official email address (Value Object).
    /// يتم التحقق من الصيغة عبر EmailAddress.Create().
    /// </summary>
    public EmailAddress? Email { get; private set; }

    /// <summary>
    /// رقم الهاتف الرسمي / Official phone number (Value Object).
    /// يجب أن يكون بصيغة E.164 (مثال: +966501234567).
    /// </summary>
    public PhoneNumber? PhoneNumber { get; private set; }

    /// <summary>معرّف العملة الافتراضية / Default currency identifier.</summary>
    public Guid? DefaultCurrencyId { get; private set; }

    /// <summary>الشهر الأول من السنة المالية (1–12) / Fiscal year start month (1–12).</summary>
    public byte FiscalYearStartMonth { get; private set; }

    /// <summary>حالة نشاط الشركة / Company active status.</summary>
    public bool IsActive { get; private set; }

    // ─── EF Core Private Parameterless Constructor ──────────────────────────────

    /// <summary>
    /// المُنشئ الخاص — مخصص لـ EF Core فقط ولا يُستخدم في كود التطبيق.
    /// Private parameterless constructor — reserved for EF Core materialization only.
    /// </summary>
    private Company()
    {
        NameAr = string.Empty;
        TaxNumber = string.Empty;
        Address = Address.Empty;
    }

    // ─── Public Constructor ─────────────────────────────────────────────────────

    /// <summary>
    /// أنشئ كيان شركة جديدة.
    /// Creates a new Company aggregate root.
    /// </summary>
    /// <param name="tenantId">معرّف المستأجر / Tenant identifier.</param>
    /// <param name="name">الاسم الرسمي / Official name.</param>
    /// <param name="taxNumber">الرقم الضريبي الوطني / National tax number.</param>
    /// <param name="nameAr">الاسم بالعربي (اختياري) / Arabic name (optional).</param>
    /// <param name="vatNumber">
    /// رقم ضريبة القيمة المضافة كـ Value Object (اختياري) / VAT number Value Object (optional).
    /// استخدم VatNumber.Create() للإنشاء مع التحقق.
    /// </param>
    /// <param name="commercialRegister">رقم السجل التجاري (اختياري) / Commercial register (optional).</param>
    /// <exception cref="ArgumentException">يُرفع عند فراغ أي حقل إلزامي / Thrown when any required field is empty.</exception>
    public Company(
        Guid tenantId,
        string nameAr,
        string taxNumber,
        string? nameEn = null,
        VatNumber? vatNumber = null,
        string? commercialRegister = null)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("معرّف المستأجر لا يمكن أن يكون فارغاً. / TenantId cannot be empty.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(nameAr))
            throw new BusinessException(ErrorCodes.NameArRequired);
        if (string.IsNullOrWhiteSpace(taxNumber))
            throw new ArgumentException("الرقم الضريبي لا يمكن أن يكون فارغاً. / Tax number cannot be empty.", nameof(taxNumber));

        TenantId = tenantId;
        NameAr = nameAr;
        NameEn = nameEn;
        TaxNumber = taxNumber;
        VatNumber = vatNumber;
        CommercialRegister = commercialRegister;
        Address = Address.Empty;
        FiscalYearStartMonth = 1;
        IsActive = true;

        RaiseDomainEvent(new CompanyCreatedEvent(Id, TenantId, NameAr));
    }

    // ─── Domain Methods ─────────────────────────────────────────────────────────

    /// <summary>
    /// حدّث معلومات التواصل (البريد الإلكتروني، الهاتف، الموقع الإلكتروني).
    /// Updates the company's contact information.
    /// </summary>
    /// <param name="email">
    /// البريد الإلكتروني كـ Value Object / Email as Value Object.
    /// استخدم EmailAddress.TryCreate() للإنشاء الآمن.
    /// </param>
    /// <param name="phone">
    /// رقم الهاتف كـ Value Object / Phone as Value Object.
    /// استخدم PhoneNumber.TryCreate() للإنشاء الآمن.
    /// </param>
    /// <param name="website">الموقع الإلكتروني / Website URL (nullable).</param>
    public void UpdateContactInfo(EmailAddress? email, PhoneNumber? phone, string? website)
    {
        Email = email;
        PhoneNumber = phone;
        Website = website;
    }

    /// <summary>
    /// حدّث عنوان الشركة.
    /// Updates the company's physical address.
    /// </summary>
    /// <param name="address">كائن القيمة للعنوان / Address value object.</param>
    /// <exception cref="ArgumentNullException">يُرفع عند إدخال null / Thrown when null is passed.</exception>
    public void UpdateAddress(Address address)
    {
        ArgumentNullException.ThrowIfNull(address);
        Address = address;
    }

    /// <summary>
    /// عيّن بداية السنة المالية.
    /// Sets the fiscal year start month.
    /// </summary>
    /// <param name="startMonth">رقم الشهر من 1 إلى 12 / Month number from 1 to 12.</param>
    /// <exception cref="ArgumentOutOfRangeException">يُرفع عند تجاوز النطاق / Thrown when value is out of range.</exception>
    public void SetFiscalYear(byte startMonth)
    {
        if (startMonth < 1 || startMonth > 12)
            throw new ArgumentOutOfRangeException(
                nameof(startMonth),
                "شهر بداية السنة المالية يجب أن يكون بين 1 و 12. / Fiscal year start month must be between 1 and 12.");

        FiscalYearStartMonth = startMonth;
    }

    /// <summary>
    /// عيّن العملة الافتراضية للشركة.
    /// Sets the company's default currency.
    /// </summary>
    /// <param name="currencyId">معرّف العملة / Currency entity identifier.</param>
    /// <exception cref="ArgumentException">يُرفع عند إدخال Guid فارغ / Thrown when Guid is empty.</exception>
    public void SetDefaultCurrency(Guid currencyId)
    {
        if (currencyId == Guid.Empty)
            throw new ArgumentException(
                "معرّف العملة لا يمكن أن يكون فارغاً. / Currency identifier cannot be empty.",
                nameof(currencyId));

        DefaultCurrencyId = currencyId;
    }

    /// <summary>
    /// حدّث البيانات القانونية للشركة (الاسم، الرقم الضريبي للقيمة المضافة، السجل التجاري).
    /// Updates the company's legal information.
    /// يُطلق CompanyLegalInfoUpdatedEvent لإعلام الأنظمة الأخرى بالتغيير.
    /// </summary>
    /// <param name="name">الاسم الجديد بالإنجليزي / New name in English.</param>
    /// <param name="nameAr">الاسم الجديد بالعربي / New name in Arabic (nullable).</param>
    /// <param name="vatNumber">
    /// رقم الضريبة الجديد كـ Value Object / New VAT number as Value Object (nullable).
    /// استخدم VatNumber.TryCreate() للإنشاء الآمن.
    /// </param>
    /// <param name="commercialRegister">رقم السجل التجاري / Commercial register number (nullable).</param>
    /// <exception cref="ArgumentException">يُرفع عند فراغ الاسم / Thrown when name is empty.</exception>
    public void UpdateLegalInfo(string nameAr, string? nameEn, VatNumber? vatNumber, string? commercialRegister)
    {
        if (string.IsNullOrWhiteSpace(nameAr))
            throw new BusinessException(ErrorCodes.NameArRequired);

        NameAr = nameAr;
        NameEn = nameEn;
        VatNumber = vatNumber;
        CommercialRegister = commercialRegister;

        RaiseDomainEvent(new CompanyLegalInfoUpdatedEvent(Id, TenantId, NameAr));
    }

    /// <summary>
    /// أوقف الشركة (Idempotent).
    /// Deactivates the company. Idempotent — no effect if already inactive.
    /// يُطلق CompanyDeactivatedEvent عند تغيير الحالة فعلياً فقط.
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive) return;

        IsActive = false;
        RaiseDomainEvent(new CompanyDeactivatedEvent(Id, TenantId));
    }

    /// <summary>
    /// فعّل الشركة (Idempotent).
    /// Activates the company. Idempotent — no effect if already active.
    /// يُطلق CompanyActivatedEvent عند تغيير الحالة فعلياً فقط.
    /// </summary>
    public void Activate()
    {
        if (IsActive) return;

        IsActive = true;
        RaiseDomainEvent(new CompanyActivatedEvent(Id, TenantId));
    }

    // ─── Computed Properties ────────────────────────────────────────────────────

    /// <summary>
    /// هل الشركة لديها رقم ضريبة قيمة مضافة مسجّل؟
    /// Does the company have a registered VAT number?
    /// </summary>
    public bool IsVatRegistered => VatNumber is not null;
}
