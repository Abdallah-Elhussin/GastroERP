namespace GastroErp.Domain.Enums;

/// <summary>حالة المستأجر</summary>
public enum TenantStatus
{
    Active = 1,
    Suspended = 2,
    Expired = 3,
    Cancelled = 4
}

/// <summary>نوع باقة الاشتراك</summary>
public enum SubscriptionPlanType
{
    Starter = 1,
    Professional = 2,
    Enterprise = 3
}

/// <summary>حالة الاشتراك</summary>
public enum SubscriptionStatus
{
    Trial = 0,
    Active = 1,
    Suspended = 2,
    Expired = 3,
    Cancelled = 4
}

/// <summary>دورة الفوترة</summary>
public enum BillingCycle
{
    Monthly = 1,
    Yearly = 2
}

/// <summary>نوع الفرع</summary>
public enum BranchType
{
    Restaurant = 1,
    CoffeeShop = 2,
    CloudKitchen = 3,
    FoodTruck = 4,
    Bakery = 5,
    Catering = 6,
    HotelRestaurant = 7,
    FoodCourt = 8,
    DarkKitchen = 9,
    CentralKitchen = 10
}

/// <summary>حالة الفرع</summary>
public enum BranchStatus
{
    Active = 1,
    Inactive = 2,
    UnderMaintenance = 3,
    Archived = 4
}

/// <summary>نوع الجهاز</summary>
public enum DeviceType
{
    POSTerminal = 1,
    KitchenDisplay = 2,
    CustomerDisplay = 3,
    OrderKiosk = 4,
    MobileDevice = 5,
    Printer = 6
}

/// <summary>أيام الأسبوع لساعات العمل</summary>
public enum BusinessDayOfWeek
{
    Sunday = 0,
    Monday = 1,
    Tuesday = 2,
    Wednesday = 3,
    Thursday = 4,
    Friday = 5,
    Saturday = 6
}
