using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Crm.DTOs;

public record CustomerDto(
    Guid Id,
    string CustomerNumber,
    string FullName,
    string Mobile,
    string? Email,
    DateTime? DateOfBirth,
    string? Gender,
    string? PreferredLanguage,
    string? Notes,
    CustomerStatus Status,
    string? TaxNumber,
    Guid? ArAccountId,
    string Currency,
    int PaymentDueDays,
    string? PaymentTerms,
    decimal CreditLimit,
    int TotalOrders,
    decimal TotalSpending,
    decimal AverageTicket,
    DateTimeOffset? LastVisit,
    Guid? LastOrderId,
    Guid? FavoriteBranchId,
    Guid? LoyaltyAccountId);

public record CreateCustomerDto(
    string FullName,
    string Mobile,
    string? Email,
    DateTime? DateOfBirth,
    string? Gender,
    string? PreferredLanguage,
    string? Notes,
    string? TaxNumber = null,
    Guid? ArAccountId = null,
    string? Currency = null,
    int PaymentDueDays = 0,
    string? PaymentTerms = null,
    decimal CreditLimit = 0);

public record UpdateCustomerDto(
    string FullName,
    string Mobile,
    string? Email,
    DateTime? DateOfBirth,
    string? Gender,
    string? PreferredLanguage,
    string? Notes,
    string? TaxNumber = null,
    Guid? ArAccountId = null,
    string? Currency = null,
    int PaymentDueDays = 0,
    string? PaymentTerms = null,
    decimal CreditLimit = 0);

public record LoyaltyAccountDto(Guid Id, Guid CustomerId, decimal CurrentPoints, decimal EarnedPoints, decimal RedeemedPoints, decimal ExpiredPoints, Guid? MembershipTierId);
public record LoyaltyTransactionDto(Guid Id, Guid LoyaltyAccountId, LoyaltyTransactionType Type, decimal Points, string Reason, Guid? OrderId, DateTimeOffset CreatedAt);

public record MembershipTierDto(Guid Id, string Name, LoyaltyTier TierLevel, decimal RequiredPoints, decimal DiscountPercentage, string? Benefits, int Priority);
public record CreateMembershipTierDto(string Name, LoyaltyTier TierLevel, decimal RequiredPoints, decimal DiscountPercentage, string? Benefits, int Priority);

public record CouponDto(Guid Id, string Code, CouponType Type, decimal Value, DateTimeOffset ValidFrom, DateTimeOffset ValidTo, int UsageLimit, int RemainingUses, decimal MinimumOrderAmount, Guid? RestrictedToCustomerId, bool IsActive);
public record CreateCouponDto(string Code, CouponType Type, decimal Value, DateTimeOffset ValidFrom, DateTimeOffset ValidTo, int UsageLimit, decimal MinimumOrderAmount, Guid? RestrictedToCustomerId);

public record PromotionCampaignDto(Guid Id, string Name, PromotionType Type, decimal Value, DateTimeOffset StartDate, DateTimeOffset EndDate, int Priority, bool Stackable, bool IsActive, string? ConfigurationJson);
public record CreatePromotionCampaignDto(string Name, PromotionType Type, decimal Value, DateTimeOffset StartDate, DateTimeOffset EndDate, int Priority, bool Stackable, string? ConfigurationJson);

public record GiftCardDto(Guid Id, string CardNumber, decimal InitialValue, decimal CurrentBalance, DateTimeOffset? ExpiryDate, GiftCardStatus Status, Guid? CustomerId);
public record IssueGiftCardDto(string CardNumber, decimal InitialValue, DateTimeOffset? ExpiryDate, Guid? CustomerId);
public record RechargeGiftCardDto(decimal Amount);
