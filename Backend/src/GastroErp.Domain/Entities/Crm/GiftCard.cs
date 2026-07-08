using GastroErp.Domain.Common;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Crm;

namespace GastroErp.Domain.Entities.Crm;

public sealed class GiftCard : AuditableBaseEntity
{
    public Guid TenantId { get; private set; }
    public string CardNumber { get; private set; }
    public decimal InitialValue { get; private set; }
    public decimal CurrentBalance { get; private set; }
    public DateTimeOffset? ExpiryDate { get; private set; }
    public GiftCardStatus Status { get; private set; }
    
    public Guid? CustomerId { get; private set; }

    private GiftCard() { CardNumber = string.Empty; }

    public GiftCard(Guid tenantId, string cardNumber, decimal initialValue, DateTimeOffset? expiryDate, Guid? customerId = null)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("TenantId cannot be empty.");
        if (string.IsNullOrWhiteSpace(cardNumber)) throw new ArgumentException("Card number is required.");
        if (initialValue < 0) throw new ArgumentException("Initial value cannot be negative.");
        
        TenantId = tenantId;
        CardNumber = cardNumber;
        InitialValue = initialValue;
        CurrentBalance = initialValue;
        ExpiryDate = expiryDate;
        CustomerId = customerId;
        Status = GiftCardStatus.Active;
    }

    public void Redeem(decimal amount, Guid orderId)
    {
        if (Status != GiftCardStatus.Active) throw new InvalidOperationException("Gift card is not active.");
        if (amount <= 0) throw new ArgumentException("Redeem amount must be positive.");
        if (CurrentBalance < amount) throw new InvalidOperationException("Insufficient balance.");
        if (ExpiryDate.HasValue && DateTimeOffset.UtcNow > ExpiryDate.Value)
        {
            Status = GiftCardStatus.Expired;
            throw new InvalidOperationException("Gift card has expired.");
        }
        
        CurrentBalance -= amount;
        
        if (CurrentBalance == 0)
        {
            Status = GiftCardStatus.Depleted;
        }

        RaiseDomainEvent(new GiftCardRedeemedEvent(Id, TenantId, amount, orderId));
    }

    public void Recharge(decimal amount)
    {
        if (amount <= 0) throw new ArgumentException("Recharge amount must be positive.");
        if (Status == GiftCardStatus.Expired) throw new InvalidOperationException("Cannot recharge an expired gift card.");
        
        CurrentBalance += amount;
        Status = GiftCardStatus.Active;
    }
}
