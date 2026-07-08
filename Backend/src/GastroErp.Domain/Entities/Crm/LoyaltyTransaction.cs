using GastroErp.Domain.Common;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Crm;

public sealed class LoyaltyTransaction : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid LoyaltyAccountId { get; private set; }
    public LoyaltyTransactionType Type { get; private set; }
    public decimal Points { get; private set; }
    public string Reason { get; private set; }
    public Guid? OrderId { get; private set; }

    private LoyaltyTransaction() { Reason = string.Empty; }

    internal LoyaltyTransaction(Guid tenantId, Guid accountId, LoyaltyTransactionType type, decimal points, string reason, Guid? orderId = null)
    {
        TenantId = tenantId;
        LoyaltyAccountId = accountId;
        Type = type;
        Points = points;
        Reason = reason;
        OrderId = orderId;
    }
}
