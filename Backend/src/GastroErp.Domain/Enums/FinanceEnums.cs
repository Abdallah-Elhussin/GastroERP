namespace GastroErp.Domain.Enums;

public enum AccountType
{
    Asset = 1,
    Liability = 2,
    Equity = 3,
    Revenue = 4,
    Expense = 5
}

public enum AccountCategory
{
    CurrentAsset = 1,
    FixedAsset = 2,
    CurrentLiability = 3,
    LongTermLiability = 4,
    Equity = 5,
    OperatingRevenue = 6,
    OtherRevenue = 7,
    CostOfGoodsSold = 8,
    OperatingExpense = 9,
    OtherExpense = 10
}

public enum FiscalPeriodStatus
{
    Open = 1,
    Closed = 2,
    Locked = 3
}

public enum JournalStatus
{
    Draft = 1,
    Posted = 2,
    Reversed = 3
}

public enum PostingSource
{
    Manual = 1,
    Sales = 2,
    Payment = 3,
    Purchase = 4,
    Inventory = 5,
    Payroll = 6,
    Crm = 7
}

public enum CostCenterStatus
{
    Active = 1,
    Inactive = 2
}
