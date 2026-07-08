using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Features.Automation.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GastroErp.Infrastructure.BackgroundJobs;

/// <summary>Runs recurring scheduled jobs for all tenants.</summary>
public sealed class RecurringJobHostedService : BackgroundService, IRecurringJobService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RecurringJobHostedService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(1);

    public RecurringJobHostedService(IServiceScopeFactory scopeFactory, ILogger<RecurringJobHostedService> logger)
        => (_scopeFactory, _logger) = (scopeFactory, logger);

    public void RegisterRecurringJobs() { /* registered via hosted service loop */ }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunDailyJobsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Recurring job cycle failed");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task RunDailyJobsAsync(CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var catalog = scope.ServiceProvider.GetRequiredService<IScheduledJobCatalog>();

        var tenantIds = await context.Tenants.AsNoTracking().Select(t => t.Id).ToListAsync(ct);
        var hour = System.DateTime.UtcNow.Hour;

        foreach (var tenantId in tenantIds)
        {
            if (hour == 2)
            {
                await catalog.ExecuteNamedJobAsync(tenantId, "AutoCloseFiscalPeriod", ct);
                await catalog.ExecuteNamedJobAsync(tenantId, "LoyaltyPointsExpiry", ct);
                await catalog.ExecuteNamedJobAsync(tenantId, "CacheCleanup", ct);
                await catalog.ExecuteNamedJobAsync(tenantId, "AiWarehouseSync", ct);
                await catalog.ExecuteNamedJobAsync(tenantId, "AiFeatureCompute", ct);
                await catalog.ExecuteNamedJobAsync(tenantId, "AiDataQualityCheck", ct);
                await catalog.ExecuteNamedJobAsync(tenantId, "AiForecastRefresh", ct);
                await catalog.ExecuteNamedJobAsync(tenantId, "AiRecommendationRefresh", ct);
                await catalog.ExecuteNamedJobAsync(tenantId, "AiIntelligenceSegmentation", ct);
                await catalog.ExecuteNamedJobAsync(tenantId, "AiIntelligenceChurnPrediction", ct);
                await catalog.ExecuteNamedJobAsync(tenantId, "HrLeaveBalanceUpdate", ct);
                await catalog.ExecuteNamedJobAsync(tenantId, "HrAttendanceSummary", ct);
                await catalog.ExecuteNamedJobAsync(tenantId, "HrContractExpiry", ct);
                await catalog.ExecuteNamedJobAsync(tenantId, "HrCertificationExpiry", ct);
                await catalog.ExecuteNamedJobAsync(tenantId, "WorkflowEscalationJob", ct);
                await catalog.ExecuteNamedJobAsync(tenantId, "DelegationExpiryJob", ct);
                await catalog.ExecuteNamedJobAsync(tenantId, "WorkflowTimeoutJob", ct);
            }

            if (hour % 4 == 0)
            {
                await catalog.ExecuteNamedJobAsync(tenantId, "WorkflowReminderJob", ct);
                await catalog.ExecuteNamedJobAsync(tenantId, "WorkflowRetryJob", ct);
            }

            if (hour == 5 && System.DateTime.UtcNow.DayOfWeek == DayOfWeek.Sunday)
                await catalog.ExecuteNamedJobAsync(tenantId, "WorkflowCleanupJob", ct);

            if (hour == 3 && System.DateTime.UtcNow.DayOfWeek == DayOfWeek.Monday)
            {
                await catalog.ExecuteNamedJobAsync(tenantId, "HrMissingAttendanceReport", ct);
                await catalog.ExecuteNamedJobAsync(tenantId, "HrOvertimeSummary", ct);
            }

            if (hour == 4 && System.DateTime.UtcNow.Day == 1)
            {
                await catalog.ExecuteNamedJobAsync(tenantId, "HrPayrollGeneration", ct);
                await catalog.ExecuteNamedJobAsync(tenantId, "HrPayrollPostingReminder", ct);
                await catalog.ExecuteNamedJobAsync(tenantId, "HrPerformanceReminder", ct);
                await catalog.ExecuteNamedJobAsync(tenantId, "HrProbationExpiry", ct);
            }

            await catalog.ExecuteNamedJobAsync(tenantId, "AiIntelligenceFraudAnalysis", ct);

            if (hour % 2 == 0)
                await catalog.ExecuteNamedJobAsync(tenantId, "AiIntelligenceProductRecommendations", ct);

            if (hour is 6 or 12 or 18)
                await catalog.ExecuteNamedJobAsync(tenantId, "LowStockCheck", ct);

            if (hour % 6 == 0)
                await catalog.ExecuteNamedJobAsync(tenantId, "CancelExpiredOrders", ct);

            await catalog.ExecuteNamedJobAsync(tenantId, "SyncDeliveryStatus", ct);
            await catalog.ExecuteNamedJobAsync(tenantId, "KitchenDelayedAlert", ct);
        }

        _logger.LogInformation("Recurring jobs completed for {TenantCount} tenants", tenantIds.Count);
    }
}
