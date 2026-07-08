namespace GastroErp.Domain.Enums;

public enum InvoiceStatus
{
    Draft = 1,
    Finalized = 2,
    Cancelled = 3
}

public enum InvoiceType
{
    Sales = 1,
    Tax = 2,
    Simplified = 3,
    Credit = 4,
    Debit = 5
}

public enum TaxType
{
    VAT = 1,
    SalesTax = 2,
    Excise = 3,
    Service = 4,
    Other = 5
}

public enum TaxCalculationMethod
{
    Percentage = 1,
    FixedAmount = 2
}

public enum InvoicePaymentStatus
{
    Unpaid = 1,
    PartiallyPaid = 2,
    Paid = 3
}

public enum CreditNoteStatus
{
    Draft = 1,
    Issued = 2,
    Cancelled = 3
}

public enum DebitNoteStatus
{
    Draft = 1,
    Issued = 2,
    Cancelled = 3
}

public enum CreditNoteType
{
    Full = 1,
    Partial = 2,
    Item = 3
}
