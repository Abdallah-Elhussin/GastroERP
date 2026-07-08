namespace GastroErp.Domain.Workflow;

/// <summary>Reference types used by WorkflowInstance across all modules.</summary>
public static class WorkflowIntegrationReferenceTypes
{
    // HR
    public const string LeaveRequest = "LeaveRequest";
    public const string HrWorkflowRequest = "HrWorkflowRequest";
    public const string PayrollRun = "PayrollRun";
    public const string PerformanceRecord = "PerformanceRecord";
    public const string JobApplicant = "JobApplicant";

    // Purchasing
    public const string PurchaseOrder = "PurchaseOrder";
    public const string PurchaseReturn = "PurchaseReturn";
    public const string Supplier = "Supplier";

    // Finance
    public const string JournalEntry = "JournalEntry";
    public const string PaymentVoucher = "PaymentVoucher";
    public const string ReceiptVoucher = "ReceiptVoucher";
    public const string Budget = "Budget";
    public const string Expense = "Expense";

    // Inventory
    public const string StockAdjustment = "StockAdjustment";
    public const string StockTransfer = "StockTransfer";
    public const string StockCount = "StockCount";
    public const string InventoryItem = "InventoryItem";

    // CRM
    public const string Customer = "Customer";
    public const string CustomerCreditLimit = "CustomerCreditLimit";
    public const string DiscountApproval = "DiscountApproval";
    public const string CrmRefund = "CrmRefund";

    // POS / Sales
    public const string Refund = "Refund";
    public const string Invoice = "Invoice";
    public const string PriceOverride = "PriceOverride";
    public const string CashDrawerAdjustment = "CashDrawerAdjustment";
}

public static class WorkflowIntegrationCodes
{
    public const string LeaveApproval = "LEAVE-APPROVAL";
    public const string OvertimeApproval = "OVERTIME-APPROVAL";
    public const string LoanApproval = "LOAN-APPROVAL";
    public const string SalaryAdvanceApproval = "SALARY-ADVANCE-APPROVAL";
    public const string ResignationApproval = "RESIGNATION-APPROVAL";
    public const string PromotionApproval = "PROMOTION-APPROVAL";
    public const string TransferApproval = "TRANSFER-APPROVAL";
    public const string PayrollApproval = "PAYROLL-APPROVAL";
    public const string PerformanceApproval = "PERFORMANCE-APPROVAL";
    public const string RecruitmentApproval = "RECRUITMENT-APPROVAL";

    public const string PurchaseOrderApproval = "PO-APPROVAL";
    public const string PurchaseReturnApproval = "PURCHASE-RETURN-APPROVAL";
    public const string SupplierApproval = "SUPPLIER-APPROVAL";

    public const string JournalApproval = "JOURNAL-APPROVAL";
    public const string PaymentVoucherApproval = "PAYMENT-VOUCHER-APPROVAL";
    public const string ReceiptVoucherApproval = "RECEIPT-VOUCHER-APPROVAL";
    public const string BudgetApproval = "BUDGET-APPROVAL";
    public const string ExpenseApproval = "EXPENSE-APPROVAL";

    public const string StockAdjustmentApproval = "STOCK-ADJUSTMENT-APPROVAL";
    public const string StockTransferApproval = "STOCK-TRANSFER-APPROVAL";
    public const string StockCountApproval = "STOCK-COUNT-APPROVAL";
    public const string ItemCreationApproval = "ITEM-CREATION-APPROVAL";

    public const string CustomerCreditApproval = "CUSTOMER-CREDIT-APPROVAL";
    public const string CustomerRegistrationApproval = "CUSTOMER-REGISTRATION-APPROVAL";
    public const string DiscountApproval = "DISCOUNT-APPROVAL";
    public const string CrmRefundApproval = "CRM-REFUND-APPROVAL";

    public const string PosRefundApproval = "POS-REFUND-APPROVAL";
    public const string VoidInvoiceApproval = "VOID-INVOICE-APPROVAL";
    public const string PriceOverrideApproval = "PRICE-OVERRIDE-APPROVAL";
    public const string CashDrawerAdjustmentApproval = "CASH-DRAWER-ADJUSTMENT-APPROVAL";
}
