namespace GastroErp.Domain.Enums;

/// <summary>
/// دورة مستندات المبيعات الإدارية (Back Office) — مستقلة عن POS.
/// Draft → Approved → Posted → Reversed → Cancelled
/// </summary>
public enum BackOfficeSalesDocumentStatus : byte
{
    Draft = 0,
    Approved = 1,
    Posted = 2,
    Reversed = 8,
    Cancelled = 9
}

/// <summary>طبيعة فاتورة المبيعات الإدارية (تؤثر على المخزون والقيد).</summary>
public enum BackOfficeSalesInvoiceNature : byte
{
    /// <summary>بيع بضاعة — خصم مخزون + COGS.</summary>
    Inventory = 1,
    /// <summary>بيع خدمات — بلا مخزون.</summary>
    Services = 2,
    /// <summary>بيع مشروع.</summary>
    Project = 3,
    /// <summary>بيع أصل ثابت — بلا مخزون تشغيلي.</summary>
    Assets = 4,
    /// <summary>مختلط — حسب طبيعة كل بند.</summary>
    Mixed = 5
}

/// <summary>طريقة السداد لفاتورة المبيعات الإدارية.</summary>
public enum BackOfficeSalesPaymentMode : byte
{
    Credit = 1,
    Cash = 2
}

/// <summary>طبيعة بند الفاتورة (للمختلط).</summary>
public enum BackOfficeSalesLineNature : byte
{
    Inventory = 1,
    Service = 2,
    Project = 3,
    Asset = 4
}

/// <summary>حالة سداد فاتورة المبيعات الإدارية.</summary>
public enum BackOfficeSalesPaymentStatus : byte
{
    Unpaid = 1,
    PartiallyPaid = 2,
    FullyPaid = 3
}

/// <summary>حالة تنفيذ أمر البيع الإداري (تسليم/فوترة).</summary>
public enum BackOfficeSalesFulfillmentStatus : byte
{
    Open = 0,
    PartiallyDelivered = 1,
    FullyDelivered = 2,
    PartiallyInvoiced = 3,
    FullyInvoiced = 4,
    Closed = 5
}
