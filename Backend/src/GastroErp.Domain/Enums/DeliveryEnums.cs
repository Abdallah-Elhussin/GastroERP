namespace GastroErp.Domain.Enums;

public enum DeliveryStatus
{
    Pending = 1,
    Assigned = 2,
    PickedUp = 3,
    InTransit = 4,
    Delivered = 5,
    Failed = 6,
    Cancelled = 7
}

public enum DeliveryPriority
{
    Normal = 1,
    Express = 2,
    Scheduled = 3
}

public enum DriverStatus
{
    Available = 1,
    OnDelivery = 2,
    OffDuty = 3,
    Suspended = 4
}

public enum DeliveryPaymentMode
{
    Prepaid = 1,
    CashOnDelivery = 2,
    CardOnDelivery = 3
}

public enum DeliveryProviderType
{
    Internal = 1,
    Jahez = 2,
    HungerStation = 3,
    Ninja = 4,
    ToYou = 5,
    Keta = 6,
    External = 99
}

public enum DeliveryZoneFeeType
{
    Fixed = 1,
    PerKilometer = 2
}
