namespace GastroErp.Application.Common.Authorization;

public static class Permissions
{
    public static class Organization
    {
        public const string View = "Organization.View";
        public const string Create = "Organization.Create";
        public const string Update = "Organization.Update";
        public const string Delete = "Organization.Delete";
    }

    public static class Tenant
    {
        public const string View = "Tenant.View";
        public const string Manage = "Tenant.Manage";
    }

    public static class Company
    {
        public const string View = "Company.View";
        public const string Create = "Company.Create";
        public const string Update = "Company.Update";
    }

    public static class Branch
    {
        public const string View = "Branch.View";
        public const string Create = "Branch.Create";
        public const string Update = "Branch.Update";
        public const string Delete = "Branch.Delete";
        public const string Activate = "Branch.Activate";
        public const string Export = "Branch.Export";
        public const string Print = "Branch.Print";
    }

    /// <summary>Settings UI aliases for branch coding screen (ترميز الفروع).</summary>
    public static class SettingsBranches
    {
        public const string View = "Settings.Branches.View";
        public const string Create = "Settings.Branches.Create";
        public const string Edit = "Settings.Branches.Edit";
        public const string Delete = "Settings.Branches.Delete";
        public const string Export = "Settings.Branches.Export";
        public const string Print = "Settings.Branches.Print";
    }

    public static class Department
    {
        public const string View = "Department.View";
        public const string Create = "Department.Create";
        public const string Update = "Department.Update";
    }

    public static class Device
    {
        public const string View = "Device.View";
        public const string Manage = "Device.Manage";
        public const string Activate = "Device.Activate";
    }

    public static class Menu
    {
        public const string View = "Menu.View";
        public const string Create = "Menu.Create";
        public const string Update = "Menu.Update";
        public const string Publish = "Menu.Publish";
        public const string Archive = "Menu.Archive";
    }

    public static class Category
    {
        public const string View = "Category.View";
        public const string Create = "Category.Create";
        public const string Update = "Category.Update";
    }

    public static class Product
    {
        public const string View = "Product.View";
        public const string Create = "Product.Create";
        public const string Update = "Product.Update";
        public const string Activate = "Product.Activate";
    }

    public static class Modifier
    {
        public const string View = "Modifier.View";
        public const string Create = "Modifier.Create";
        public const string Update = "Modifier.Update";
    }

    public static class Combo
    {
        public const string View = "Combo.View";
        public const string Create = "Combo.Create";
        public const string Update = "Combo.Update";
    }

    public static class Inventory
    {
        public const string View = "Inventory.View";
        public const string Manage = "Inventory.Manage";

        public static class ItemTypes
        {
            public const string View = "Inventory.ItemTypes.View";
            public const string Create = "Inventory.ItemTypes.Create";
            public const string Edit = "Inventory.ItemTypes.Edit";
            public const string Delete = "Inventory.ItemTypes.Delete";
            public const string Export = "Inventory.ItemTypes.Export";
        }

        public static class ValuationGroups
        {
            public const string View = "Inventory.ValuationGroups.View";
            public const string Create = "Inventory.ValuationGroups.Create";
            public const string Edit = "Inventory.ValuationGroups.Edit";
            public const string Delete = "Inventory.ValuationGroups.Delete";
            public const string Export = "Inventory.ValuationGroups.Export";
        }

        public static class Settings
        {
            public const string View = "Inventory.Settings.View";
            public const string Edit = "Inventory.Settings.Edit";
            public const string Reset = "Inventory.Settings.Reset";
        }

        public static class ProductInquiry
        {
            public const string View = "Inventory.ProductInquiry.View";
            public const string Export = "Inventory.ProductInquiry.Export";
            public const string ViewCost = "Inventory.ProductInquiry.ViewCost";
            public const string ViewPrices = "Inventory.ProductInquiry.ViewPrices";
            public const string ViewMovements = "Inventory.ProductInquiry.ViewMovements";
            public const string ViewSuppliers = "Inventory.ProductInquiry.ViewSuppliers";
        }

        public static class IssueDestinations
        {
            public const string View = "Inventory.IssueDestinations.View";
            public const string Create = "Inventory.IssueDestinations.Create";
            public const string Edit = "Inventory.IssueDestinations.Edit";
            public const string Delete = "Inventory.IssueDestinations.Delete";
            public const string Export = "Inventory.IssueDestinations.Export";
        }
    }

    public static class Catalog
    {
        public const string View = "Catalog.View";
        public const string Create = "Catalog.Create";
        public const string Update = "Catalog.Update";
        public const string Delete = "Catalog.Delete";
        public const string Approve = "Catalog.Approve";
        public const string Archive = "Catalog.Archive";
        public const string Export = "Catalog.Export";
        public const string Import = "Catalog.Import";
        public const string Print = "Catalog.Print";
    }

    public static class Warehouse
    {
        public const string View = "Warehouse.View";
        public const string Create = "Warehouse.Create";
        public const string Update = "Warehouse.Update";
        public const string Activate = "Warehouse.Activate";
        public const string Delete = "Warehouse.Delete";
        public const string Export = "Warehouse.Export";
    }

    public static class InventoryWarehouses
    {
        public const string View = "Inventory.Warehouses.View";
        public const string Create = "Inventory.Warehouses.Create";
        public const string Edit = "Inventory.Warehouses.Edit";
        public const string Delete = "Inventory.Warehouses.Delete";
        public const string Export = "Inventory.Warehouses.Export";
    }

    public static class Supplier
    {
        public const string View = "Supplier.View";
        public const string Create = "Supplier.Create";
        public const string Update = "Supplier.Update";
        public const string Delete = "Supplier.Delete";
        public const string Manage = "Supplier.Manage";
        public const string Activate = "Supplier.Activate";
        public const string Deactivate = "Supplier.Deactivate";
        public const string Blacklist = "Supplier.Blacklist";
        public const string ClearBlacklist = "Supplier.ClearBlacklist";
        public const string UpdateFinancial = "Supplier.UpdateFinancial";
        public const string ViewBalance = "Supplier.ViewBalance";
        public const string ViewStatement = "Supplier.ViewStatement";
        public const string ManageAttachments = "Supplier.ManageAttachments";
    }

    public static class Purchase
    {
        public const string View = "Purchase.View";
        public const string Create = "Purchase.Create";
        public const string Approve = "Purchase.Approve";
        public const string Cancel = "Purchase.Cancel";
    }

    public static class Recipe
    {
        public const string View = "Recipe.View";
        public const string Create = "Recipe.Create";
        public const string Update = "Recipe.Update";
        public const string Activate = "Recipe.Activate";
    }

    public static class Stock
    {
        public const string View = "Stock.View";
        public const string Transfer = "Stock.Transfer";
        public const string Adjust = "Stock.Adjust";
        public const string Waste = "Stock.Waste";
    }

    public static class Sales
    {
        public const string View = "Sales.View";
        public const string Create = "Sales.Create";
        public const string Update = "Sales.Update";
        public const string Cancel = "Sales.Cancel";
        public const string Complete = "Sales.Complete";
        public const string Reopen = "Sales.Reopen";
        public const string VoidItem = "Sales.VoidItem";
        public const string ViewProfit = "Sales.ViewProfit";
        public const string Export = "Sales.Export";
        public const string Dashboard = "Sales.Dashboard.View";

        public static class ProductPricing
        {
            public const string View = "Sales.ProductPricing.View";
            public const string Create = "Sales.ProductPricing.Create";
            public const string Edit = "Sales.ProductPricing.Edit";
            public const string Delete = "Sales.ProductPricing.Delete";
            public const string Export = "Sales.ProductPricing.Export";
            public const string Copy = "Sales.ProductPricing.Copy";
        }
    }

    public static class Crm
    {
        public const string View = "Crm.View";
        public const string Create = "Crm.Create";
        public const string Update = "Crm.Update";
    }

    /// <summary>صلاحيات المبيعات الإدارية — منفصلة عن POS (Sales.*).</summary>
    public static class BackOfficeSales
    {
        public const string View = "BackOfficeSales.View";
        public const string Create = "BackOfficeSales.Create";
        public const string Update = "BackOfficeSales.Update";
        public const string Delete = "BackOfficeSales.Delete";
        public const string Approve = "BackOfficeSales.Approve";
        public const string Unapprove = "BackOfficeSales.Unapprove";
        public const string Post = "BackOfficeSales.Post";
        public const string Unpost = "BackOfficeSales.Unpost";
        public const string Cancel = "BackOfficeSales.Cancel";
        public const string Print = "BackOfficeSales.Print";
        public const string Export = "BackOfficeSales.Export";
        public const string ViewDashboard = "BackOfficeSales.ViewDashboard";
        public const string ViewReports = "BackOfficeSales.ViewReports";
        public const string EditPrices = "BackOfficeSales.EditPrices";
        public const string EditDiscounts = "BackOfficeSales.EditDiscounts";
        public const string EditTaxes = "BackOfficeSales.EditTaxes";
    }

    public static class Payments
    {
        public const string View = "Payments.View";
        public const string Create = "Payments.Create";
        public const string Refund = "Payments.Refund";
        public const string Void = "Payments.Void";
        public const string Cancel = "Payments.Cancel";
    }

    public static class CashRegister
    {
        public const string View = "CashRegister.View";
        public const string Open = "CashRegister.Open";
        public const string Close = "CashRegister.Close";
    }

    public static class Shift
    {
        public const string View = "Shift.View";
        public const string Open = "Shift.Open";
        public const string Close = "Shift.Close";
        public const string Suspend = "Shift.Suspend";
        public const string Resume = "Shift.Resume";
    }

    public static class CashMovement
    {
        public const string View = "CashMovement.View";
        public const string Create = "CashMovement.Create";
        public const string Approve = "CashMovement.Approve";
    }

    public static class Kitchen
    {
        public const string View = "Kitchen.View";
        public const string Manage = "Kitchen.Manage";
    }

    public static class Table
    {
        public const string View = "Table.View";
        public const string Manage = "Table.Manage";
    }

    public static class Reservation
    {
        public const string View = "Reservation.View";
        public const string Create = "Reservation.Create";
        public const string Manage = "Reservation.Manage";
    }

    public static class FloorPlan
    {
        public const string View = "FloorPlan.View";
        public const string Manage = "FloorPlan.Manage";
    }

    public static class Invoice
    {
        public const string View = "Invoice.View";
        public const string Create = "Invoice.Create";
        public const string Finalize = "Invoice.Finalize";
        public const string Cancel = "Invoice.Cancel";
        public const string Print = "Invoice.Print";
    }

    public static class Tax
    {
        public const string View = "Tax.View";
        public const string Manage = "Tax.Manage";
    }

    public static class CreditNote
    {
        public const string View = "CreditNote.View";
        public const string Create = "CreditNote.Create";
        public const string Cancel = "CreditNote.Cancel";
    }

    public static class DebitNote
    {
        public const string View = "DebitNote.View";
        public const string Create = "DebitNote.Create";
        public const string Cancel = "DebitNote.Cancel";
    }

    public static class Delivery
    {
        public const string View = "Delivery.View";
        public const string Manage = "Delivery.Manage";
    }

    public static class DeliveryZone
    {
        public const string View = "DeliveryZone.View";
        public const string Manage = "DeliveryZone.Manage";
    }

    public static class Driver
    {
        public const string View = "Driver.View";
        public const string Manage = "Driver.Manage";
    }

    public static class Accounting
    {
        public const string View = "Accounting.View";
        public const string Create = "Accounting.Create";
        public const string Update = "Accounting.Update";
        public const string Delete = "Accounting.Delete";
        public const string ClassificationsView = "Accounting.Classifications.View";
        public const string ClassificationsCreate = "Accounting.Classifications.Create";
        public const string ClassificationsUpdate = "Accounting.Classifications.Update";
        public const string ClassificationsDelete = "Accounting.Classifications.Delete";
    }

    public static class Journal
    {
        public const string View = "Journal.View";
        public const string Create = "Journal.Create";
        public const string Edit = "Journal.Edit";
        public const string Approve = "Journal.Approve";
        public const string Post = "Journal.Post";
        public const string Reverse = "Journal.Reverse";
        public const string Delete = "Journal.Delete";
        public const string Export = "Journal.Export";
        public const string Print = "Journal.Print";
    }

    public static class OpeningBalances
    {
        public const string View = "Finance.OpeningBalances.View";
        public const string Create = "Finance.OpeningBalances.Create";
        public const string Edit = "Finance.OpeningBalances.Edit";
        public const string Delete = "Finance.OpeningBalances.Delete";
        public const string Post = "Finance.OpeningBalances.Post";
        public const string Reverse = "Finance.OpeningBalances.Reverse";
        public const string Export = "Finance.OpeningBalances.Export";
        public const string Print = "Finance.OpeningBalances.Print";
    }

    public static class ReceiptVouchers
    {
        public const string View = "Finance.ReceiptVouchers.View";
        public const string Create = "Finance.ReceiptVouchers.Create";
        public const string Edit = "Finance.ReceiptVouchers.Edit";
        public const string Delete = "Finance.ReceiptVouchers.Delete";
        public const string Approve = "Finance.ReceiptVouchers.Approve";
        public const string Post = "Finance.ReceiptVouchers.Post";
        public const string Reverse = "Finance.ReceiptVouchers.Reverse";
        public const string Cancel = "Finance.ReceiptVouchers.Cancel";
        public const string Export = "Finance.ReceiptVouchers.Export";
        public const string Print = "Finance.ReceiptVouchers.Print";
    }

    public static class FinancialNotes
    {
        public const string View = "Finance.FinancialNotes.View";
        public const string Create = "Finance.FinancialNotes.Create";
        public const string Edit = "Finance.FinancialNotes.Edit";
        public const string Delete = "Finance.FinancialNotes.Delete";
        public const string Approve = "Finance.FinancialNotes.Approve";
        public const string Post = "Finance.FinancialNotes.Post";
        public const string Reverse = "Finance.FinancialNotes.Reverse";
        public const string Cancel = "Finance.FinancialNotes.Cancel";
        public const string Export = "Finance.FinancialNotes.Export";
        public const string Print = "Finance.FinancialNotes.Print";
    }

    public static class FiscalPeriod
    {
        public const string View = "FiscalPeriod.View";
        public const string Create = "FiscalPeriod.Create";
        public const string Edit = "FiscalPeriod.Edit";
        public const string Delete = "FiscalPeriod.Delete";
        public const string Close = "FiscalPeriod.Close";
        public const string Lock = "FiscalPeriod.Lock";
        public const string Reopen = "FiscalPeriod.Reopen";
        public const string Export = "FiscalPeriod.Export";
    }

    public static class CostCenter
    {
        public const string View = "CostCenter.View";
        public const string Create = "CostCenter.Create";
        public const string Update = "CostCenter.Update";
        public const string Delete = "CostCenter.Delete";
        public const string Activate = "CostCenter.Activate";
        public const string Export = "CostCenter.Export";
        public const string Import = "CostCenter.Import";
    }

    public static class Currency
    {
        public const string View = "Currency.View";
        public const string Create = "Currency.Create";
        public const string Update = "Currency.Update";
        public const string Delete = "Currency.Delete";
        public const string Activate = "Currency.Activate";
        public const string SetCompany = "Currency.SetCompany";
        public const string ManageRates = "Currency.ManageRates";
        public const string Export = "Currency.Export";
        public const string Import = "Currency.Import";
    }

    public static class DocumentType
    {
        public const string View = "DocumentType.View";
        public const string Create = "DocumentType.Create";
        public const string Update = "DocumentType.Update";
        public const string Delete = "DocumentType.Delete";
        public const string Activate = "DocumentType.Activate";
        public const string Export = "DocumentType.Export";
    }

    public static class Bank
    {
        public const string View = "Bank.View";
        public const string Create = "Bank.Create";
        public const string Update = "Bank.Update";
        public const string Delete = "Bank.Delete";
        public const string Activate = "Bank.Activate";
        public const string Export = "Bank.Export";
    }

    /// <summary>Finance cash boxes (تعريف الصناديق) — distinct from Sales CashRegister.</summary>
    public static class CashBox
    {
        public const string View = "CashBox.View";
        public const string Create = "CashBox.Create";
        public const string Update = "CashBox.Update";
        public const string Delete = "CashBox.Delete";
        public const string Activate = "CashBox.Activate";
        public const string Open = "CashBox.Open";
        public const string Close = "CashBox.Close";
        public const string Receive = "CashBox.Receive";
        public const string Pay = "CashBox.Pay";
        public const string Transfer = "CashBox.Transfer";
        public const string Count = "CashBox.Count";
        public const string ApproveCount = "CashBox.ApproveCount";
        public const string ViewMovements = "CashBox.ViewMovements";
        public const string PrintReport = "CashBox.PrintReport";
        public const string Export = "CashBox.Export";
    }

    public static class TaxRegistration
    {
        public const string View = "TaxRegistration.View";
        public const string Create = "TaxRegistration.Create";
        public const string Update = "TaxRegistration.Update";
        public const string Delete = "TaxRegistration.Delete";
        public const string UploadCertificate = "TaxRegistration.UploadCertificate";
        public const string DownloadCertificate = "TaxRegistration.DownloadCertificate";
        public const string Print = "TaxRegistration.Print";
        public const string Export = "TaxRegistration.Export";
    }

    /// <summary>Tax code management (تعريف وترميز الضرائب).</summary>
    public static class TaxCode
    {
        public const string View = "Settings.TaxCodes.View";
        public const string Create = "Settings.TaxCodes.Create";
        public const string Edit = "Settings.TaxCodes.Edit";
        public const string Delete = "Settings.TaxCodes.Delete";
        public const string Export = "Settings.TaxCodes.Export";
        public const string Print = "Settings.TaxCodes.Print";
    }

    /// <summary>Accounting notification reason codes (ترميزات أسباب الإشعارات).</summary>
    public static class NotificationReason
    {
        public const string View = "Settings.NotificationReasons.View";
        public const string Create = "Settings.NotificationReasons.Create";
        public const string Edit = "Settings.NotificationReasons.Edit";
        public const string Delete = "Settings.NotificationReasons.Delete";
        public const string Export = "Settings.NotificationReasons.Export";
        public const string Print = "Settings.NotificationReasons.Print";
    }

    public static class GeneralLedgerSettings
    {
        public const string View = "Finance.GeneralLedgerSettings.View";
        public const string Create = "Finance.GeneralLedgerSettings.Create";
        public const string Edit = "Finance.GeneralLedgerSettings.Edit";
        public const string Delete = "Finance.GeneralLedgerSettings.Delete";
        public const string Export = "Finance.GeneralLedgerSettings.Export";
        public const string Print = "Finance.GeneralLedgerSettings.Print";
    }

    public static class ReportsAccounting
    {
        public const string View = "Reports.Accounting.View";
    }

    /// <summary>General Ledger inquiry (read-only operational screen).</summary>
    public static class GeneralLedger
    {
        public const string View = "Finance.GeneralLedger.View";
        public const string Export = "Finance.GeneralLedger.Export";
        public const string Print = "Finance.GeneralLedger.Print";
        public const string ViewAllBranches = "Finance.GeneralLedger.ViewAllBranches";
    }

    public static class Reports
    {
        public const string View = "Reports.View";
        public const string Export = "Reports.Export";
    }

    public static class Dashboard
    {
        public const string View = "Dashboard.View";
    }

    public static class SalesReports
    {
        public const string View = "SalesReports.View";
    }

    public static class InventoryReports
    {
        public const string View = "InventoryReports.View";
    }

    public static class CustomerReports
    {
        public const string View = "CustomerReports.View";
    }

    public static class KitchenReports
    {
        public const string View = "KitchenReports.View";
    }

    public static class DeliveryReports
    {
        public const string View = "DeliveryReports.View";
    }

    public static class FinanceReports
    {
        public const string View = "FinanceReports.View";
    }

    public static class Jobs
    {
        public const string View = "Jobs.View";
        public const string Execute = "Jobs.Execute";
        public const string Manage = "Jobs.Manage";
    }

    public static class Notifications
    {
        public const string View = "Notifications.View";
        public const string Send = "Notifications.Send";
        public const string Manage = "Notifications.Manage";
    }

    public static class Integrations
    {
        public const string View = "Integrations.View";
        public const string Manage = "Integrations.Manage";
    }

    public static class System
    {
        public const string Monitor = "System.Monitor";
    }

    public static class Ai
    {
        public const string DataView = "Ai.Data.View";
        public const string DataManage = "Ai.Data.Manage";
        public const string ForecastView = "Ai.Forecast.View";
        public const string RecommendationsView = "Ai.Recommendations.View";
        public const string RecommendationsApply = "Ai.Recommendations.Apply";
        public const string ChatUse = "Ai.Chat.Use";
        public const string VoiceUse = "Ai.Voice.Use";
        public const string IntelligenceView = "Ai.Intelligence.View";
        public const string IntelligenceManage = "Ai.Intelligence.Manage";
        public const string FraudView = "Ai.Fraud.View";
        public const string SegmentsView = "Ai.Segments.View";
        public const string ChurnView = "Ai.Churn.View";
        public const string ProductRecommendationView = "Ai.Recommendation.View";
        public const string AdminManageModels = "Ai.Admin.ManageModels";
    }

    public static class Hr
    {
        public static class Employee
        {
            public const string View = "Hr.Employee.View";
            public const string Create = "Hr.Employee.Create";
            public const string Update = "Hr.Employee.Update";
            public const string Delete = "Hr.Employee.Delete";
        }

        public static class Attendance
        {
            public const string View = "Hr.Attendance.View";
            public const string Manage = "Hr.Attendance.Manage";
        }

        public static class Schedule
        {
            public const string View = "Hr.Schedule.View";
            public const string Manage = "Hr.Schedule.Manage";
        }

        public static class Leave
        {
            public const string View = "Hr.Leave.View";
            public const string Request = "Hr.Leave.Request";
            public const string Approve = "Hr.Leave.Approve";
            public const string Reject = "Hr.Leave.Reject";
        }

        public static class Payroll
        {
            public const string View = "Hr.Payroll.View";
            public const string Generate = "Hr.Payroll.Generate";
            public const string Approve = "Hr.Payroll.Approve";
            public const string Post = "Hr.Payroll.Post";
        }

        public static class Performance
        {
            public const string View = "Hr.Performance.View";
            public const string Manage = "Hr.Performance.Manage";
        }

        public static class Recruitment
        {
            public const string View = "Hr.Recruitment.View";
            public const string Manage = "Hr.Recruitment.Manage";
        }

        public static class Training
        {
            public const string View = "Hr.Training.View";
            public const string Manage = "Hr.Training.Manage";
        }

        public static class SelfService
        {
            public const string Use = "Hr.SelfService.Use";
        }

        public static class Dashboard
        {
            public const string View = "Hr.Dashboard.View";
        }
    }

    public static class Workflow
    {
        public const string View = "Workflow.View";
        public const string Create = "Workflow.Create";
        public const string Edit = "Workflow.Edit";
        public const string Delete = "Workflow.Delete";
        public const string Publish = "Workflow.Publish";
        public const string Start = "Workflow.Start";
        public const string Approve = "Workflow.Approve";
        public const string Reject = "Workflow.Reject";
        public const string Cancel = "Workflow.Cancel";
        public const string Delegate = "Workflow.Delegate";
        public const string Admin = "Workflow.Admin";
        public const string Restart = "Workflow.Restart";
        public const string Return = "Workflow.Return";
        public const string ViewTimeline = "Workflow.ViewTimeline";
    }

    public static class Reporting
    {
        public const string View = "Reporting.View";
        public const string Create = "Reporting.Create";
        public const string Edit = "Reporting.Edit";
        public const string Delete = "Reporting.Delete";
        public const string Execute = "Reporting.Execute";
        public const string Export = "Reporting.Export";
        public const string Publish = "Reporting.Publish";
        public const string Schedule = "Reporting.Schedule";
        public const string Kpi = "Reporting.KPI";
        public const string Admin = "Reporting.Admin";
    }

    public static class Identity
    {
        public const string View = "Identity.View";
        public const string Manage = "Identity.Manage";
        public const string RolesView = "Identity.Roles.View";
        public const string RolesManage = "Identity.Roles.Manage";
        public const string UsersView = "Identity.Users.View";
        public const string UsersManage = "Identity.Users.Manage";
        public const string UsersCreate = "Identity.Users.Create";
        public const string UsersEdit = "Identity.Users.Edit";
        public const string UsersDelete = "Identity.Users.Delete";
        public const string UsersResetPassword = "Identity.Users.ResetPassword";
        public const string UsersLockUnlock = "Identity.Users.LockUnlock";
        public const string UsersExport = "Identity.Users.Export";
        public const string UsersPrint = "Identity.Users.Print";
        public const string PermissionsView = "Identity.Permissions.View";
        public const string PermissionsManage = "Identity.Permissions.Manage";
    }

    /// <summary>Settings UI aliases for user coding screen (ترميز المستخدمين).</summary>
    public static class SettingsUsers
    {
        public const string View = "Settings.Users.View";
        public const string Create = "Settings.Users.Create";
        public const string Edit = "Settings.Users.Edit";
        public const string Delete = "Settings.Users.Delete";
        public const string ResetPassword = "Settings.Users.ResetPassword";
        public const string LockUnlock = "Settings.Users.LockUnlock";
        public const string Export = "Settings.Users.Export";
        public const string Print = "Settings.Users.Print";
    }

    public static class Administration
    {
        public const string View = "Administration.View";
        public const string Manage = "Administration.Manage";
    }

    public static class Settings
    {
        public const string View = "Settings.View";
        public const string Manage = "Settings.Manage";
    }
}
