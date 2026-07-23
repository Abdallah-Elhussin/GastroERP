namespace GastroErp.Presentation.Common;

public static class ApiRoutes
{
    public const string Root = "api/v{version:apiVersion}";

    public static class EnterpriseDashboard
    {
        public const string Base = $"{Root}/dashboard";
        public const string Overview = $"{Base}/overview";
        public const string Sales = $"{Base}/sales";
        public const string Inventory = $"{Base}/inventory";
        public const string Finance = $"{Base}/finance";
        public const string Kitchen = $"{Base}/kitchen";
        public const string Delivery = $"{Base}/delivery";
        public const string Hr = $"{Base}/hr";
        public const string Customers = $"{Base}/customers";
        public const string Products = $"{Base}/products";
        public const string Activities = $"{Base}/activities";
    }

    public static class Auth
    {
        public const string Base = $"{Root}/auth";
        public const string Login = $"{Base}/login";
        public const string Refresh = $"{Base}/refresh";
        public const string Logout = $"{Base}/logout";
        public const string Me = $"{Base}/me";
        public const string ChangePassword = $"{Base}/change-password";
        public const string ForgotPassword = $"{Base}/forgot-password";
        public const string ResetPassword = $"{Base}/reset-password";
        public const string SwitchTenant = $"{Base}/switch-tenant";
    }

    public static class Onboarding
    {
        public const string RegisterCompany = $"{Root}/onboarding/register-company";
        public const string SetupRestaurant = $"{Root}/onboarding/setup-restaurant";
    }

    public static class Organization
    {
        public const string Tenants = $"{Root}/organization/tenants";
        public const string Companies = $"{Root}/organization/companies";
        public const string Branches = $"{Root}/organization/branches";
        public const string Departments = $"{Root}/organization/departments";
        public const string Devices = $"{Root}/organization/devices";
        public const string Settings = $"{Root}/organization/settings";
        public const string Subscriptions = $"{Root}/organization/subscriptions";
    }

    public static class Menu
    {
        public const string Menus = $"{Root}/menu/menus";
        public const string Categories = $"{Root}/menu/categories";
        public const string Products = $"{Root}/menu/products";
        public const string Modifiers = $"{Root}/menu/modifiers";
        public const string Combos = $"{Root}/menu/combos";
        public const string PriceLevels = $"{Root}/menu/price-levels";
        public const string BranchMenus = $"{Root}/menu/branch-menus";
    }

    public static class Catalog
    {
        public const string Types = $"{Root}/catalog/types";
        public const string Definitions = $"{Root}/catalog/definitions";
    }

    public static class Inventory
    {
        public const string Items = $"{Root}/inventory/items";
        public const string Categories = $"{Root}/inventory/categories";
        public const string Units = $"{Root}/inventory/units";
        public const string Warehouses = $"{Root}/inventory/warehouses";
        public const string Suppliers = $"{Root}/inventory/suppliers";
        public const string Purchases = $"{Root}/inventory/purchases";
        public const string Recipes = $"{Root}/inventory/recipes";
        public const string Stock = $"{Root}/inventory/stock";
        public const string StockCounts = $"{Root}/inventory/stock-counts";
        public const string PurchaseReturns = $"{Root}/inventory/purchase-returns";
        public const string Reservations = $"{Root}/inventory/reservations";
        public const string GoodsReceipts = $"{Root}/inventory/goods-receipts";
        public const string PurchaseInvoices = $"{Root}/inventory/purchase-invoices";
        public const string GoodsIssues = $"{Root}/inventory/goods-issues";
        public const string IssueDestinations = $"{Root}/inventory/issue-destinations";
        public const string OpeningBalances = $"{Root}/inventory/opening-balances";
        public const string Transfers = $"{Root}/inventory/stock/transfers";
        public const string StockTransfers = $"{Root}/inventory/stock-transfers";
        public const string Adjustments = $"{Root}/inventory/stock/adjustments";
        public const string Waste = $"{Root}/inventory/stock/waste";
        public const string Dashboard = $"{Root}/inventory/dashboard";
        public const string PurchasingDashboard = $"{Root}/inventory/purchasing-dashboard";
        public const string Settings = $"{Root}/inventory/settings";
        public const string Brands = $"{Root}/inventory/brands";
        public const string Manufacturers = $"{Root}/inventory/manufacturers";
        public const string Attributes = $"{Root}/inventory/attributes";
        public const string PriceLists = $"{Root}/inventory/price-lists";
        public const string ItemTypes = $"{Root}/inventory/item-types";
        public const string ValuationGroups = $"{Root}/inventory/valuation-groups";
        public const string ProductInquiry = $"{Root}/inventory/product-inquiry";
    }

    public static class Sales
    {
        public const string Orders = $"{Root}/sales/orders";
        public const string Payments = $"{Root}/sales/payments";
        public const string CashRegisters = $"{Root}/sales/cash-registers";
        public const string Shifts = $"{Root}/sales/shifts";
        public const string CashMovements = $"{Root}/sales/cash-movements";
        public const string Kitchen = $"{Root}/sales/kitchen";
        public const string FloorPlans = $"{Root}/sales/floor-plans";
        public const string Tables = $"{Root}/sales/tables";
        public const string TableReservations = $"{Root}/sales/reservations";
        public const string Invoices = $"{Root}/sales/invoices";
        public const string TaxRates = $"{Root}/sales/tax-rates";
        public const string TaxGroups = $"{Root}/sales/tax-groups";
        public const string CreditNotes = $"{Root}/sales/credit-notes";
        public const string DebitNotes = $"{Root}/sales/debit-notes";
        public const string InvoicingReports = $"{Root}/sales/invoicing/reports";
        public const string Delivery = $"{Root}/sales/delivery";
        public const string DeliveryZones = $"{Root}/sales/delivery/zones";
        public const string DeliveryDrivers = $"{Root}/sales/delivery/drivers";
        public const string DeliveryOrders = $"{Root}/sales/delivery/orders";
        public const string ProductPrices = $"{Root}/sales/product-prices";
        public const string PriceLists = $"{Root}/sales/price-lists";
        public const string Dashboard = $"{Root}/sales/dashboard";
    }

    /// <summary>المبيعات الإدارية — منفصلة عن /sales (POS).</summary>
    public static class BackOfficeSales
    {
        public const string Base = $"{Root}/back-office-sales";
        public const string Invoices = $"{Base}/invoices";
        public const string Dashboard = $"{Base}/dashboard";
        public const string Orders = $"{Base}/orders";
        public const string Quotations = $"{Base}/quotations";
        public const string DeliveryNotes = $"{Base}/delivery-notes";
        public const string Returns = $"{Base}/returns";
        public const string DebitNotes = $"{Base}/debit-notes";
        public const string Reports = $"{Base}/reports";
    }

    public static class Crm
    {
        public const string Base = $"{Root}/crm";
        public const string Customers = $"{Base}/customers";
    }

    public static class Finance
    {
        public const string Accounts = $"{Root}/finance/accounts";
        public const string AccountClassifications = $"{Root}/finance/account-classifications";
        public const string AccountMainClassifications = $"{Root}/finance/account-main-classifications";
        public const string AccountingSettings = $"{Root}/finance/accounting-settings";
        public const string Journals = $"{Root}/finance/journals";
        public const string FiscalPeriods = $"{Root}/finance/fiscal-periods";
        public const string CostCenters = $"{Root}/finance/cost-centers";
        public const string Currencies = $"{Root}/finance/currencies";
        public const string ExchangeRates = $"{Root}/finance/exchange-rates";
        public const string DocumentTypes = $"{Root}/finance/document-types";
        public const string Banks = $"{Root}/finance/banks";
        public const string CashBoxes = $"{Root}/finance/cash-boxes";
        public const string TaxRegistrations = $"{Root}/finance/tax-registrations";
        public const string TaxCodes = $"{Root}/finance/tax-codes";
        public const string NotificationReasons = $"{Root}/finance/notification-reasons";
        public const string OpeningBalances = $"{Root}/finance/opening-balances";
        public const string ReceiptVouchers = $"{Root}/finance/receipt-vouchers";
        public const string FinancialNotes = $"{Root}/finance/financial-notes";
        public const string GeneralLedgerSettings = $"{Root}/finance/general-ledger-settings";
        public const string Reports = $"{Root}/finance/reports";
    }

    public static class Reports
    {
        public const string Base = $"{Root}/reports";
        public const string Dashboard = $"{Base}/dashboard";
        public const string Sales = $"{Base}/sales";
        public const string Kitchen = $"{Base}/kitchen";
        public const string Delivery = $"{Base}/delivery";
        public const string Inventory = $"{Base}/inventory";
        public const string Customers = $"{Base}/customers";
        public const string Finance = $"{Base}/finance";
        public const string Analytics = $"{Base}/analytics";
    }

    public static class Jobs
    {
        public const string Base = $"{Root}/jobs";
    }

    public static class Notifications
    {
        public const string Base = $"{Root}/notifications";
    }

    public static class Integrations
    {
        public const string Base = $"{Root}/integrations";
    }

    public static class Ai
    {
        public const string Base = $"{Root}/ai";
        public const string Warehouse = $"{Base}/warehouse";
        public const string Features = $"{Base}/features";
        public const string Datasets = $"{Base}/datasets";
        public const string DataQuality = $"{Base}/data-quality";
        public const string Forecast = $"{Base}/forecast";
        public const string Predictions = $"{Base}/predictions";
        public const string Recommendations = $"{Base}/recommendations";
        public const string Insights = $"{Base}/insights";
        public const string Chat = $"{Base}/chat";
        public const string Query = $"{Base}/query";
        public const string Voice = $"{Base}/voice";
        public const string Intelligence = $"{Base}/intelligence";
    }

    public static class Hr
    {
        public const string Base = $"{Root}/hr";
        public const string Employees = $"{Base}/employees";
        public const string Attendance = $"{Base}/attendance";
        public const string Schedules = $"{Base}/schedules";
        public const string Shifts = $"{Base}/shifts";
        public const string Positions = $"{Base}/positions";
        public const string Leave = $"{Base}/leave";
        public const string Payroll = $"{Base}/payroll";
        public const string Performance = $"{Base}/performance";
        public const string Recruitment = $"{Base}/recruitment";
        public const string Training = $"{Base}/training";
        public const string Dashboard = $"{Base}/dashboard";
        public const string SelfService = $"{Base}/self-service";
        public const string WorkflowRequests = $"{Base}/workflow-requests";
    }

    public static class Workflow
    {
        public const string Base = $"{Root}/workflow";
        public const string Definitions = $"{Base}/definitions";
        public const string Instances = $"{Base}/instances";
        public const string Delegations = $"{Base}/delegations";
    }

    public static class ReportingPlatform
    {
        public const string Base = $"{Root}/reporting";
        public const string Dashboards = $"{Base}/dashboards";
        public const string Reports = $"{Base}/definitions";
        public const string Kpis = $"{Base}/kpis";
        public const string Scheduled = $"{Base}/scheduled";
        public const string Charts = $"{Base}/charts";
        public const string PowerBi = $"{Base}/powerbi";
    }

    public static class Identity
    {
        public const string Base = $"{Root}/identity";
        public const string Users = $"{Base}/users";
        public const string Roles = $"{Base}/roles";
    }
}
