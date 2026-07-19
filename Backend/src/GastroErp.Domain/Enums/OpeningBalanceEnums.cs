namespace GastroErp.Domain.Enums;

/// <summary>حالة مستند الرصيد الافتتاحي.</summary>
public enum OpeningBalanceStatus : byte
{
    Draft = 1,
    Approved = 2,
    Posted = 3
}

/// <summary>طريقة إدخال الرصيد الافتتاحي.</summary>
public enum OpeningBalanceEntryMethod : byte
{
    Manual = 1,
    ExcelImport = 2,
    AutoGenerate = 3
}

/// <summary>طريقة عرض أسطر الرصيد الافتتاحي.</summary>
public enum OpeningBalanceDisplayMethod : byte
{
    ByItem = 1,
    ByWarehouse = 2,
    ByCategory = 3
}

/// <summary>نطاق المتوسط المرجح عند الترحيل.</summary>
public enum WeightedAverageScope : byte
{
    Warehouse = 1,
    Branch = 2,
    Company = 3
}
