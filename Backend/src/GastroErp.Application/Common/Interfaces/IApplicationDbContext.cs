using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GastroErp.Domain.Entities.Organization;
using GastroErp.Domain.Entities.Menu;
using GastroErp.Domain.Entities.Inventory.Catalog;
using GastroErp.Domain.Entities.Inventory.Suppliers;
using GastroErp.Domain.Entities.Inventory.Warehouse;
using GastroErp.Domain.Entities.Inventory.Transactions;
using GastroErp.Domain.Entities.Inventory.Settings;
using GastroErp.Domain.Entities.Inventory.Purchasing;
using GastroErp.Domain.Entities.Inventory.Counting;
using GastroErp.Domain.Entities.Inventory.Waste;
using GastroErp.Domain.Entities.Inventory.Recipe;
using GastroErp.Domain.Entities.Inventory.Reservation;
using GastroErp.Domain.Entities.Identity;
using GastroErp.Domain.Entities.Sales;
using GastroErp.Domain.Entities.Invoicing;
using GastroErp.Domain.Entities.Delivery;
using GastroErp.Domain.Entities.Crm;
using GastroErp.Domain.Entities.Finance;
using GastroErp.Domain.Entities.Automation;
using GastroErp.Domain.Entities.Ai;
using GastroErp.Domain.Entities.HR;
using GastroErp.Domain.Entities.Workflow;
using GastroErp.Domain.Entities.Reporting;

namespace GastroErp.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    // ─── Organization ─────────────────────────────────────────────────────────
    DbSet<Tenant> Tenants { get; }
    DbSet<Company> Companies { get; }
    DbSet<Branch> Branches { get; }
    DbSet<Department> Departments { get; }
    DbSet<Device> Devices { get; }
    DbSet<BranchDevice> BranchDevices { get; }
    DbSet<OrganizationSettings> OrganizationSettings { get; }
    DbSet<Subscription> Subscriptions { get; }
    DbSet<SubscriptionPlan> SubscriptionPlans { get; }
    DbSet<Feature> Features { get; }
    DbSet<SubscriptionFeatureLimit> SubscriptionFeatureLimits { get; }

    // ─── Identity ───────────────────────────────────────────────────
    DbSet<AppUser> AppUsers { get; }
    DbSet<Role> Roles { get; }

    // ─── Menu ─────────────────────────────────────────────────────────────────
    DbSet<Category> Categories { get; }
    DbSet<global::GastroErp.Domain.Entities.Menu.Menu> Menus { get; }
    DbSet<MenuSection> MenuSections { get; }
    DbSet<MenuItem> MenuItems { get; }
    DbSet<ModifierGroup> ModifierGroups { get; }
    DbSet<Modifier> Modifiers { get; }
    DbSet<OptionGroup> OptionGroups { get; }
    DbSet<Option> Options { get; }
    DbSet<PriceLevel> PriceLevels { get; }
    DbSet<BranchMenu> BranchMenus { get; }
    DbSet<ComboMeal> ComboMeals { get; }
    DbSet<Product> Products { get; }

    // ─── Inventory – Catalog ──────────────────────────────────────────────────
    DbSet<InventoryCategory> InventoryCategories { get; }
    DbSet<InventoryItem> InventoryItems { get; }
    DbSet<InventoryUnit> InventoryUnits { get; }
    DbSet<UnitConversion> UnitConversions { get; }

    // ─── Inventory – Warehouse ────────────────────────────────────────────────
    DbSet<Warehouse> Warehouses { get; }
    DbSet<StockTransfer> StockTransfers { get; }

    // ─── Inventory – Suppliers ────────────────────────────────────────────────
    DbSet<Supplier> Suppliers { get; }

    // ─── Inventory – Purchasing ───────────────────────────────────────────────
    DbSet<PurchaseOrder> PurchaseOrders { get; }
    DbSet<GoodsReceipt> GoodsReceipts { get; }
    DbSet<PurchaseReturn> PurchaseReturns { get; }

    // ─── Inventory – Counting ─────────────────────────────────────────────────
    DbSet<StockAdjustment> StockAdjustments { get; }
    DbSet<StockCount> StockCounts { get; }
    DbSet<AdjustmentReason> AdjustmentReasons { get; }

    // ─── Inventory – Waste ────────────────────────────────────────────────────
    DbSet<WasteRecord> WasteRecords { get; }
    DbSet<WasteItem> WasteItems { get; }
    DbSet<WasteReason> WasteReasons { get; }

    // ─── Inventory – Recipe ───────────────────────────────────────────────────
    DbSet<Recipe> Recipes { get; }

    // ─── Inventory – Transactions ─────────────────────────────────────────────
    DbSet<InventoryTransaction> InventoryTransactions { get; }
    DbSet<StockMovement> StockMovements { get; }
    DbSet<InventoryBatch> InventoryBatches { get; }

    // ─── Inventory – Reservation ──────────────────────────────────────────────
    DbSet<InventoryReservation> InventoryReservations { get; }

    // ─── Inventory – Settings ─────────────────────────────────────────────────
    DbSet<InventorySetting> InventorySettings { get; }

    // ─── Sales / POS ──────────────────────────────────────────────────────────
    DbSet<SalesOrder> SalesOrders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<KitchenTicketItem> KitchenTicketItems { get; }
    DbSet<Payment> Payments { get; }
    DbSet<Refund> Refunds { get; }
    DbSet<CashRegister> CashRegisters { get; }
    DbSet<CashierShift> CashierShifts { get; }
    DbSet<KitchenStation> KitchenStations { get; }
    DbSet<KitchenTicket> KitchenTickets { get; }
    DbSet<FloorPlan> FloorPlans { get; }
    DbSet<TableReservation> TableReservations { get; }

    // ─── Invoicing ────────────────────────────────────────────────────────────
    DbSet<Invoice> Invoices { get; }
    DbSet<TaxRate> TaxRates { get; }
    DbSet<TaxGroup> TaxGroups { get; }
    DbSet<CreditNote> CreditNotes { get; }
    DbSet<DebitNote> DebitNotes { get; }

    // ─── Delivery ─────────────────────────────────────────────────────────────
    DbSet<DeliveryZone> DeliveryZones { get; }
    DbSet<DeliveryDriver> DeliveryDrivers { get; }
    DbSet<DeliveryOrder> DeliveryOrders { get; }

    // ─── CRM & Loyalty ────────────────────────────────────────────────────────────
    DbSet<Customer> Customers { get; }
    DbSet<LoyaltyAccount> LoyaltyAccounts { get; }
    DbSet<LoyaltyTransaction> LoyaltyTransactions { get; }
    DbSet<MembershipTier> MembershipTiers { get; }
    DbSet<Coupon> Coupons { get; }
    DbSet<PromotionCampaign> PromotionCampaigns { get; }
    DbSet<GiftCard> GiftCards { get; }

    // ─── Finance / Accounting ───────────────────────────────────────────────────
    DbSet<ChartOfAccount> ChartOfAccounts { get; }
    DbSet<JournalEntry> JournalEntries { get; }
    DbSet<JournalEntryLine> JournalEntryLines { get; }
    DbSet<FiscalPeriod> FiscalPeriods { get; }
    DbSet<CostCenter> CostCenters { get; }
    DbSet<AccountingTransaction> AccountingTransactions { get; }

    // ─── Automation (Jobs, Notifications, Integrations) ───────────────────────
    DbSet<NotificationMessage> NotificationMessages { get; }
    DbSet<JobExecutionLog> JobExecutionLogs { get; }
    DbSet<IntegrationConfiguration> IntegrationConfigurations { get; }
    DbSet<WebhookSubscription> WebhookSubscriptions { get; }
    DbSet<ExternalEventLog> ExternalEventLogs { get; }

    // ─── AI / Data Foundation ───────────────────────────────────────────────────
    DbSet<WarehouseSyncRun> WarehouseSyncRuns { get; }
    DbSet<SalesDailySnapshot> SalesDailySnapshots { get; }
    DbSet<InventoryDailySnapshot> InventoryDailySnapshots { get; }
    DbSet<DataQualityMetric> DataQualityMetrics { get; }
    DbSet<FeatureDefinition> FeatureDefinitions { get; }
    DbSet<FeatureStoreSnapshot> FeatureStoreSnapshots { get; }
    DbSet<FeatureLineage> FeatureLineages { get; }
    DbSet<MlDatasetDefinition> MlDatasetDefinitions { get; }
    DbSet<MlDatasetExport> MlDatasetExports { get; }
    DbSet<AiModelRegistry> AiModelRegistries { get; }
    DbSet<PredictionRun> PredictionRuns { get; }
    DbSet<RecommendationAction> RecommendationActions { get; }
    DbSet<AiGenerativeLog> AiGenerativeLogs { get; }
    DbSet<VoiceOrderDraft> VoiceOrderDrafts { get; }
    DbSet<FraudAlert> FraudAlerts { get; }
    DbSet<CustomerSegment> CustomerSegments { get; }
    DbSet<ChurnPrediction> ChurnPredictions { get; }
    DbSet<ProductRecommendation> ProductRecommendations { get; }

    DbSet<Employee> Employees { get; }
    DbSet<EmployeeContract> EmployeeContracts { get; }
    DbSet<EmployeeEmergencyContact> EmployeeEmergencyContacts { get; }
    DbSet<EmployeeDocument> EmployeeDocuments { get; }
    DbSet<EmploymentHistoryEntry> EmploymentHistoryEntries { get; }
    DbSet<AttendanceRecord> AttendanceRecords { get; }
    DbSet<LeaveBalance> LeaveBalances { get; }
    DbSet<LeaveRequest> LeaveRequests { get; }
    DbSet<HrWorkflowRequest> HrWorkflowRequests { get; }
    DbSet<WorkScheduleEntry> WorkScheduleEntries { get; }
    DbSet<SalaryStructure> SalaryStructures { get; }
    DbSet<PayrollRun> PayrollRuns { get; }
    DbSet<PayrollPayslip> PayrollPayslips { get; }
    DbSet<PerformanceRecord> PerformanceRecords { get; }
    DbSet<JobApplicant> JobApplicants { get; }
    DbSet<InterviewRecord> InterviewRecords { get; }
    DbSet<TrainingCourse> TrainingCourses { get; }
    DbSet<EmployeeTrainingRecord> EmployeeTrainingRecords { get; }
    DbSet<WorkingShift> WorkingShifts { get; }
    DbSet<EmployeePosition> EmployeePositions { get; }
    DbSet<Holiday> Holidays { get; }

    DbSet<WorkflowDefinition> WorkflowDefinitions { get; }
    DbSet<WorkflowStep> WorkflowSteps { get; }
    DbSet<WorkflowCondition> WorkflowConditions { get; }
    DbSet<WorkflowInstance> WorkflowInstances { get; }
    DbSet<WorkflowApproval> WorkflowApprovals { get; }
    DbSet<WorkflowHistory> WorkflowHistories { get; }
    DbSet<ApprovalDelegate> ApprovalDelegates { get; }
    DbSet<ApprovalEscalation> ApprovalEscalations { get; }

    DbSet<Dashboard> ReportingDashboards { get; }
    DbSet<DashboardWidget> ReportingDashboardWidgets { get; }
    DbSet<ReportDefinition> ReportDefinitions { get; }
    DbSet<ReportExecution> ReportExecutions { get; }
    DbSet<ScheduledReport> ScheduledReports { get; }
    DbSet<KpiDefinition> KpiDefinitions { get; }
    DbSet<KpiSnapshot> KpiSnapshots { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
