namespace GastroErp.Domain.Enums;

/// <summary>حالة طلب البيع</summary>
public enum OrderStatus
{
    Draft = 1,
    Pending = 2,
    Confirmed = 3,
    Preparing = 4,
    Ready = 5,
    Served = 6,
    Completed = 7,
    Cancelled = 8,
    Archived = 9
}

/// <summary>نوع الطلب</summary>
public enum OrderType
{
    DineIn = 1,
    TakeAway = 2,
    Delivery = 3,
    DriveThru = 4,
    QROrdering = 5,
    Kiosk = 6
}

/// <summary>نوع الخصم</summary>
public enum DiscountType
{
    Percentage = 1,
    FixedAmount = 2
}

/// <summary>حالة عنصر المطبخ في الطلب</summary>
public enum KitchenItemStatus
{
    Pending = 1,
    Preparing = 2,
    Ready = 3,
    Served = 4,
    Voided = 5
}

/// <summary>حالة المزامنة (Offline-First)</summary>
public enum SyncStatus
{
    Local = 1,
    PendingSync = 2,
    Synced = 3,
    Conflict = 4
}

/// <summary>حالة الدفع على مستوى الطلب</summary>
public enum OrderPaymentStatus
{
    Unpaid = 1,
    PartiallyPaid = 2,
    Paid = 3,
    Refunded = 4,
    PartiallyRefunded = 5
}

/// <summary>حالة عملية الدفع</summary>
public enum PaymentStatus
{
    Pending = 1,
    Authorized = 2,
    Completed = 3,
    Failed = 4,
    Voided = 5,
    Cancelled = 6,
    Refunded = 7,
    PartiallyRefunded = 8
}

/// <summary>طريقة الدفع</summary>
public enum PaymentMethodType
{
    Cash = 1,
    CreditCard = 2,
    DebitCard = 3,
    Mada = 4,
    ApplePay = 5,
    GooglePay = 6,
    STCPay = 7,
    BankTransfer = 8,
    GiftCard = 9,
    Voucher = 10,
    StoreCredit = 11,
    Other = 99
}

/// <summary>حالة وردية الكاشير</summary>
public enum ShiftStatus
{
    Open = 1,
    Active = 2,
    Suspended = 3,
    Closing = 4,
    Closed = 5,
    Reconciled = 6
}

/// <summary>حالة الخزينة</summary>
public enum RegisterStatus
{
    Closed = 1,
    Open = 2,
    Suspended = 3
}

/// <summary>حالة الاسترداد</summary>
public enum RefundStatus
{
    Pending = 1,
    Approved = 2,
    Processed = 3,
    Rejected = 4
}

/// <summary>نوع الحركة النقدية</summary>
public enum CashMovementType
{
    CashIn = 1,
    CashOut = 2,
    SafeDeposit = 3,
    SafeWithdrawal = 4,
    PettyCash = 5,
    Expense = 6,
    Float = 7,
    Sale = 8,
    Refund = 9,
    Variance = 10,
    Tip = 11
}

/// <summary>حالة التسوية</summary>
public enum ReconciliationStatus
{
    Pending = 1,
    Balanced = 2,
    VarianceDetected = 3,
    Approved = 4
}

/// <summary>حالة تذكرة المطبخ</summary>
public enum KitchenTicketStatus
{
    Pending = 1,
    InProgress = 2,
    Ready = 3,
    Completed = 4,
    Cancelled = 5
}

/// <summary>نوع محطة المطبخ</summary>
public enum KitchenStationType
{
    Hot = 1,
    Cold = 2,
    Grill = 3,
    Fry = 4,
    Bar = 5,
    Dessert = 6,
    Expo = 7,
    General = 99
}

/// <summary>حالة الطاولة</summary>
public enum TableStatus
{
    Available = 1,
    Occupied = 2,
    Reserved = 3,
    Cleaning = 4,
    OutOfService = 5
}

/// <summary>شكل الطاولة</summary>
public enum TableShape
{
    Square = 1,
    Round = 2,
    Rectangle = 3,
    Bar = 4
}

/// <summary>حالة حجز الطاولة</summary>
public enum TableReservationStatus
{
    Pending = 1,
    Confirmed = 2,
    Seated = 3,
    Completed = 4,
    Cancelled = 5,
    NoShow = 6
}
