using GastroErp.Domain.Common;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;

namespace GastroErp.Domain.Entities.Inventory.Suppliers;

/// <summary>
/// المورد (Aggregate Root)
/// </summary>
public sealed class Supplier : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public string? TaxNumber { get; private set; }
    
    /// <summary>طريقة الدفع (مثلاً: Net 30, Cash, On Delivery)</summary>
    public string? PaymentTerms { get; private set; }

    /// <summary>الحد الائتماني للمورد</summary>
    public decimal CreditLimit { get; private set; }

    /// <summary>عملة التعامل مع المورد (افتراضي: SAR)</summary>
    public string Currency { get; private set; }

    /// <summary>الوقت المتوقع للتوريد بالأيام</summary>
    public int LeadTimeDays { get; private set; }

    /// <summary>هل هذا المورد هو المفضل للتعامل؟</summary>
    public bool IsPreferred { get; private set; }

    /// <summary>تقييم المورد (من 1 إلى 5)</summary>
    public int Rating { get; private set; }

    public bool IsActive { get; private set; }

    private readonly List<SupplierContact> _contacts = [];
    public IReadOnlyCollection<SupplierContact> Contacts => _contacts.AsReadOnly();

    private Supplier()
    {
        NameAr = string.Empty;
        Currency = "SAR";
    }

    public Supplier(Guid tenantId, string nameAr, string? nameEn = null, string currency = "SAR")
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);

        TenantId = tenantId;
        NameAr = nameAr;
        NameEn = nameEn;
        Currency = currency.ToUpperInvariant();
        CreditLimit = 0;
        LeadTimeDays = 0;
        Rating = 0;
        IsPreferred = false;
        IsActive = true;
    }

    public void UpdateInfo(string nameAr, string? nameEn, string currency)
    {
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        NameAr = nameAr;
        NameEn = nameEn;
        Currency = currency.ToUpperInvariant();
    }

    public void UpdateFinancialInfo(string? taxNumber, string? paymentTerms, decimal creditLimit, int leadTimeDays)
    {
        if (creditLimit < 0) throw new ArgumentException("CreditLimit cannot be negative.", nameof(creditLimit));
        if (leadTimeDays < 0) throw new ArgumentException("LeadTimeDays cannot be negative.", nameof(leadTimeDays));

        TaxNumber = taxNumber;
        PaymentTerms = paymentTerms;
        CreditLimit = creditLimit;
        LeadTimeDays = leadTimeDays;
    }

    public void SetRating(int rating)
    {
        if (rating < 0 || rating > 5) throw new ArgumentException("Rating must be between 0 and 5.", nameof(rating));
        Rating = rating;
    }

    public void SetPreferred(bool isPreferred) => IsPreferred = isPreferred;

    public void AddContact(string nameAr, string phoneNumber, string? email = null, string? position = null, string? nameEn = null)
    {
        if (string.IsNullOrWhiteSpace(nameAr)) throw new BusinessException(ErrorCodes.NameArRequired);
        if (string.IsNullOrWhiteSpace(phoneNumber)) throw new ArgumentException("Phone number cannot be empty.", nameof(phoneNumber));

        _contacts.Add(new SupplierContact(TenantId, Id, nameAr, phoneNumber, email, position, nameEn));
    }

    public void RemoveContact(Guid contactId)
    {
        var contact = _contacts.FirstOrDefault(c => c.Id == contactId)
            ?? throw new BusinessException(ErrorCodes.ItemNotFound);
        _contacts.Remove(contact);
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}

// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// جهة اتصال المورد
/// </summary>
public sealed class SupplierContact : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid SupplierId { get; private set; }
    public string NameAr { get; private set; }
    public string? NameEn { get; private set; }
    public string PhoneNumber { get; private set; }
    public string? Email { get; private set; }
    public string? Position { get; private set; }

    private SupplierContact()
    {
        NameAr = string.Empty;
        PhoneNumber = string.Empty;
    }

    internal SupplierContact(Guid tenantId, Guid supplierId, string nameAr, string phoneNumber, string? email, string? position, string? nameEn = null)
    {
        TenantId = tenantId;
        SupplierId = supplierId;
        NameAr = nameAr;
        NameEn = nameEn;
        PhoneNumber = phoneNumber;
        Email = email;
        Position = position;
    }
}
