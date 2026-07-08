namespace GastroErp.Domain.Enums;

public enum CustomerStatus
{
    Active = 1,
    Inactive = 2,
    Blocked = 3
}

public enum LoyaltyTier
{
    Bronze = 1,
    Silver = 2,
    Gold = 3,
    Platinum = 4,
    VIP = 5
}

public enum LoyaltyTransactionType
{
    Earn = 1,
    Redeem = 2,
    ManualAdjustment = 3,
    Expire = 4,
    RefundAdjustment = 5
}

public enum CouponType
{
    FixedDiscount = 1,
    PercentageDiscount = 2,
    FreeItem = 3,
    FreeDelivery = 4
}

public enum PromotionType
{
    OrderDiscount = 1,
    ProductDiscount = 2,
    CategoryDiscount = 3,
    ComboPromotion = 4,
    HappyHour = 5,
    BuyXGetY = 6
}

public enum GiftCardStatus
{
    Active = 1,
    Inactive = 2,
    Depleted = 3,
    Expired = 4
}
