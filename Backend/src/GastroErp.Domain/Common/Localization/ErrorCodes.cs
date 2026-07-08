namespace GastroErp.Domain.Common.Localization;

public static class ErrorCodes
{
    // Common Validation
    public const string RequiredField = "Validation.RequiredField";
    public const string MaxLengthExceeded = "Validation.MaxLengthExceeded";
    public const string NameRequired = "Validation.NameRequired";
    public const string NameArRequired = "Validation.NameArRequired";
    public const string NameEnRequired = "Validation.NameEnRequired";
    public const string InvalidEmail = "Validation.InvalidEmail";
    public const string InvalidTripleName = "Validation.InvalidTripleName";
    // Organization Module
    public const string CompanyDeactivated = "Organization.CompanyDeactivated";
    public const string CompanyAlreadyActive = "Organization.CompanyAlreadyActive";
    public const string CompanyAlreadyInactive = "Organization.CompanyAlreadyInactive";
    public const string BranchAlreadyExists = "Organization.BranchAlreadyExists";
    
    // Menu Module
    public const string CategoryNotFound = "Menu.CategoryNotFound";
    public const string ProductNotFound = "Menu.ProductNotFound";
    
    // Inventory Module
    public const string InsufficientStock = "Inventory.InsufficientStock";
    public const string WarehouseNotFound = "Inventory.WarehouseNotFound";
    public const string InvalidStatusTransition = "Inventory.InvalidStatusTransition";
    public const string CannotModifyApprovedDocument = "Inventory.CannotModifyApprovedDocument";
    public const string ItemAlreadyAdded = "Inventory.ItemAlreadyAdded";
    public const string ItemNotFound = "Inventory.ItemNotFound";

    // Sales / POS Module
    public const string OrderNotFound = "Sales.OrderNotFound";
    public const string OrderAlreadyClosed = "Sales.OrderAlreadyClosed";
    public const string OrderCannotBeCancelled = "Sales.OrderCannotBeCancelled";
    public const string SalesInvalidStatusTransition = "Sales.InvalidStatusTransition";
    public const string OrderHasNoItems = "Sales.OrderHasNoItems";
    public const string SalesItemNotFound = "Sales.ItemNotFound";
    public const string ItemAlreadyVoided = "Sales.ItemAlreadyVoided";
    public const string VoidReasonRequired = "Sales.VoidReasonRequired";
    public const string TableRequired = "Sales.TableRequired";
    public const string OfflineSalesNotAllowed = "Sales.OfflineSalesNotAllowed";
    public const string ReopenWindowExpired = "Sales.ReopenWindowExpired";
    public const string ProductNotAvailable = "Sales.ProductNotAvailable";
    public const string BranchNotFound = "Sales.BranchNotFound";
    public const string DeviceNotFound = "Sales.DeviceNotFound";
    public const string PaymentNotFound = "Sales.PaymentNotFound";
    public const string PaymentExceedsBalance = "Sales.PaymentExceedsBalance";
    public const string OrderCannotAcceptPayment = "Sales.OrderCannotAcceptPayment";
    public const string ShiftClosed = "Sales.ShiftClosed";
    public const string ShiftNotFound = "Sales.ShiftNotFound";
    public const string ShiftAlreadyOpen = "Sales.ShiftAlreadyOpen";
    public const string RegisterClosed = "Sales.RegisterClosed";
    public const string RegisterNotFound = "Sales.RegisterNotFound";
    public const string RegisterAlreadyOpen = "Sales.RegisterAlreadyOpen";
    public const string RefundExceedsPaid = "Sales.RefundExceedsPaid";
    public const string PaymentAlreadyVoided = "Sales.PaymentAlreadyVoided";
    public const string ManagerApprovalRequired = "Sales.ManagerApprovalRequired";
    public const string InvalidPaymentAmount = "Sales.InvalidPaymentAmount";
    public const string TableNotFound = "Sales.TableNotFound";
    public const string TableNotAvailable = "Sales.TableNotAvailable";
    public const string TableAlreadyOccupied = "Sales.TableAlreadyOccupied";
    public const string FloorPlanNotFound = "Sales.FloorPlanNotFound";
    public const string KitchenStationNotFound = "Sales.KitchenStationNotFound";
    public const string KitchenTicketNotFound = "Sales.KitchenTicketNotFound";
    public const string TableReservationNotFound = "Sales.TableReservationNotFound";

    // Invoicing Module
    public const string InvoiceNotFound = "Invoicing.InvoiceNotFound";
    public const string InvoiceAlreadyFinalized = "Invoicing.InvoiceAlreadyFinalized";
    public const string InvoiceAlreadyCancelled = "Invoicing.InvoiceAlreadyCancelled";
    public const string InvoiceNotEditable = "Invoicing.InvoiceNotEditable";
    public const string InvoiceNotFinalized = "Invoicing.InvoiceNotFinalized";
    public const string InvoiceHasNoLines = "Invoicing.InvoiceHasNoLines";
    public const string InvoiceAlreadyExistsForOrder = "Invoicing.InvoiceAlreadyExistsForOrder";
    public const string InvoiceReferenceRequired = "Invoicing.InvoiceReferenceRequired";
    public const string InvalidInvoiceAmount = "Invoicing.InvalidInvoiceAmount";
    public const string InvalidTaxAmount = "Invoicing.InvalidTaxAmount";
    public const string TaxRateNotFound = "Invoicing.TaxRateNotFound";
    public const string TaxGroupNotFound = "Invoicing.TaxGroupNotFound";
    public const string TaxRateAlreadyInGroup = "Invoicing.TaxRateAlreadyInGroup";
    public const string CreditNoteNotFound = "Invoicing.CreditNoteNotFound";
    public const string CreditNoteExceedsInvoice = "Invoicing.CreditNoteExceedsInvoice";
    public const string CreditNoteNotEditable = "Invoicing.CreditNoteNotEditable";
    public const string CreditNoteAlreadyIssued = "Invoicing.CreditNoteAlreadyIssued";
    public const string CreditNoteAlreadyCancelled = "Invoicing.CreditNoteAlreadyCancelled";
    public const string CreditNoteHasNoLines = "Invoicing.CreditNoteHasNoLines";
    public const string DebitNoteNotFound = "Invoicing.DebitNoteNotFound";
    public const string DebitNoteNotEditable = "Invoicing.DebitNoteNotEditable";
    public const string DebitNoteAlreadyIssued = "Invoicing.DebitNoteAlreadyIssued";
    public const string DebitNoteAlreadyCancelled = "Invoicing.DebitNoteAlreadyCancelled";
    public const string DebitNoteHasNoLines = "Invoicing.DebitNoteHasNoLines";
    public const string CancellationReasonRequired = "Invoicing.CancellationReasonRequired";
    public const string FiscalValidationFailed = "Invoicing.FiscalValidationFailed";

    // Delivery Module
    public const string DeliveryOrderNotFound = "Delivery.OrderNotFound";
    public const string DeliveryDriverNotFound = "Delivery.DriverNotFound";
    public const string DeliveryZoneNotFound = "Delivery.ZoneNotFound";
    public const string DeliveryAddressRequired = "Delivery.AddressRequired";
    public const string DeliveryAddressOutOfZone = "Delivery.AddressOutOfZone";
    public const string DeliveryInvalidStatusTransition = "Delivery.InvalidStatusTransition";
    public const string DeliveryCannotBeCancelled = "Delivery.CannotBeCancelled";
    public const string DeliveryDriverNotAvailable = "Delivery.DriverNotAvailable";
    public const string DeliveryDriverMismatch = "Delivery.DriverMismatch";
    public const string DeliveryOrderTypeRequired = "Delivery.OrderTypeRequired";
    public const string DeliveryAlreadyExistsForOrder = "Delivery.AlreadyExistsForOrder";
    public const string DeliveryNotReadyForPickup = "Delivery.NotReadyForPickup";

    // Finance / Accounting Module
    public const string AccountNotFound = "Finance.AccountNotFound";
    public const string AccountNumberRequired = "Finance.AccountNumberRequired";
    public const string AccountNumberDuplicate = "Finance.AccountNumberDuplicate";
    public const string AccountInactive = "Finance.AccountInactive";
    public const string AccountPostingNotAllowed = "Finance.AccountPostingNotAllowed";
    public const string JournalNotFound = "Finance.JournalNotFound";
    public const string JournalNumberRequired = "Finance.JournalNumberRequired";
    public const string JournalNotDraft = "Finance.JournalNotDraft";
    public const string JournalNotPosted = "Finance.JournalNotPosted";
    public const string JournalNotEditable = "Finance.JournalNotEditable";
    public const string JournalNotBalanced = "Finance.JournalNotBalanced";
    public const string JournalHasNoLines = "Finance.JournalHasNoLines";
    public const string JournalLineBothSides = "Finance.JournalLineBothSides";
    public const string InvalidJournalAmount = "Finance.InvalidJournalAmount";
    public const string JournalAlreadyPosted = "Finance.JournalAlreadyPosted";
    public const string FiscalPeriodNotFound = "Finance.FiscalPeriodNotFound";
    public const string FiscalPeriodClosed = "Finance.FiscalPeriodClosed";
    public const string FiscalPeriodNotOpen = "Finance.FiscalPeriodNotOpen";
    public const string FiscalPeriodAlreadyLocked = "Finance.FiscalPeriodAlreadyLocked";
    public const string FiscalPeriodLocked = "Finance.FiscalPeriodLocked";
    public const string InvalidFiscalPeriodDates = "Finance.InvalidFiscalPeriodDates";
    public const string CostCenterNotFound = "Finance.CostCenterNotFound";
    public const string PostingAlreadyExists = "Finance.PostingAlreadyExists";
    public const string StandardAccountsNotConfigured = "Finance.StandardAccountsNotConfigured";
}
