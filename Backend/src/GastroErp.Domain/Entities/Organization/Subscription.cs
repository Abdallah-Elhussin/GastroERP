using GastroErp.Domain.Common;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Organization;
using GastroErp.Domain.ValueObjects;

namespace GastroErp.Domain.Entities.Organization;

/// <summary>
/// SubscriptionPlan — باقة الاشتراك (Aggregate Root)
/// تعرّف الخطط المتاحة للمستأجرين مع حدودها وأسعارها.
/// </summary>
public sealed class SubscriptionPlan : AuditableBaseEntity
{
    public string Name { get; private set; }
    public string NameAr { get; private set; }
    public string? Description { get; private set; }
    public SubscriptionPlanType PlanType { get; private set; }
    public Money MonthlyPrice { get; private set; }
    public Money YearlyPrice { get; private set; }
    public int MaxBranches { get; private set; }
    public int MaxUsers { get; private set; }
    public int MaxDevices { get; private set; }
    public int MaxProducts { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsSystem { get; private set; }
    public int SortOrder { get; private set; }

    private SubscriptionPlan()
    {
        Name = string.Empty;
        NameAr = string.Empty;
        MonthlyPrice = Money.Zero();
        YearlyPrice = Money.Zero();
    }

    public SubscriptionPlan(
        string name, string nameAr, SubscriptionPlanType planType,
        Money monthlyPrice, Money yearlyPrice,
        int maxBranches, int maxUsers, int maxDevices,
        int maxProducts = -1, string? description = null, int sortOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Plan name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(nameAr)) throw new ArgumentException("Arabic name cannot be empty.", nameof(nameAr));
        if (maxBranches < -1) throw new ArgumentException("MaxBranches cannot be less than -1.", nameof(maxBranches));
        if (maxUsers < -1) throw new ArgumentException("MaxUsers cannot be less than -1.", nameof(maxUsers));
        if (maxDevices < -1) throw new ArgumentException("MaxDevices cannot be less than -1.", nameof(maxDevices));

        Name = name;
        NameAr = nameAr;
        PlanType = planType;
        MonthlyPrice = monthlyPrice;
        YearlyPrice = yearlyPrice;
        MaxBranches = maxBranches;
        MaxUsers = maxUsers;
        MaxDevices = maxDevices;
        MaxProducts = maxProducts;
        Description = description;
        SortOrder = sortOrder;
        IsActive = true;
        IsSystem = false;
    }

    public void Deactivate()
    {
        if (IsSystem)
            throw new InvalidOperationException("System plans cannot be deactivated.");
        IsActive = false;
    }

    public void UpdatePricing(Money monthly, Money yearly)
    {
        MonthlyPrice = monthly;
        YearlyPrice = yearly;
    }

    public bool HasUnlimitedBranches => MaxBranches == -1;
    public bool HasUnlimitedUsers => MaxUsers == -1;
}

/// <summary>
/// Subscription — الاشتراك (Entity داخل Tenant Aggregate)
/// يمثل اشتراك مستأجر في باقة محددة مع تواريخها وحدودها.
/// </summary>
public sealed class Subscription : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid PlanId { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public BillingCycle BillingCycle { get; private set; }
    public DateTimeOffset StartDate { get; private set; }
    public DateTimeOffset EndDate { get; private set; }
    public int MaxBranches { get; private set; }
    public int MaxUsers { get; private set; }
    public int MaxDevices { get; private set; }
    public Money Price { get; private set; }
    public string? Notes { get; private set; }

    private Subscription()
    {
        Price = Money.Zero();
    }

    public Subscription(
        Guid tenantId, Guid planId, BillingCycle billingCycle,
        DateTimeOffset startDate, DateTimeOffset endDate,
        int maxBranches, int maxUsers, int maxDevices,
        Money price, string? notes = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        if (planId == Guid.Empty) throw new ArgumentException("PlanId cannot be empty.", nameof(planId));
        if (endDate <= startDate) throw new ArgumentException("End date must be after start date.", nameof(endDate));
        if (maxBranches < 0) throw new ArgumentException("MaxBranches cannot be negative.", nameof(maxBranches));

        TenantId = tenantId;
        PlanId = planId;
        BillingCycle = billingCycle;
        StartDate = startDate;
        EndDate = endDate;
        MaxBranches = maxBranches;
        MaxUsers = maxUsers;
        MaxDevices = maxDevices;
        Price = price;
        Notes = notes;
        Status = SubscriptionStatus.Active;

        RaiseDomainEvent(new SubscriptionCreatedEvent(Id, TenantId, EndDate));
    }

    public void Suspend()
    {
        if (Status == SubscriptionStatus.Active)
        {
            Status = SubscriptionStatus.Suspended;
            RaiseDomainEvent(new SubscriptionSuspendedEvent(Id, TenantId));
        }
    }

    public void Resume()
    {
        if (Status == SubscriptionStatus.Suspended)
        {
            Status = SubscriptionStatus.Active;
            RaiseDomainEvent(new SubscriptionResumedEvent(Id, TenantId));
        }
    }

    public void Renew(DateTimeOffset newEndDate, Money price)
    {
        if (newEndDate <= EndDate) throw new ArgumentException("New end date must be after current end date.", nameof(newEndDate));
        
        EndDate = newEndDate;
        Price = price;
        Status = SubscriptionStatus.Active;
        
        RaiseDomainEvent(new SubscriptionRenewedEvent(Id, TenantId, newEndDate));
    }

    public void Extend(int days)
    {
        if (days <= 0) throw new ArgumentException("Days to extend must be positive.", nameof(days));
        EndDate = EndDate.AddDays(days);
    }

    public void SetTrial()
    {
        Status = SubscriptionStatus.Trial;
    }

    public void Activate()
    {
        Status = SubscriptionStatus.Active;
    }

    public void Expire()
    {
        if (Status == SubscriptionStatus.Active)
        {
            Status = SubscriptionStatus.Expired;
            RaiseDomainEvent(new SubscriptionExpiredEvent(Id, TenantId));
        }
    }

    public void Cancel()
    {
        Status = SubscriptionStatus.Cancelled;
        RaiseDomainEvent(new SubscriptionExpiredEvent(Id, TenantId));
    }

    public bool IsExpired => DateTimeOffset.UtcNow > EndDate;
    public int DaysRemaining => Math.Max(0, (int)(EndDate - DateTimeOffset.UtcNow).TotalDays);
}
