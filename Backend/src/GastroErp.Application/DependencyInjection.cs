using System.Reflection;
using FluentValidation;
using GastroErp.Application.Common.Behaviors;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Messaging;
using GastroErp.Application.Features.Ai.Services;
using GastroErp.Application.Features.Automation.Services;
using GastroErp.Application.Features.Sales.Services;
using GastroErp.Application.Features.Delivery.Services;
using GastroErp.Application.Features.Finance.Services;
using GastroErp.Application.Features.Hr.Services;
using GastroErp.Application.Features.Workflow.Services;
using GastroErp.Application.Features.Platform.Services;
using GastroErp.Application.Features.ReportingPlatform.Services;
using GastroErp.Application.Features.Reporting.Services;
using GastroErp.Application.Features.Invoicing.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GastroErp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddAutoMapper(config => {
            config.AddMaps(Assembly.GetExecutingAssembly());
        });
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddScoped<IMenuPricingService, MenuPricingService>();
        services.AddScoped<IOrderNumberGenerator, OrderNumberGenerator>();
        services.AddScoped<IOrderInventoryService, OrderInventoryService>();
        services.AddScoped<IReceiptNumberGenerator, ReceiptNumberGenerator>();
        services.AddScoped<IShiftNumberGenerator, ShiftNumberGenerator>();
        services.AddScoped<IKitchenRoutingService, KitchenRoutingService>();
        services.AddScoped<ITableService, TableService>();

        services.AddScoped<IInvoiceNumberGenerator, InvoiceNumberGenerator>();
        services.AddScoped<ITaxCalculationService, TaxCalculationService>();
        services.AddScoped<IInvoiceGenerationService, InvoiceGenerationService>();
        services.AddScoped<IFiscalValidationService, FiscalValidationService>();
        services.AddScoped<IReceiptPrintingService, ReceiptPrintingService>();

        services.AddScoped<IDeliveryNumberGenerator, DeliveryNumberGenerator>();
        services.AddScoped<IDeliveryFeeCalculationService, DeliveryFeeCalculationService>();
        services.AddScoped<IDeliveryAssignmentService, DeliveryAssignmentService>();
        services.AddScoped<IDeliveryEtaService, DeliveryEtaService>();
        services.AddScoped<IDeliveryOrderSyncService, DeliveryOrderSyncService>();
        services.AddScoped<IDeliveryKitchenIntegrationService, DeliveryKitchenIntegrationService>();
        services.AddScoped<IDeliveryOrderFactory, DeliveryOrderFactory>();
        services.AddScoped<IDeliveryProviderAdapter, InternalDeliveryProviderAdapter>();
        services.AddScoped<DeliveryCodPaymentService>();

        services.AddScoped<IJournalNumberGenerator, JournalNumberGenerator>();
        services.AddScoped<IFiscalPeriodService, FiscalPeriodService>();
        services.AddScoped<IFinancialValidationService, FinancialValidationService>();
        services.AddScoped<IJournalPostingService, JournalPostingService>();
        services.AddScoped<IAutoPostingService, AutoPostingService>();
        services.AddScoped<IAccountBalanceService, AccountBalanceService>();
        services.AddScoped<ITrialBalanceService, TrialBalanceService>();

        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<ISalesAnalyticsService, SalesAnalyticsService>();
        services.AddScoped<IKitchenAnalyticsService, KitchenAnalyticsService>();
        services.AddScoped<IDeliveryAnalyticsService, DeliveryAnalyticsService>();
        services.AddScoped<IInventoryAnalyticsService, InventoryAnalyticsService>();
        services.AddScoped<ICustomerAnalyticsService, CustomerAnalyticsService>();
        services.AddScoped<IFinancialAnalyticsService, FinancialAnalyticsService>();
        services.AddScoped<IKpiEngineService, KpiEngineService>();
        services.AddScoped<IReportExportService, ReportExportService>();

        services.AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();
        services.AddScoped<IJobHistoryService, JobHistoryService>();
        services.AddScoped<IJobMonitoringService, JobMonitoringService>();
        services.AddScoped<IRetryPolicyService, RetryPolicyService>();
        services.AddScoped<INotificationTemplateService, NotificationTemplateService>();
        services.AddScoped<INotificationOrchestrator, NotificationOrchestrator>();
        services.AddScoped<INotificationInboxService, NotificationInboxService>();
        services.AddScoped<IIntegrationRegistryService, IntegrationRegistryService>();
        services.AddScoped<IInboundWebhookService, InboundWebhookService>();
        services.AddScoped<ScheduledJobExecutor>();
        services.AddScoped<IScheduledJobCatalog, ScheduledJobCatalog>();

        services.AddScoped<IDataWarehouseSyncService, DataWarehouseSyncService>();
        services.AddScoped<IDataQualityService, DataQualityService>();
        services.AddScoped<IFeatureStoreService, FeatureStoreService>();
        services.AddScoped<IFeatureComputationService, FeatureComputationService>();
        services.AddScoped<IMlDatasetBuilderService, MlDatasetBuilderService>();
        services.AddScoped<IAiDataJobExecutor, AiDataJobExecutor>();

        services.AddScoped<IDemandForecastService, DemandForecastService>();
        services.AddScoped<ISalesForecastService, SalesForecastService>();
        services.AddScoped<IInventoryForecastService, InventoryForecastService>();
        services.AddScoped<IAiForecastOrchestrator, AiForecastOrchestrator>();
        services.AddScoped<IPredictionRunService, PredictionRunService>();

        services.AddScoped<IPurchaseRecommendationService, PurchaseRecommendationService>();
        services.AddScoped<IRecipeCostOptimizationService, RecipeCostOptimizationService>();
        services.AddScoped<IStaffSchedulingAdvisorService, StaffSchedulingAdvisorService>();
        services.AddScoped<IDynamicPricingService, DynamicPricingService>();
        services.AddScoped<IRecommendationActionService, RecommendationActionService>();

        services.AddScoped<IManagementAiAssistantService, ManagementAiAssistantService>();
        services.AddScoped<IAiDashboardInsightsService, AiDashboardInsightsService>();
        services.AddScoped<INaturalLanguageQueryService, NaturalLanguageQueryService>();
        services.AddScoped<IVoiceOrderingService, VoiceOrderingService>();

        services.AddScoped<IFraudDetectionService, FraudDetectionService>();
        services.AddScoped<ICustomerSegmentationService, CustomerSegmentationService>();
        services.AddScoped<IChurnPredictionService, ChurnPredictionService>();
        services.AddScoped<IRecommendationEngineService, RecommendationEngineService>();
        services.AddScoped<IIntelligenceDashboardService, IntelligenceDashboardService>();
        services.AddScoped<IAiIntelligenceJobExecutor, AiIntelligenceJobExecutor>();

        services.AddScoped<IEmployeeNumberGenerator, EmployeeNumberGenerator>();
        services.AddScoped<IEmployeeManagementService, EmployeeManagementService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<ISchedulingService, SchedulingService>();
        services.AddScoped<ILeaveManagementService, LeaveManagementService>();
        services.AddScoped<IPayrollService, PayrollService>();
        services.AddScoped<IPerformanceManagementService, PerformanceManagementService>();
        services.AddScoped<IRecruitmentService, RecruitmentService>();
        services.AddScoped<ITrainingService, TrainingService>();
        services.AddScoped<IHrDashboardService, HrDashboardService>();
        services.AddScoped<IHrSelfService, HrSelfServiceImpl>();
        services.AddScoped<IHrJobExecutor, HrJobExecutor>();

        services.AddScoped<IWorkflowDefinitionService, WorkflowDefinitionService>();
        services.AddScoped<IWorkflowEngine, WorkflowEngine>();
        services.AddScoped<IApprovalService, ApprovalService>();
        services.AddScoped<IWorkflowHistoryService, WorkflowHistoryService>();
        services.AddScoped<IDelegateService, DelegateService>();
        services.AddScoped<IEscalationService, EscalationService>();
        services.AddScoped<IWorkflowJobExecutor, WorkflowJobExecutor>();
        services.AddScoped<IWorkflowIntegrationService, WorkflowIntegrationService>();
        services.AddScoped<IWorkflowModuleOutcomeService, WorkflowModuleOutcomeService>();
        services.AddScoped<IHrWorkflowRequestService, HrWorkflowRequestService>();

        services.AddScoped<IDashboardManagementService, DashboardManagementService>();
        services.AddScoped<IReportDefinitionService, ReportDefinitionService>();
        services.AddScoped<IReportDataResolver, ReportDataResolver>();
        services.AddScoped<IReportExecutionService, ReportExecutionService>();
        services.AddScoped<IKpiAnalyticsEngine, KpiAnalyticsEngine>();
        services.AddScoped<IPlatformExportService, PlatformExportService>();
        services.AddScoped<IChartService, ChartService>();
        services.AddScoped<IPowerBiIntegrationService, PowerBiIntegrationService>();
        services.AddScoped<IScheduledReportService, ScheduledReportService>();
        services.AddScoped<IReportingPlatformJobExecutor, ReportingPlatformJobExecutor>();

        services.AddScoped<IPlatformJobExecutor, PlatformJobExecutor>();

        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        });

        return services;
    }
}
