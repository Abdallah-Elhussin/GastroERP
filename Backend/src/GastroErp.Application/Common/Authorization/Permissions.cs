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
        public const string Activate = "Branch.Activate";
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
    }

    public static class Supplier
    {
        public const string View = "Supplier.View";
        public const string Create = "Supplier.Create";
        public const string Update = "Supplier.Update";
        public const string Manage = "Supplier.Manage";
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
    }

    public static class Journal
    {
        public const string View = "Journal.View";
        public const string Create = "Journal.Create";
        public const string Post = "Journal.Post";
        public const string Reverse = "Journal.Reverse";
    }

    public static class FiscalPeriod
    {
        public const string View = "FiscalPeriod.View";
        public const string Create = "FiscalPeriod.Create";
        public const string Close = "FiscalPeriod.Close";
        public const string Lock = "FiscalPeriod.Lock";
        public const string Reopen = "FiscalPeriod.Reopen";
    }

    public static class CostCenter
    {
        public const string View = "CostCenter.View";
        public const string Create = "CostCenter.Create";
        public const string Update = "CostCenter.Update";
    }

    public static class ReportsAccounting
    {
        public const string View = "Reports.Accounting.View";
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
        public const string PermissionsView = "Identity.Permissions.View";
        public const string PermissionsManage = "Identity.Permissions.Manage";
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
