using GastroErp.Domain.Common;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Crm;
using GastroErp.Domain.Common.Exceptions;
using GastroErp.Domain.Common.Localization;

namespace GastroErp.Domain.Entities.Crm;

public sealed class Customer : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public string CustomerNumber { get; private set; }
    public string FullName { get; private set; }
    public string Mobile { get; private set; }
    public string? Email { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public string? Gender { get; private set; }
    public string? PreferredLanguage { get; private set; }
    public string? Notes { get; private set; }
    public CustomerStatus Status { get; private set; }

    // Commercial / AR (Back Office Sales)
    public string? TaxNumber { get; private set; }
    public Guid? ArAccountId { get; private set; }
    public string Currency { get; private set; }
    public int PaymentDueDays { get; private set; }
    public string? PaymentTerms { get; private set; }
    public decimal CreditLimit { get; private set; }

    // Statistics
    public int TotalOrders { get; private set; }
    public decimal TotalSpending { get; private set; }
    public decimal AverageTicket { get; private set; }
    public DateTimeOffset? LastVisit { get; private set; }
    public Guid? LastOrderId { get; private set; }
    public Guid? FavoriteBranchId { get; private set; }

    // Navigation
    public LoyaltyAccount? LoyaltyAccount { get; private set; }

    private Customer()
    {
        CustomerNumber = string.Empty;
        FullName = string.Empty;
        Mobile = string.Empty;
        Currency = "SAR";
    }

    public Customer(Guid tenantId, string customerNumber, string fullName, string mobile, string? email = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.");
        if (string.IsNullOrWhiteSpace(fullName)) throw new BusinessException(ErrorCodes.RequiredField, "Full Name is required.");
        if (string.IsNullOrWhiteSpace(mobile)) throw new BusinessException(ErrorCodes.RequiredField, "Mobile is required.");

        TenantId = tenantId;
        CustomerNumber = customerNumber;
        FullName = fullName;
        Mobile = mobile;
        Email = email;
        Currency = "SAR";
        Status = CustomerStatus.Active;

        RaiseDomainEvent(new CustomerCreatedEvent(Id, TenantId));
    }

    public void UpdateInfo(string fullName, string mobile, string? email, DateTime? dateOfBirth, string? gender, string? preferredLanguage, string? notes)
    {
        if (string.IsNullOrWhiteSpace(fullName)) throw new BusinessException(ErrorCodes.RequiredField, "Full Name is required.");
        if (string.IsNullOrWhiteSpace(mobile)) throw new BusinessException(ErrorCodes.RequiredField, "Mobile is required.");

        FullName = fullName;
        Mobile = mobile;
        Email = email;
        DateOfBirth = dateOfBirth;
        Gender = gender;
        PreferredLanguage = preferredLanguage;
        Notes = notes;

        RaiseDomainEvent(new CustomerUpdatedEvent(Id, TenantId));
    }

    /// <summary>شروط البيع الآجل والذمم المدينة للعميل التجاري.</summary>
    public void UpdateCommercialTerms(
        string? taxNumber,
        Guid? arAccountId,
        string? currency,
        int paymentDueDays,
        string? paymentTerms,
        decimal creditLimit)
    {
        if (creditLimit < 0)
            throw new BusinessException(ErrorCodes.InvalidAmount, "Credit limit cannot be negative.");
        if (paymentDueDays < 0)
            throw new BusinessException(ErrorCodes.InvalidAmount, "Payment due days cannot be negative.");

        TaxNumber = string.IsNullOrWhiteSpace(taxNumber) ? null : taxNumber.Trim();
        ArAccountId = arAccountId;
        Currency = string.IsNullOrWhiteSpace(currency) ? "SAR" : currency.Trim().ToUpperInvariant();
        PaymentDueDays = paymentDueDays;
        PaymentTerms = string.IsNullOrWhiteSpace(paymentTerms) ? null : paymentTerms.Trim();
        CreditLimit = creditLimit;

        RaiseDomainEvent(new CustomerUpdatedEvent(Id, TenantId));
    }

    public bool IsOverCreditLimit(decimal currentBalance)
        => CreditLimit > 0 && currentBalance > CreditLimit;

    public void ChangeStatus(CustomerStatus status)
    {
        Status = status;
    }

    public void UpdateStatistics(decimal orderAmount, Guid orderId, Guid branchId, DateTimeOffset visitDate)
    {
        TotalOrders++;
        TotalSpending += orderAmount;
        AverageTicket = TotalOrders > 0 ? TotalSpending / TotalOrders : 0;
        LastVisit = visitDate;
        LastOrderId = orderId;
        // Logic for FavoriteBranchId can be derived periodically or dynamically.
    }
}
