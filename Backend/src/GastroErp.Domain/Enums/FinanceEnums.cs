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

public enum FiscalPeriodPolicy
{
    Monthly = 1
}

public enum JournalStatus
{
    Draft = 1,
    Posted = 2,
    Reversed = 3,
    /// <summary>Approved and ready for posting (manual vouchers).</summary>
    Approved = 4
}

/// <summary>Manual journal voucher classification (نوع القيد).</summary>
public enum JournalVoucherType : byte
{
    Ordinary = 1,
    Adjustment = 2,
    Closing = 3,
    /// <summary>Created only from Opening Balances posting — not selectable on journal screen.</summary>
    Opening = 4,
    Reversal = 5
}

public enum PostingSource
{
    Manual = 1,
    Sales = 2,
    Payment = 3,
    Purchase = 4,
    Inventory = 5,
    Payroll = 6,
    Crm = 7,
    OpeningBalance = 8,
    Receipt = 9,
    PaymentVoucher = 10,
    DebitNote = 11,
    CreditNote = 12
}

/// <summary>Kind of financial adjustment note (إشعار مدين / دائن).</summary>
public enum FinancialNoteKind : byte
{
    Debit = 1,
    Credit = 2
}

/// <summary>Lifecycle for financial debit/credit notes.</summary>
public enum FinancialNoteStatus
{
    Draft = 1,
    Submitted = 2,
    Approved = 3,
    Posted = 4,
    Reversed = 5,
    Cancelled = 6
}

/// <summary>Optional reference document linked to a financial note.</summary>
public enum FinancialNoteReferenceType : byte
{
    None = 0,
    SalesInvoice = 1,
    PurchaseInvoice = 2,
    ReceiptVoucher = 3,
    PaymentVoucher = 4,
    JournalVoucher = 5
}

/// <summary>Lifecycle for financial opening balance documents.</summary>
public enum FinancialOpeningBalanceStatus
{
    Draft = 1,
    Posted = 2,
    Reversed = 3
}

/// <summary>Lifecycle for receipt vouchers (سندات القبض).</summary>
public enum ReceiptVoucherStatus
{
    Draft = 1,
    Submitted = 2,
    Approved = 3,
    Posted = 4,
    Reversed = 5,
    Cancelled = 6
}

/// <summary>طريقة القبض.</summary>
public enum ReceiptMethod : byte
{
    Cash = 1,
    /// <summary>Kept for compatibility with earlier Bank=2 coding.</summary>
    BankTransfer = 2,
    Cheque = 3,
    CreditCard = 4,
    DebitCard = 5,
    Wallet = 6,
    Other = 7
}

/// <summary>طرف سند القبض.</summary>
public enum ReceiptPartyType : byte
{
    Customer = 1,
    General = 2,
    Supplier = 3
}

public enum CostCenterStatus
{
    Active = 1,
    Inactive = 2
}

/// <summary>نوع مركز التكلفة</summary>
public enum CostCenterType : byte
{
    Operational = 1,
    Administrative = 2,
    Production = 3,
    Service = 4,
    Branch = 5,
    Project = 6
}

public enum CurrencyStatus : byte
{
    Active = 1,
    Inactive = 2
}
