using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Inventory.Suppliers;

/// <summary>
/// المورد — Master Data لدورة المشتريات والذمم الدائنة.
/// </summary>
public sealed class Supplier : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid? CompanyId { get; private set; }
    public Guid? BranchId { get; private set; }

    public string Code { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public SupplierType SupplierType { get; private set; }
    public SupplierCategory Category { get; private set; }

    // ── Tax ──────────────────────────────────────────────────────────────────
    public string? TaxNumber { get; private set; }
    public string? CommercialRegister { get; private set; }
    public string? EstablishmentNumber { get; private set; }
    public string? TaxRegistrationCountry { get; private set; }
    public string? TaxType { get; private set; }
    public decimal DefaultTaxPercent { get; private set; }
    public DateOnly? TaxCertificateExpiry { get; private set; }
    public DateOnly? CommercialRegisterExpiry { get; private set; }
    public string? TaxCertificatePath { get; private set; }
    public string? CommercialRegisterPath { get; private set; }

    // ── Contact / address ────────────────────────────────────────────────────
    public string? ContactPerson { get; private set; }
    public string? ContactJobTitle { get; private set; }
    public string? Phone { get; private set; }
    public string? Mobile { get; private set; }
    public string? Email { get; private set; }
    public string? Website { get; private set; }
    public string? City { get; private set; }
    public string? Region { get; private set; }
    public string? Country { get; private set; }
    public string? PostalCode { get; private set; }
    public string? Address { get; private set; }

    // ── Financial ────────────────────────────────────────────────────────────
    /// <summary>حساب ذمة المورد في دليل الحسابات (إجباري للترحيل).</summary>
    public Guid? ApAccountId { get; private set; }
    public Guid? DiscountAccountId { get; private set; }
    public Guid? PurchaseReturnAccountId { get; private set; }
    public Guid? ExchangeDifferenceAccountId { get; private set; }
    public string Currency { get; private set; }
    public SupplierPaymentMethodKind DefaultPaymentMethod { get; private set; }
    public int PaymentDueDays { get; private set; }
    public string? PaymentTerms { get; private set; }
    public decimal CreditLimit { get; private set; }
    public decimal OpeningBalance { get; private set; }
    public DateOnly? OpeningBalanceDate { get; private set; }
    public SupplierVatEvaluation VatEvaluation { get; private set; }

    public int LeadTimeDays { get; private set; }
    public bool IsPreferred { get; private set; }
    public int Rating { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsBlacklisted { get; private set; }
    public string? BlacklistReason { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<SupplierContact> _contacts = [];
    public IReadOnlyCollection<SupplierContact> Contacts => _contacts.AsReadOnly();

    private readonly List<SupplierPaymentMethod> _paymentMethods = [];
    public IReadOnlyCollection<SupplierPaymentMethod> PaymentMethods => _paymentMethods.AsReadOnly();

    private readonly List<SupplierAttachment> _attachments = [];
    public IReadOnlyCollection<SupplierAttachment> Attachments => _attachments.AsReadOnly();

    private Supplier()
    {
        Code = string.Empty;
        NameAr = string.Empty;
        Currency = "SAR";
        SupplierType = SupplierType.Local;
        Category = SupplierCategory.Other;
        DefaultPaymentMethod = SupplierPaymentMethodKind.Credit;
        VatEvaluation = SupplierVatEvaluation.ExcludeVat;
    }

    public static Supplier Create(
        Guid tenantId,
        string code,
        string nameAr,
        string? nameEn = null,
        string currency = "SAR",
        Guid? companyId = null,
        Guid? branchId = null,
        SupplierType supplierType = SupplierType.Local,
        SupplierCategory category = SupplierCategory.Other,
        Guid? apAccountId = null,
        SupplierPaymentMethodKind defaultPaymentMethod = SupplierPaymentMethodKind.Credit)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException(nameof(tenantId));
        if (string.IsNullOrWhiteSpace(code)) throw new BusinessException(ErrorCodes.RequiredField, "Supplier code is required.");
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length != 3)
            throw new BusinessException(ErrorCodes.RequiredField, "Currency is required.");

        return new Supplier
        {
            TenantId = tenantId,
            CompanyId = companyId,
            BranchId = branchId,
            Code = code.Trim().ToUpperInvariant(),
            NameAr = nameAr.Trim(),
            NameEn = string.IsNullOrWhiteSpace(nameEn) ? null : nameEn.Trim(),
            Currency = currency.Trim().ToUpperInvariant(),
            SupplierType = supplierType,
            Category = category,
            ApAccountId = apAccountId == Guid.Empty ? null : apAccountId,
            DefaultPaymentMethod = defaultPaymentMethod,
            IsActive = true,
            IsBlacklisted = false
        };
    }

    public void UpdateBasic(
        string nameAr,
        string? nameEn,
        SupplierType supplierType,
        SupplierCategory category,
        Guid? companyId,
        Guid? branchId)
    {
        EnsureNotBlacklistedForEdit();
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        NameAr = nameAr.Trim();
        NameEn = string.IsNullOrWhiteSpace(nameEn) ? null : nameEn.Trim();
        SupplierType = supplierType;
        Category = category;
        CompanyId = companyId;
        BranchId = branchId;
    }

    public void UpdateTax(
        string? taxNumber,
        string? commercialRegister,
        string? establishmentNumber,
        string? taxRegistrationCountry,
        string? taxType,
        decimal defaultTaxPercent,
        DateOnly? taxCertificateExpiry,
        DateOnly? commercialRegisterExpiry,
        string? taxCertificatePath = null,
        string? commercialRegisterPath = null)
    {
        if (defaultTaxPercent is < 0 or > 100)
            throw new BusinessException(ErrorCodes.InvalidAmount, "Default tax percent is invalid.");
        TaxNumber = NullIfWhiteSpace(taxNumber);
        CommercialRegister = NullIfWhiteSpace(commercialRegister);
        EstablishmentNumber = NullIfWhiteSpace(establishmentNumber);
        TaxRegistrationCountry = NullIfWhiteSpace(taxRegistrationCountry);
        TaxType = NullIfWhiteSpace(taxType);
        DefaultTaxPercent = defaultTaxPercent;
        TaxCertificateExpiry = taxCertificateExpiry;
        CommercialRegisterExpiry = commercialRegisterExpiry;
        if (taxCertificatePath is not null) TaxCertificatePath = NullIfWhiteSpace(taxCertificatePath);
        if (commercialRegisterPath is not null) CommercialRegisterPath = NullIfWhiteSpace(commercialRegisterPath);
    }

    public void UpdateContact(
        string? contactPerson,
        string? contactJobTitle,
        string? phone,
        string? mobile,
        string? email,
        string? website,
        string? city,
        string? region,
        string? country,
        string? postalCode,
        string? address)
    {
        ContactPerson = NullIfWhiteSpace(contactPerson);
        ContactJobTitle = NullIfWhiteSpace(contactJobTitle);
        Phone = NullIfWhiteSpace(phone);
        Mobile = NullIfWhiteSpace(mobile);
        Email = NullIfWhiteSpace(email);
        Website = NullIfWhiteSpace(website);
        City = NullIfWhiteSpace(city);
        Region = NullIfWhiteSpace(region);
        Country = NullIfWhiteSpace(country);
        PostalCode = NullIfWhiteSpace(postalCode);
        Address = NullIfWhiteSpace(address);
    }

    public void UpdateFinancial(
        Guid? apAccountId,
        Guid? discountAccountId,
        Guid? purchaseReturnAccountId,
        Guid? exchangeDifferenceAccountId,
        string currency,
        SupplierPaymentMethodKind defaultPaymentMethod,
        int paymentDueDays,
        string? paymentTerms,
        decimal creditLimit,
        decimal openingBalance,
        DateOnly? openingBalanceDate,
        SupplierVatEvaluation vatEvaluation,
        int leadTimeDays)
    {
        if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length != 3)
            throw new BusinessException(ErrorCodes.RequiredField, "Currency is required.");
        if (creditLimit < 0) throw new BusinessException(ErrorCodes.InvalidAmount, "Credit limit cannot be negative.");
        if (paymentDueDays < 0) throw new BusinessException(ErrorCodes.InvalidAmount, "Credit days cannot be negative.");
        if (leadTimeDays < 0) throw new BusinessException(ErrorCodes.InvalidAmount, "Lead time cannot be negative.");
        if (apAccountId is null || apAccountId == Guid.Empty)
            throw new BusinessException(ErrorCodes.RequiredField, "Supplier AP account is required.");

        ApAccountId = apAccountId;
        DiscountAccountId = EmptyToNull(discountAccountId);
        PurchaseReturnAccountId = EmptyToNull(purchaseReturnAccountId);
        ExchangeDifferenceAccountId = EmptyToNull(exchangeDifferenceAccountId);
        Currency = currency.Trim().ToUpperInvariant();
        DefaultPaymentMethod = defaultPaymentMethod;
        PaymentDueDays = paymentDueDays;
        PaymentTerms = NullIfWhiteSpace(paymentTerms);
        CreditLimit = creditLimit;
        OpeningBalance = openingBalance;
        OpeningBalanceDate = openingBalanceDate;
        VatEvaluation = vatEvaluation;
        LeadTimeDays = leadTimeDays;
    }

    public void UpdateNotes(string? notes) => Notes = NullIfWhiteSpace(notes);

    public void SetRating(int rating)
    {
        if (rating is < 0 or > 5) throw new ArgumentException("Rating must be between 0 and 5.", nameof(rating));
        Rating = rating;
    }

    public void SetPreferred(bool isPreferred) => IsPreferred = isPreferred;

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Blacklist(string? reason)
    {
        IsBlacklisted = true;
        IsActive = false;
        BlacklistReason = NullIfWhiteSpace(reason);
    }

    public void ClearBlacklist()
    {
        IsBlacklisted = false;
        BlacklistReason = null;
        IsActive = true;
    }

    public void EnsureCanPurchase()
    {
        if (IsBlacklisted)
            throw new BusinessException("SupplierBlacklisted", "Supplier is blacklisted and cannot be used in purchasing.");
        if (!IsActive)
            throw new BusinessException("SupplierInactive", "Supplier is inactive.");
    }

    public bool IsOverCreditLimit(decimal currentBalance)
        => CreditLimit > 0 && currentBalance > CreditLimit;

    public bool IsTaxCertificateExpired(DateOnly today)
        => TaxCertificateExpiry.HasValue && TaxCertificateExpiry.Value < today;

    public bool IsCommercialRegisterExpired(DateOnly today)
        => CommercialRegisterExpiry.HasValue && CommercialRegisterExpiry.Value < today;

    public void AddContact(string nameAr, string phoneNumber, string? email = null, string? position = null, string? nameEn = null, string? mobile = null)
    {
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        if (string.IsNullOrWhiteSpace(phoneNumber) && string.IsNullOrWhiteSpace(mobile))
            throw new BusinessException(ErrorCodes.RequiredField, "Phone or mobile is required.");
        _contacts.Add(new SupplierContact(TenantId, Id, nameAr, phoneNumber ?? mobile ?? "", email, position, nameEn, mobile));
    }

    public void RemoveContact(Guid contactId)
    {
        var contact = _contacts.FirstOrDefault(c => c.Id == contactId)
            ?? throw new BusinessException(ErrorCodes.ItemNotFound);
        _contacts.Remove(contact);
    }

    public void ReplacePaymentMethods(IEnumerable<SupplierPaymentMethodDraft> drafts)
    {
        _paymentMethods.Clear();
        var list = drafts.ToList();
        var defaultCount = list.Count(d => d.IsDefault);
        if (defaultCount > 1)
            throw new BusinessException(ErrorCodes.InvalidAmount, "Only one default payment method is allowed.");

        foreach (var d in list)
        {
            _paymentMethods.Add(new SupplierPaymentMethod(
                TenantId, Id, d.Kind, d.BankName, d.Iban, d.Swift, d.AccountNumber,
                d.BeneficiaryName, d.Currency, d.IsDefault, d.Notes));
        }

        if (_paymentMethods.Count > 0 && !_paymentMethods.Any(p => p.IsDefault))
            _paymentMethods[0].SetDefault(true);
    }

    public void SetDefaultPaymentMethod(Guid paymentMethodId)
    {
        var target = _paymentMethods.FirstOrDefault(p => p.Id == paymentMethodId)
            ?? throw new BusinessException(ErrorCodes.ItemNotFound);
        foreach (var pm in _paymentMethods)
            pm.SetDefault(pm.Id == target.Id);
    }

    public void RemovePaymentMethod(Guid paymentMethodId)
    {
        var pm = _paymentMethods.FirstOrDefault(p => p.Id == paymentMethodId)
            ?? throw new BusinessException(ErrorCodes.ItemNotFound);
        _paymentMethods.Remove(pm);
        if (_paymentMethods.Count > 0 && !_paymentMethods.Any(p => p.IsDefault))
            _paymentMethods[0].SetDefault(true);
    }

    public void AddAttachment(string fileName, string contentType, string storagePath, long sizeBytes, string? category = null)
    {
        if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(storagePath))
            throw new BusinessException(ErrorCodes.RequiredField);
        _attachments.Add(new SupplierAttachment(TenantId, Id, fileName, contentType, storagePath, sizeBytes, category));
    }

    public void RemoveAttachment(Guid attachmentId)
    {
        var att = _attachments.FirstOrDefault(a => a.Id == attachmentId)
            ?? throw new BusinessException(ErrorCodes.ItemNotFound);
        _attachments.Remove(att);
    }

    // Legacy helpers used by older handlers
    public void UpdateInfo(string nameAr, string? nameEn, string currency)
    {
        UpdateBasic(nameAr, nameEn, SupplierType, Category, CompanyId, BranchId);
        if (string.IsNullOrWhiteSpace(currency) || currency.Trim().Length != 3)
            throw new BusinessException(ErrorCodes.RequiredField, "Currency is required.");
        Currency = currency.Trim().ToUpperInvariant();
    }

    public void UpdateFinancialInfo(
        string? taxNumber,
        string? paymentTerms,
        decimal creditLimit,
        int leadTimeDays,
        Guid? apAccountId = null,
        int paymentDueDays = 0)
    {
        TaxNumber = NullIfWhiteSpace(taxNumber);
        PaymentTerms = NullIfWhiteSpace(paymentTerms);
        if (creditLimit < 0) throw new BusinessException(ErrorCodes.InvalidAmount);
        CreditLimit = creditLimit;
        LeadTimeDays = Math.Max(0, leadTimeDays);
        if (apAccountId.HasValue) ApAccountId = EmptyToNull(apAccountId);
        PaymentDueDays = Math.Max(0, paymentDueDays);
    }

    private void EnsureNotBlacklistedForEdit()
    {
        // Editing basic data allowed even when blacklisted (to clear / fix), no-op.
    }

    private static string? NullIfWhiteSpace(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static Guid? EmptyToNull(Guid? id)
        => id is null || id == Guid.Empty ? null : id;
}

public sealed record SupplierPaymentMethodDraft(
    SupplierPaymentMethodKind Kind,
    string? BankName,
    string? Iban,
    string? Swift,
    string? AccountNumber,
    string? BeneficiaryName,
    string Currency,
    bool IsDefault,
    string? Notes);

public sealed class SupplierContact : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid SupplierId { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public string PhoneNumber { get; private set; }
    public string? Mobile { get; private set; }
    public string? Email { get; private set; }
    public string? Position { get; private set; }

    private SupplierContact()
    {
        NameAr = string.Empty;
        PhoneNumber = string.Empty;
    }

    internal SupplierContact(
        Guid tenantId, Guid supplierId, string nameAr, string phoneNumber,
        string? email, string? position, string? nameEn = null, string? mobile = null)
    {
        TenantId = tenantId;
        SupplierId = supplierId;
        NameAr = nameAr.Trim();
        NameEn = string.IsNullOrWhiteSpace(nameEn) ? null : nameEn.Trim();
        PhoneNumber = phoneNumber.Trim();
        Mobile = string.IsNullOrWhiteSpace(mobile) ? null : mobile.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        Position = string.IsNullOrWhiteSpace(position) ? null : position.Trim();
    }
}

public sealed class SupplierPaymentMethod : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid SupplierId { get; private set; }
    public SupplierPaymentMethodKind Kind { get; private set; }
    public string? BankName { get; private set; }
    public string? Iban { get; private set; }
    public string? Swift { get; private set; }
    public string? AccountNumber { get; private set; }
    public string? BeneficiaryName { get; private set; }
    public string Currency { get; private set; }
    public bool IsDefault { get; private set; }
    public string? Notes { get; private set; }

    private SupplierPaymentMethod()
    {
        Currency = "SAR";
    }

    internal SupplierPaymentMethod(
        Guid tenantId, Guid supplierId, SupplierPaymentMethodKind kind,
        string? bankName, string? iban, string? swift, string? accountNumber,
        string? beneficiaryName, string currency, bool isDefault, string? notes)
    {
        TenantId = tenantId;
        SupplierId = supplierId;
        Kind = kind;
        BankName = string.IsNullOrWhiteSpace(bankName) ? null : bankName.Trim();
        Iban = string.IsNullOrWhiteSpace(iban) ? null : iban.Trim();
        Swift = string.IsNullOrWhiteSpace(swift) ? null : swift.Trim();
        AccountNumber = string.IsNullOrWhiteSpace(accountNumber) ? null : accountNumber.Trim();
        BeneficiaryName = string.IsNullOrWhiteSpace(beneficiaryName) ? null : beneficiaryName.Trim();
        Currency = string.IsNullOrWhiteSpace(currency) ? "SAR" : currency.Trim().ToUpperInvariant();
        IsDefault = isDefault;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    internal void SetDefault(bool isDefault) => IsDefault = isDefault;
}

public sealed class SupplierAttachment : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid SupplierId { get; private set; }
    public string FileName { get; private set; }
    public string ContentType { get; private set; }
    public string StoragePath { get; private set; }
    public long SizeBytes { get; private set; }
    public string? Category { get; private set; }

    private SupplierAttachment()
    {
        FileName = string.Empty;
        ContentType = string.Empty;
        StoragePath = string.Empty;
    }

    internal SupplierAttachment(
        Guid tenantId, Guid supplierId, string fileName, string contentType,
        string storagePath, long sizeBytes, string? category)
    {
        TenantId = tenantId;
        SupplierId = supplierId;
        FileName = fileName.Trim();
        ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType.Trim();
        StoragePath = storagePath.Trim();
        SizeBytes = Math.Max(0, sizeBytes);
        Category = string.IsNullOrWhiteSpace(category) ? null : category.Trim();
    }
}
