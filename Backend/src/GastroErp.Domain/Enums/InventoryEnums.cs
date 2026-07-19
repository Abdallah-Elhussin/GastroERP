namespace GastroErp.Domain.Enums;

/// <summary>طريقة تقييم المخزون</summary>
public enum InventoryCostingMethod
{
    FIFO = 1,
    WeightedAverage = 2,
    StandardCost = 3,
}

/// <summary>أنواع سلاسل ترقيم مستندات المخزون</summary>
public enum InventoryDocumentSeriesType : byte
{
    GoodsReceipt = 1,
    GoodsIssue = 2,
    StockTransfer = 3,
    InventoryAdjustment = 4,
    InventoryCount = 5,
    Waste = 6,
    OpeningBalance = 7,
    Reservation = 8,
    ProductionIssue = 9,
    ProductionReceipt = 10
}

/// <summary>أنواع المستودعات</summary>
public enum WarehouseType : byte
{
    Main = 1,
    POS = 2,
    Production = 3,
    RawMaterial = 4,
    FinishedGoods = 5,
    Returns = 6,
    Damaged = 7,
    Transit = 8,
    Kitchen = 9,
    Beverage = 10,
    DryStore = 11,
    Chiller = 12,
    Freezer = 13,
    Packaging = 14,
    Cleaning = 15,
    Waste = 16
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
    StockCountCorrection = 11,
    Reversal = 12,
    OpeningBalance = 13,
    GoodsIssue = 14,
    StockTransferOutReversal = 15,
    StockTransferInReversal = 16
}

/// <summary>حالة أمر الشراء — دورة تشغيلية بدون ترحيل محاسبي.
/// الرئيسي للمستخدم: مسودة → معتمد → مغلق / ملغي.
/// Partially/FullyReceived تُشتق من الكميات المستلمة (حقائق استلام) وليست بديلاً عن الترحيل.</summary>
/// <summary>
/// حالات أمر الشراء — طلب شراء فقط (لا قيود محاسبية).
/// الدورة الرسمية: مسودة → معتمد → مغلق / ملغي.
/// الحالات الإضافية (Sent/Partially/FullyReceived) حالات تشغيلية للمتابعة فقط — ليست ترحيلاً محاسبياً.
/// </summary>
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

/// <summary>حالة سند الاستلام (GRN). عرض UI: 0 مسودة / 1 معتمد / 2 مرحل / 8 معكوس / 9 ملغي.</summary>
public enum GoodsReceiptStatus
{
    Draft = 1,
    /// <summary>Posted — inventory + GRNI journal applied (legacy value kept as 2).</summary>
    Posted = 2,
    Cancelled = 3,
    Approved = 4,
    Reversed = 8
}

/// <summary>مصدر إنشاء سند الاستلام.</summary>
public enum GoodsReceiptSource : byte
{
    FromPurchaseOrder = 1,
    Direct = 2
}

/// <summary>نتيجة فحص الجودة على مستوى السند.</summary>
public enum InspectionResult : byte
{
    Accepted = 1,
    PartiallyAccepted = 2,
    Rejected = 3
}

/// <summary>Unified purchasing document lifecycle codes (spec §9).</summary>
public enum PurchasingDocumentStatus : byte
{
    Draft = 0,
    Approved = 1,
    Posted = 2,
    Reversed = 8,
    Cancelled = 9
}

/// <summary>أنواع المورد.</summary>
public enum SupplierType : byte
{
    Local = 1,
    International = 2,
    Service = 3,
    Manufacturer = 4,
    Importer = 5
}

/// <summary>تصنيف المورد.</summary>
public enum SupplierCategory : byte
{
    Food = 1,
    Beverages = 2,
    Equipment = 3,
    Maintenance = 4,
    Services = 5,
    Other = 9
}

/// <summary>طريقة الدفع الافتراضية للمورد.</summary>
public enum SupplierPaymentMethodKind : byte
{
    Cash = 1,
    Credit = 2,
    BankTransfer = 3,
    Cheque = 4,
    Card = 5
}

/// <summary>طريقة تقييم الضريبة على مشتريات المورد.</summary>
public enum SupplierVatEvaluation : byte
{
    ExcludeVat = 1,
    IncludeVat = 2
}

/// <summary>Purchase invoice path.</summary>
public enum PurchaseInvoiceKind : byte
{
    FromReceipt = 1,
    Direct = 2
}

/// <summary>Settlement mode for purchase invoice posting.</summary>
public enum PurchaseInvoicePaymentMode : byte
{
    Credit = 1,
    Cash = 2
}

/// <summary>طبيعة فاتورة المشتريات المباشرة (يؤثر على المخزون والقيد).</summary>
public enum DirectPurchaseNature : byte
{
    /// <summary>شراء بضاعة — يدخل المخزون.</summary>
    Inventory = 1,
    /// <summary>شراء خدمات — مصروف، بلا مخزون.</summary>
    Services = 2,
    /// <summary>شراء أصول ثابتة — بلا مخزون.</summary>
    FixedAssets = 3
}

/// <summary>Invoice settlement state — system-managed only.</summary>
public enum PurchaseInvoicePaymentStatus : byte
{
    Unpaid = 1,
    PartiallyPaid = 2,
    FullyPaid = 3,
    FullyReturned = 4
}

/// <summary>أنواع مرتجع المشتريات (ثلاثة فقط).</summary>
public enum PurchaseReturnType : byte
{
    /// <summary>من سند استلام قبل إصدار الفاتورة — يعكس المخزون وGRNI فقط.</summary>
    BeforeInvoice = 1,
    /// <summary>من فاتورة مشتريات آجلة — يعكس المخزون والضريبة وذمة المورد.</summary>
    AfterInvoice = 2,
    /// <summary>من فاتورة مشتريات مباشرة — نقداً أو آجلاً.</summary>
    Direct = 3
}

/// <summary>حالة التحويل المخزني — Draft → Approved → InTransit(مرحل) → Completed(مستلم) | Cancelled</summary>
public enum StockTransferStatus : byte
{
    Draft = 1,
    InTransit = 2,
    Completed = 3,
    Cancelled = 4,
    Approved = 5
}

/// <summary>نوع مستند التحويل (منظور الحركة).</summary>
public enum StockTransferType : byte
{
    Outbound = 1,
    Inbound = 2
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

/// <summary>نوع وحدة القياس — مقاسة أو عددية</summary>
public enum InventoryUnitType : byte
{
    Measured = 1,
    Count = 2
}

/// <summary>تصنيف وحدة القياس</summary>
public enum InventoryUnitClassification : byte
{
    Weight = 1,
    Volume = 2,
    Length = 3,
    Count = 4,
    Packaging = 5,
    Other = 6
}

/// <summary>نوع بيانات سمة المخزون</summary>
public enum InventoryAttributeDataType : byte
{
    Text = 1,
    Number = 2,
    Boolean = 3,
    List = 4
}

/// <summary>
/// اتجاه الحركة المخزنية — الكمية دائماً موجبة؛ الاتجاه من هذا النوع فقط.
/// </summary>
public enum InventoryMovementType : byte
{
    IN = 1,
    OUT = 2,
    TRO = 3,
    TRI = 4,
    ADJ = 5,
    REV = 6
}
