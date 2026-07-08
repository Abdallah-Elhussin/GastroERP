namespace GastroErp.Application.Features.Automation.Services;

using GastroErp.Application.Features.Ai.Services;
using GastroErp.Application.Features.Hr.Services;
using GastroErp.Application.Features.Workflow.Services;

public interface IScheduledJobCatalog
{
    Task ExecuteNamedJobAsync(Guid tenantId, string jobName, CancellationToken ct = default);
    IReadOnlyList<string> GetRegisteredJobNames();
}

public sealed class ScheduledJobCatalog : IScheduledJobCatalog
{
    private readonly ScheduledJobExecutor _executor;
    private readonly IAiDataJobExecutor _aiJobs;
    private readonly IAiIntelligenceJobExecutor _intelligenceJobs;
    private readonly IHrJobExecutor _hrJobs;
    private readonly IWorkflowJobExecutor _workflowJobs;
    private readonly Dictionary<string, Func<Guid, CancellationToken, Task>> _jobs;

    public ScheduledJobCatalog(
        ScheduledJobExecutor executor, IAiDataJobExecutor aiJobs,
        IAiIntelligenceJobExecutor intelligenceJobs, IHrJobExecutor hrJobs,
        IWorkflowJobExecutor workflowJobs)
    {
        _executor = executor;
        _aiJobs = aiJobs;
        _intelligenceJobs = intelligenceJobs;
        _hrJobs = hrJobs;
        _workflowJobs = workflowJobs;
        _jobs = new Dictionary<string, Func<Guid, CancellationToken, Task>>(StringComparer.OrdinalIgnoreCase)
        {
            ["AutoCloseFiscalPeriod"] = (t, c) => _executor.AutoCloseFiscalPeriodAsync(t, c),
            ["LowStockCheck"] = (t, c) => _executor.LowStockCheckAsync(t, c),
            ["LoyaltyPointsExpiry"] = (t, c) => _executor.LoyaltyPointsExpiryAsync(t, c),
            ["CancelExpiredOrders"] = (t, c) => _executor.CancelExpiredOrdersAsync(t, c),
            ["SyncDeliveryStatus"] = (t, c) => _executor.SyncDeliveryStatusAsync(t, c),
            ["KitchenDelayedAlert"] = (t, c) => _executor.KitchenDelayedAlertAsync(t, c),
            ["CacheCleanup"] = (t, c) => _executor.CleanupCacheAsync(t, c),
            ["AiWarehouseSync"] = (t, c) => _aiJobs.SyncWarehouseAsync(t, c),
            ["AiFeatureCompute"] = (t, c) => _aiJobs.ComputeFeaturesAsync(t, c),
            ["AiDataQualityCheck"] = (t, c) => _aiJobs.EvaluateDataQualityAsync(t, c),
            ["AiForecastRefresh"] = (t, c) => _aiJobs.RefreshForecastsAsync(t, c),
            ["AiRecommendationRefresh"] = (t, c) => _aiJobs.RefreshRecommendationsAsync(t, c),
            ["AiIntelligenceFraudAnalysis"] = (t, c) => _intelligenceJobs.RunFraudAnalysisAsync(t, c),
            ["AiIntelligenceSegmentation"] = (t, c) => _intelligenceJobs.RunSegmentationAsync(t, c),
            ["AiIntelligenceChurnPrediction"] = (t, c) => _intelligenceJobs.RunChurnPredictionAsync(t, c),
            ["AiIntelligenceProductRecommendations"] = (t, c) => _intelligenceJobs.RunProductRecommendationsAsync(t, c),
            ["HrAttendanceSummary"] = (t, c) => _hrJobs.RunAttendanceSummaryAsync(t, c),
            ["HrLeaveBalanceUpdate"] = (t, c) => _hrJobs.RefreshLeaveBalancesAsync(t, c),
            ["HrMissingAttendanceReport"] = (t, c) => _hrJobs.SyncAttendanceAnomaliesAsync(t, c),
            ["HrOvertimeSummary"] = (t, c) => _hrJobs.RunOvertimeSummaryAsync(t, c),
            ["HrPayrollGeneration"] = (t, c) => _hrJobs.RunPayrollGenerationAsync(t, c),
            ["HrPayrollPostingReminder"] = (t, c) => _hrJobs.RunPayrollPostingReminderAsync(t, c),
            ["HrPerformanceReminder"] = (t, c) => _hrJobs.RunPerformanceRemindersAsync(t, c),
            ["HrContractExpiry"] = (t, c) => _hrJobs.RunContractExpiryAlertsAsync(t, c),
            ["HrProbationExpiry"] = (t, c) => _hrJobs.RunProbationExpiryAlertsAsync(t, c),
            ["HrCertificationExpiry"] = (t, c) => _hrJobs.RunCertificationExpiryAlertsAsync(t, c),
            ["WorkflowEscalationJob"] = (t, c) => _workflowJobs.RunEscalationJobAsync(t, c),
            ["WorkflowReminderJob"] = (t, c) => _workflowJobs.RunReminderJobAsync(t, c),
            ["WorkflowCleanupJob"] = (t, c) => _workflowJobs.RunCleanupJobAsync(t, c),
            ["DelegationExpiryJob"] = (t, c) => _workflowJobs.RunDelegationExpiryJobAsync(t, c),
            ["WorkflowRetryJob"] = (t, c) => _workflowJobs.RunRetryJobAsync(t, c),
            ["WorkflowTimeoutJob"] = (t, c) => _workflowJobs.RunTimeoutJobAsync(t, c)
        };
    }

    public IReadOnlyList<string> GetRegisteredJobNames() => _jobs.Keys.ToList();

    public Task ExecuteNamedJobAsync(Guid tenantId, string jobName, CancellationToken ct = default)
    {
        if (!_jobs.TryGetValue(jobName, out var action))
            throw new InvalidOperationException($"Unknown job: {jobName}");
        return action(tenantId, ct);
    }
}
