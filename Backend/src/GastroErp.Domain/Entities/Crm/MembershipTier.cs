using GastroErp.Domain.Common;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Crm;

public sealed class MembershipTier : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; }
    public LoyaltyTier TierLevel { get; private set; }
    public decimal RequiredPoints { get; private set; }
    public decimal DiscountPercentage { get; private set; }
    public string? Benefits { get; private set; }
    public int Priority { get; private set; }

    private MembershipTier() { Name = string.Empty; }

    public MembershipTier(Guid tenantId, string name, LoyaltyTier tierLevel, decimal requiredPoints, decimal discountPercentage, int priority, string? benefits = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.");
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.");
        
        TenantId = tenantId;
        Name = name;
        TierLevel = tierLevel;
        RequiredPoints = requiredPoints;
        DiscountPercentage = discountPercentage;
        Priority = priority;
        Benefits = benefits;
    }
}
