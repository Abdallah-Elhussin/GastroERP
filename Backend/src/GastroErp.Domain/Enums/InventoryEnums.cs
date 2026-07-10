namespace GastroErp.Domain.Enums;

/// <summary>طريقة تقييم المخزون</summary>
public enum InventoryCostingMethod
{
    FIFO = 1,
    WeightedAverage = 2,
    StandardCost = 3,
}

/// <summary>أنواع الحركات المخزنية</summary>
public enum TransactionType
{
    GoodsReceipt = 1,
    PurchaseReturn = 2,
    StockTransferIn = 3,
    StockTransferOut = 4,
    StockAdjustmentPositive = 5,
    StockAdjustmentNegative = 6,
    Waste = 7,
    SalesConsumption = 8,
    ProductionIssue = 9,
    ProductionReceipt = 10,
    StockCountCorrection = 11
}

/// <summary>حالة أمر الشراء</summary>
public enum PurchaseOrderStatus
{
    Draft = 1,
    Approved = 2,
    SentToSupplier = 3,
    PartiallyReceived = 4,
    FullyReceived = 5,
    Cancelled = 6,
    Closed = 7,
    Rejected = 8,
    PendingApproval = 9
}

/// <summary>حالة عملية الاستلام</summary>
public enum GoodsReceiptStatus
{
    Draft = 1,
    Completed = 2,
    Cancelled = 3
}

/// <summary>حالة التحويل المخزني</summary>
public enum StockTransferStatus
{
    Draft = 1,
    InTransit = 2,
    Completed = 3,
    Cancelled = 4
}

/// <summary>حالة الجرد</summary>
public enum StockCountStatus
{
    Draft = 1,
    InProgress = 2,
    Review = 3,
    Completed = 4,
    Cancelled = 5
}

/// <summary>حالة الحجز المخزني</summary>
public enum ReservationStatus
{
    Active = 1,
    Fulfilled = 2,
    Expired = 3,
    Cancelled = 4
}

/// <summary>حالة التشغيلة / الدفعة</summary>
public enum BatchStatus
{
    Active = 1,
    Quarantine = 2,
    Expired = 3,
    Depleted = 4,
    Recalled = 5
}

/// <summary>حالة الوصفة</summary>
public enum RecipeStatus
{
    Draft = 1,
    Active = 2,
    Obsolete = 3
}

/// <summary>نوع الصنف المخزني — خام أو مصنع</summary>
public enum InventoryItemKind
{
    Raw = 1,
    Manufactured = 2
}
