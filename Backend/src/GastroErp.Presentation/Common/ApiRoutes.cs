namespace GastroErp.Presentation.Common;

public static class ApiRoutes
{
    public const string Root = "api/v{version:apiVersion}";

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

    public static class Inventory
    {
        public const string Items = $"{Root}/inventory/items";
        public const string Warehouses = $"{Root}/inventory/warehouses";
        public const string Suppliers = $"{Root}/inventory/suppliers";
        public const string Purchases = $"{Root}/inventory/purchases";
        public const string Recipes = $"{Root}/inventory/recipes";
        public const string Stock = $"{Root}/inventory/stock";
        public const string StockCounts = $"{Root}/inventory/stock-counts";
        public const string PurchaseReturns = $"{Root}/inventory/purchase-returns";
        public const string Reservations = $"{Root}/inventory/reservations";
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
    }

    public static class Finance
    {
        public const string Accounts = $"{Root}/finance/accounts";
        public const string Journals = $"{Root}/finance/journals";
        public const string FiscalPeriods = $"{Root}/finance/fiscal-periods";
        public const string CostCenters = $"{Root}/finance/cost-centers";
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
