using System.Reflection;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Domain.Common;
using GastroErp.Domain.Entities.Identity;
using GastroErp.Domain.Entities.Inventory.Catalog;
using GastroErp.Domain.Entities.Inventory.Counting;
using GastroErp.Domain.Entities.Inventory.Purchasing;
using GastroErp.Domain.Entities.Inventory.Recipe;
using GastroErp.Domain.Entities.Inventory.Reservation;
using GastroErp.Domain.Entities.Inventory.Settings;
using GastroErp.Domain.Entities.Inventory.Suppliers;
using GastroErp.Domain.Entities.Inventory.Transactions;
using GastroErp.Domain.Entities.Inventory.Warehouse;
using GastroErp.Domain.Entities.Inventory.Waste;
using GastroErp.Domain.Entities.Catalog;
using GastroErp.Domain.Entities.Menu;
using GastroErp.Domain.Entities.Organization;
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
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUser _currentUser;
    private readonly IDomainEventDispatcher? _domainEventDispatcher;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUser currentUser,
        IDomainEventDispatcher? domainEventDispatcher = null) : base(options)
    {
        _currentUser = currentUser;
        _domainEventDispatcher = domainEventDispatcher;
    }

    // ─── Organization ───────────────────────────────────────────────
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<BranchDevice> BranchDevices => Set<BranchDevice>();
    public DbSet<OrganizationSettings> OrganizationSettings => Set<OrganizationSettings>();
    public DbSet<TenantCurrency> TenantCurrencies => Set<TenantCurrency>();
    public DbSet<TenantPaymentMethod> TenantPaymentMethods => Set<TenantPaymentMethod>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<Feature> Features => Set<Feature>();
    public DbSet<SubscriptionFeatureLimit> SubscriptionFeatureLimits => Set<SubscriptionFeatureLimit>();

    // ─── Identity ───────────────────────────────────────────────────
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<PermissionCategory> PermissionCategories => Set<PermissionCategory>();
    public DbSet<PermissionGroup> PermissionGroups => Set<PermissionGroup>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<RefreshTokenEntity> RefreshTokens => Set<RefreshTokenEntity>();

    // ─── Menu ─────────────────────────────────────────────────────────────────
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<GastroErp.Domain.Entities.Menu.Menu> Menus => Set<GastroErp.Domain.Entities.Menu.Menu>();
    public DbSet<MenuSection> MenuSections => Set<MenuSection>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<ModifierGroup> ModifierGroups => Set<ModifierGroup>();
    public DbSet<Modifier> Modifiers => Set<Modifier>();
    public DbSet<OptionGroup> OptionGroups => Set<OptionGroup>();
    public DbSet<Option> Options => Set<Option>();
    public DbSet<PriceLevel> PriceLevels => Set<PriceLevel>();
    public DbSet<BranchMenu> BranchMenus => Set<BranchMenu>();
    public DbSet<ComboMeal> ComboMeals => Set<ComboMeal>();
    public DbSet<Product> Products => Set<Product>();

    // ─── Product Catalog Engine ─────────────────────────────────────────────
    public DbSet<ProductCatalogDefinition> ProductCatalogDefinitions => Set<ProductCatalogDefinition>();
    public DbSet<CatalogCodeSequence> CatalogCodeSequences => Set<CatalogCodeSequence>();
    public DbSet<ProductPriceHistory> ProductPriceHistories => Set<ProductPriceHistory>();

    // ─── Inventory – Catalog ───────────────────────────────────────────────
    public DbSet<InventoryCategory> InventoryCategories => Set<InventoryCategory>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<InventoryUnit> InventoryUnits => Set<InventoryUnit>();
    public DbSet<UnitConversion> UnitConversions => Set<UnitConversion>();

    // ─── Inventory – Warehouse ────────────────────────────────────────────
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<StockTransfer> StockTransfers => Set<StockTransfer>();

    // ─── Inventory – Suppliers ────────────────────────────────────────────
    public DbSet<Supplier> Suppliers => Set<Supplier>();

    // ─── Inventory – Purchasing ───────────────────────────────────────────
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<GoodsReceipt> GoodsReceipts => Set<GoodsReceipt>();
    public DbSet<PurchaseReturn> PurchaseReturns => Set<PurchaseReturn>();

    // ─── Inventory – Counting ─────────────────────────────────────────────
    public DbSet<StockAdjustment> StockAdjustments => Set<StockAdjustment>();
    public DbSet<StockCount> StockCounts => Set<StockCount>();
    public DbSet<AdjustmentReason> AdjustmentReasons => Set<AdjustmentReason>();

    // ─── Inventory – Waste ────────────────────────────────────────────────
    public DbSet<WasteRecord> WasteRecords => Set<WasteRecord>();
    public DbSet<WasteItem> WasteItems => Set<WasteItem>();
    public DbSet<WasteReason> WasteReasons => Set<WasteReason>();

    // ─── Inventory – Recipe ───────────────────────────────────────────────
    public DbSet<Recipe> Recipes => Set<Recipe>();

    // ─── Inventory – Transactions ─────────────────────────────────────────
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<InventoryBatch> InventoryBatches => Set<InventoryBatch>();

    // ─── Inventory – Reservation ──────────────────────────────────────────
    public DbSet<InventoryReservation> InventoryReservations => Set<InventoryReservation>();

    // ─── Inventory – Settings ─────────────────────────────────────────────
    public DbSet<InventorySetting> InventorySettings => Set<InventorySetting>();

    // ─── Sales / POS ──────────────────────────────────────────────────────
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<KitchenTicketItem> KitchenTicketItems => Set<KitchenTicketItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Refund> Refunds => Set<Refund>();
    public DbSet<CashRegister> CashRegisters => Set<CashRegister>();
    public DbSet<CashierShift> CashierShifts => Set<CashierShift>();
    public DbSet<KitchenStation> KitchenStations => Set<KitchenStation>();
    public DbSet<KitchenTicket> KitchenTickets => Set<KitchenTicket>();
    public DbSet<FloorPlan> FloorPlans => Set<FloorPlan>();
    public DbSet<TableReservation> TableReservations => Set<TableReservation>();

    // ─── Invoicing ──────────────────────────────────────────────────────────
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<TaxRate> TaxRates => Set<TaxRate>();
    public DbSet<TaxGroup> TaxGroups => Set<TaxGroup>();
    public DbSet<CreditNote> CreditNotes => Set<CreditNote>();
    public DbSet<DebitNote> DebitNotes => Set<DebitNote>();

    // ─── Delivery ─────────────────────────────────────────────────────────────
    public DbSet<DeliveryZone> DeliveryZones => Set<DeliveryZone>();
    public DbSet<DeliveryDriver> DeliveryDrivers => Set<DeliveryDriver>();
    public DbSet<DeliveryOrder> DeliveryOrders => Set<DeliveryOrder>();

    // ─── CRM & Loyalty ─────────────────────────────────────────────────────────
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<LoyaltyAccount> LoyaltyAccounts => Set<LoyaltyAccount>();
    public DbSet<LoyaltyTransaction> LoyaltyTransactions => Set<LoyaltyTransaction>();
    public DbSet<MembershipTier> MembershipTiers => Set<MembershipTier>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<PromotionCampaign> PromotionCampaigns => Set<PromotionCampaign>();
    public DbSet<GiftCard> GiftCards => Set<GiftCard>();

    // ─── Finance / Accounting ─────────────────────────────────────────────────
    public DbSet<ChartOfAccount> ChartOfAccounts => Set<ChartOfAccount>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalEntryLine> JournalEntryLines => Set<JournalEntryLine>();
    public DbSet<FiscalPeriod> FiscalPeriods => Set<FiscalPeriod>();
    public DbSet<CostCenter> CostCenters => Set<CostCenter>();
    public DbSet<AccountingTransaction> AccountingTransactions => Set<AccountingTransaction>();

    // ─── Automation ───────────────────────────────────────────────────────────
    public DbSet<NotificationMessage> NotificationMessages => Set<NotificationMessage>();
    public DbSet<JobExecutionLog> JobExecutionLogs => Set<JobExecutionLog>();
    public DbSet<IntegrationConfiguration> IntegrationConfigurations => Set<IntegrationConfiguration>();
    public DbSet<WebhookSubscription> WebhookSubscriptions => Set<WebhookSubscription>();
    public DbSet<ExternalEventLog> ExternalEventLogs => Set<ExternalEventLog>();

    // ─── AI / Data Foundation ───────────────────────────────────────────────────
    public DbSet<WarehouseSyncRun> WarehouseSyncRuns => Set<WarehouseSyncRun>();
    public DbSet<SalesDailySnapshot> SalesDailySnapshots => Set<SalesDailySnapshot>();
    public DbSet<InventoryDailySnapshot> InventoryDailySnapshots => Set<InventoryDailySnapshot>();
    public DbSet<DataQualityMetric> DataQualityMetrics => Set<DataQualityMetric>();
    public DbSet<FeatureDefinition> FeatureDefinitions => Set<FeatureDefinition>();
    public DbSet<FeatureStoreSnapshot> FeatureStoreSnapshots => Set<FeatureStoreSnapshot>();
    public DbSet<FeatureLineage> FeatureLineages => Set<FeatureLineage>();
    public DbSet<MlDatasetDefinition> MlDatasetDefinitions => Set<MlDatasetDefinition>();
    public DbSet<MlDatasetExport> MlDatasetExports => Set<MlDatasetExport>();
    public DbSet<AiModelRegistry> AiModelRegistries => Set<AiModelRegistry>();
    public DbSet<PredictionRun> PredictionRuns => Set<PredictionRun>();
    public DbSet<RecommendationAction> RecommendationActions => Set<RecommendationAction>();
    public DbSet<AiGenerativeLog> AiGenerativeLogs => Set<AiGenerativeLog>();
    public DbSet<VoiceOrderDraft> VoiceOrderDrafts => Set<VoiceOrderDraft>();
    public DbSet<FraudAlert> FraudAlerts => Set<FraudAlert>();
    public DbSet<CustomerSegment> CustomerSegments => Set<CustomerSegment>();
    public DbSet<ChurnPrediction> ChurnPredictions => Set<ChurnPrediction>();
    public DbSet<ProductRecommendation> ProductRecommendations => Set<ProductRecommendation>();

    // ─── HR / Workforce ─────────────────────────────────────────────────────────
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<EmployeeContract> EmployeeContracts => Set<EmployeeContract>();
    public DbSet<EmployeeEmergencyContact> EmployeeEmergencyContacts => Set<EmployeeEmergencyContact>();
    public DbSet<EmployeeDocument> EmployeeDocuments => Set<EmployeeDocument>();
    public DbSet<EmploymentHistoryEntry> EmploymentHistoryEntries => Set<EmploymentHistoryEntry>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<HrWorkflowRequest> HrWorkflowRequests => Set<HrWorkflowRequest>();
    public DbSet<WorkScheduleEntry> WorkScheduleEntries => Set<WorkScheduleEntry>();
    public DbSet<SalaryStructure> SalaryStructures => Set<SalaryStructure>();
    public DbSet<PayrollRun> PayrollRuns => Set<PayrollRun>();
    public DbSet<PayrollPayslip> PayrollPayslips => Set<PayrollPayslip>();
    public DbSet<PerformanceRecord> PerformanceRecords => Set<PerformanceRecord>();
    public DbSet<JobApplicant> JobApplicants => Set<JobApplicant>();
    public DbSet<InterviewRecord> InterviewRecords => Set<InterviewRecord>();
    public DbSet<TrainingCourse> TrainingCourses => Set<TrainingCourse>();
    public DbSet<EmployeeTrainingRecord> EmployeeTrainingRecords => Set<EmployeeTrainingRecord>();
    public DbSet<WorkingShift> WorkingShifts => Set<WorkingShift>();
    public DbSet<EmployeePosition> EmployeePositions => Set<EmployeePosition>();
    public DbSet<Holiday> Holidays => Set<Holiday>();

    // ─── Workflow Engine ──────────────────────────────────────────────────────
    public DbSet<WorkflowDefinition> WorkflowDefinitions => Set<WorkflowDefinition>();
    public DbSet<WorkflowStep> WorkflowSteps => Set<WorkflowStep>();
    public DbSet<WorkflowCondition> WorkflowConditions => Set<WorkflowCondition>();
    public DbSet<WorkflowInstance> WorkflowInstances => Set<WorkflowInstance>();
    public DbSet<WorkflowApproval> WorkflowApprovals => Set<WorkflowApproval>();
    public DbSet<WorkflowHistory> WorkflowHistories => Set<WorkflowHistory>();
    public DbSet<ApprovalDelegate> ApprovalDelegates => Set<ApprovalDelegate>();
    public DbSet<ApprovalEscalation> ApprovalEscalations => Set<ApprovalEscalation>();

    public DbSet<Dashboard> ReportingDashboards => Set<Dashboard>();
    public DbSet<DashboardWidget> ReportingDashboardWidgets => Set<DashboardWidget>();
    public DbSet<ReportDefinition> ReportDefinitions => Set<ReportDefinition>();
    public DbSet<ReportExecution> ReportExecutions => Set<ReportExecution>();
    public DbSet<ScheduledReport> ScheduledReports => Set<ScheduledReport>();
    public DbSet<KpiDefinition> KpiDefinitions => Set<KpiDefinition>();
    public DbSet<KpiSnapshot> KpiSnapshots => Set<KpiSnapshot>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(builder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<AuditableBaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.SetCreated(_currentUser.Id?.ToString());
                    break;

                case EntityState.Modified:
                    entry.Entity.SetUpdated(_currentUser.Id?.ToString());
                    break;
            }
        }

        var domainEvents = ChangeTracker.Entries<BaseEntity>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            entry.Entity.ClearDomainEvents();

        var result = await base.SaveChangesAsync(cancellationToken);

        if (_domainEventDispatcher is not null && domainEvents.Count > 0)
            await _domainEventDispatcher.DispatchEventsAsync(domainEvents, cancellationToken);

        return result;
    }
}
