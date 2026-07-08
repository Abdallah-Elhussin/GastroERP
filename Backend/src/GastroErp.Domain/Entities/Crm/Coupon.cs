using GastroErp.Domain.Common;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Crm;

public sealed class Coupon : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public string Code { get; private set; }
    public CouponType Type { get; private set; }
    public decimal Value { get; private set; }
    public DateTimeOffset ValidFrom { get; private set; }
    public DateTimeOffset ValidTo { get; private set; }
    public int UsageLimit { get; private set; }
    public int RemainingUses { get; private set; }
    public decimal MinimumOrderAmount { get; private set; }
    public Guid? RestrictedToCustomerId { get; private set; }
    public bool IsActive { get; private set; }

    private Coupon() { Code = string.Empty; }

    public Coupon(Guid tenantId, string code, CouponType type, decimal value, DateTimeOffset validFrom, DateTimeOffset validTo, int usageLimit, decimal minOrder, Guid? restrictedCustomerId = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.");
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code is required.");
        
        TenantId = tenantId;
        Code = code;
        Type = type;
        Value = value;
        ValidFrom = validFrom;
        ValidTo = validTo;
        UsageLimit = usageLimit;
        RemainingUses = usageLimit;
        MinimumOrderAmount = minOrder;
        RestrictedToCustomerId = restrictedCustomerId;
        IsActive = true;
    }

    public void Redeem()
    {
        if (!IsActive) throw new InvalidOperationException("Coupon is not active.");
        if (RemainingUses <= 0) throw new InvalidOperationException("Coupon usage limit reached.");
        if (DateTimeOffset.UtcNow < ValidFrom || DateTimeOffset.UtcNow > ValidTo) throw new InvalidOperationException("Coupon is expired or not yet valid.");
        
        RemainingUses--;
    }
}
