namespace GastroErp.Domain.Enums;

/// <summary>الوحدة الوظيفية التابعة لنوع المستند.</summary>
public enum DocumentModule : byte
{
    Inventory = 1,
    Purchasing = 2,
    Sales = 3,
    Finance = 4,
    Hr = 5,
    Production = 6,
    Maintenance = 7,
    Pos = 8,
    General = 9
}

/// <summary>وضع الاعتماد لنوع المستند.</summary>
public enum DocumentApprovalMode : byte
{
    None = 0,
    Single = 1,
    MultiLevel = 2,
    ByAmount = 3,
    ByBranch = 4,
    ByDepartment = 5
}

/// <summary>طريقة الترحيل المحاسبي / المخزني.</summary>
public enum DocumentPostingMode : byte
{
    Manual = 0,
    AutoPost = 1,
    PostAfterApproval = 2,
    JournalOnly = 3,
    StockMovementOnly = 4,
    JournalAndStock = 5
}
