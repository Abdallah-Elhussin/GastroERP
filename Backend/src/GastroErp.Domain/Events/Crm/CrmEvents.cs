using GastroErp.Domain.Common;

namespace GastroErp.Domain.Events.Crm;

public sealed record CustomerCreatedEvent(Guid CustomerId, Guid TenantId) : IDomainEvent { public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow; }
public sealed record CustomerUpdatedEvent(Guid CustomerId, Guid TenantId) : IDomainEvent { public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow; }
public sealed record LoyaltyPointsEarnedEvent(Guid LoyaltyAccountId, Guid TenantId, decimal PointsEarned) : IDomainEvent { public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow; }
public sealed record LoyaltyPointsRedeemedEvent(Guid LoyaltyAccountId, Guid TenantId, decimal PointsRedeemed) : IDomainEvent { public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow; }
public sealed record TierUpgradedEvent(Guid LoyaltyAccountId, Guid TenantId, Guid NewTierId) : IDomainEvent { public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow; }
public sealed record CouponIssuedEvent(Guid CouponId, Guid TenantId) : IDomainEvent { public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow; }
public sealed record CouponRedeemedEvent(Guid CouponId, Guid TenantId, Guid OrderId) : IDomainEvent { public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow; }
public sealed record PromotionActivatedEvent(Guid PromotionId, Guid TenantId) : IDomainEvent { public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow; }
public sealed record GiftCardRedeemedEvent(Guid GiftCardId, Guid TenantId, decimal Amount, Guid OrderId) : IDomainEvent { public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow; }
