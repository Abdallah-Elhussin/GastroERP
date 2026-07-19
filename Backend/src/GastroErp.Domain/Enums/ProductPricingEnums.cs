namespace GastroErp.Domain.Enums;

/// <summary>طريقة تسعير المنتج.</summary>
public enum PricingMethod : byte
{
    /// <summary>سعر ثابت محدد مسبقاً.</summary>
    Fixed = 1,

    /// <summary>التكلفة + نسبة هامش.</summary>
    CostPlusMarginPercent = 2,

    /// <summary>التكلفة + ربح ثابت.</summary>
    CostPlusFixedProfit = 3,

    /// <summary>تحديد يدوي لسعر البيع.</summary>
    Manual = 4
}

/// <summary>مصدر التكلفة المرجعية المستخدم عند حساب السعر.</summary>
public enum ProductCostType : byte
{
    /// <summary>متوسط التكلفة المرجح.</summary>
    Average = 1,

    /// <summary>آخر تكلفة شراء.</summary>
    LastPurchase = 2,

    /// <summary>التكلفة المعيارية.</summary>
    Standard = 3
}
