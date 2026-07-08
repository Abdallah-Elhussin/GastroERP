using GastroErp.Domain.Entities.Crm;

namespace GastroErp.Application.Features.Crm.Services;

public interface ILoyaltyCalculationService
{
    decimal CalculateEarnedPoints(decimal orderTotal, Guid? tierId);
    decimal CalculateRedemptionValue(decimal points);
    Task EvaluateTierUpgradeAsync(Guid accountId, CancellationToken cancellationToken);
}

public interface IPromotionEngine
{
    Task<decimal> CalculateDiscountAsync(Guid promotionId, decimal orderTotal, CancellationToken cancellationToken);
    Task<List<PromotionCampaign>> GetActivePromotionsAsync(CancellationToken cancellationToken);
}

public interface ICouponValidationService
{
    Task<bool> ValidateCouponAsync(string code, decimal orderTotal, Guid? customerId, CancellationToken cancellationToken);
    Task<decimal> CalculateCouponDiscountAsync(string code, decimal orderTotal, CancellationToken cancellationToken);
}

public interface IGiftCardService
{
    Task<bool> ValidateGiftCardAsync(string cardNumber, decimal amountToRedeem, CancellationToken cancellationToken);
}

public interface ICustomerStatisticsService
{
    Task UpdateCustomerStatisticsAsync(Guid customerId, decimal orderAmount, Guid orderId, Guid branchId, CancellationToken cancellationToken);
}
