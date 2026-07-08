using GastroErp.Domain.Common;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Crm;

namespace GastroErp.Domain.Entities.Crm;

public sealed class LoyaltyAccount : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid CustomerId { get; private set; }
    public decimal CurrentPoints { get; private set; }
    public decimal EarnedPoints { get; private set; }
    public decimal RedeemedPoints { get; private set; }
    public decimal ExpiredPoints { get; private set; }
    
    public Guid? MembershipTierId { get; private set; }
    public MembershipTier? Tier { get; private set; }

    private readonly List<LoyaltyTransaction> _transactions = [];
    public IReadOnlyCollection<LoyaltyTransaction> Transactions => _transactions.AsReadOnly();

    private LoyaltyAccount() { }

    public LoyaltyAccount(Guid tenantId, Guid customerId)
    {
        TenantId = tenantId;
        CustomerId = customerId;
        CurrentPoints = 0;
        EarnedPoints = 0;
        RedeemedPoints = 0;
        ExpiredPoints = 0;
    }

    public void EarnPoints(decimal points, string reason, Guid? orderId = null)
    {
        if (points <= 0) throw new ArgumentException("Points must be positive.");
        
        CurrentPoints += points;
        EarnedPoints += points;
        
        _transactions.Add(new LoyaltyTransaction(TenantId, Id, LoyaltyTransactionType.Earn, points, reason, orderId));
        
        RaiseDomainEvent(new LoyaltyPointsEarnedEvent(Id, TenantId, points));
    }

    public void RedeemPoints(decimal points, string reason, Guid? orderId = null)
    {
        if (points <= 0) throw new ArgumentException("Points must be positive.");
        if (CurrentPoints < points) throw new InvalidOperationException("Insufficient points.");

        CurrentPoints -= points;
        RedeemedPoints += points;

        _transactions.Add(new LoyaltyTransaction(TenantId, Id, LoyaltyTransactionType.Redeem, -points, reason, orderId));

        RaiseDomainEvent(new LoyaltyPointsRedeemedEvent(Id, TenantId, points));
    }

    public void AdjustPoints(decimal points, string reason)
    {
        CurrentPoints += points;
        _transactions.Add(new LoyaltyTransaction(TenantId, Id, LoyaltyTransactionType.ManualAdjustment, points, reason));
    }

    public void ExpirePoints(decimal points, string reason)
    {
        if (points <= 0) throw new ArgumentException("Points must be positive.");
        if (CurrentPoints < points) throw new InvalidOperationException("Insufficient points to expire.");

        CurrentPoints -= points;
        ExpiredPoints += points;

        _transactions.Add(new LoyaltyTransaction(TenantId, Id, LoyaltyTransactionType.Expire, -points, reason));
    }

    public void UpdateTier(Guid tierId)
    {
        MembershipTierId = tierId;
        RaiseDomainEvent(new TierUpgradedEvent(Id, TenantId, tierId));
    }
}
