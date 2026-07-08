namespace GastroErp.Domain.Enums;

/// <summary>نوع المنيو</summary>
public enum MenuType
{
    /// <summary>منيو قياسي دائم</summary>
    Standard = 1,
    /// <summary>منيو موسمي مؤقت</summary>
    Seasonal = 2,
    /// <summary>منيو رقمي للتطبيق</summary>
    Digital = 3,
    /// <summary>منيو الكيوسك</summary>
    Kiosk = 4,
    /// <summary>منيو الإفطار</summary>
    Breakfast = 5
}

/// <summary>قناة البيع المرتبطة بالمنيو أو مستوى السعر</summary>
public enum SalesChannel
{
    /// <summary>جميع القنوات</summary>
    All = 0,
    /// <summary>الطلب داخل المطعم</summary>
    DineIn = 1,
    /// <summary>الطلب للسفر</summary>
    TakeAway = 2,
    /// <summary>التوصيل</summary>
    Delivery = 3,
    /// <summary>الكيوسك الذاتي</summary>
    Kiosk = 4,
    /// <summary>التطبيق الإلكتروني</summary>
    Online = 5
}
