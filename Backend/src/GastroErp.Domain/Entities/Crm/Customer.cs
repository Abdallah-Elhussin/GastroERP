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
