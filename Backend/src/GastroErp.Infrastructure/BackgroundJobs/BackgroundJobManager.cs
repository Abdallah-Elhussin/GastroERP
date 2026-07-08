using GastroErp.Application.Features.Automation.Services;
using GastroErp.Domain.Entities.Automation;
using GastroErp.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GastroErp.Infrastructure.BackgroundJobs;

public sealed class BackgroundJobManager : IBackgroundJobManager
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BackgroundJobManager> _logger;

    public BackgroundJobManager(IServiceScopeFactory scopeFactory, ILogger<BackgroundJobManager> logger)
        => (_scopeFactory, _logger) = (scopeFactory, logger);

    public Task<string> EnqueueAsync(
        Guid tenantId, string jobName, JobQueue queue,
        Func<CancellationToken, Task> work, CancellationToken ct = default)
    {
        var jobId = Guid.NewGuid().ToString("N");
        _ = RunAsync(tenantId, jobName, queue, jobId, work);
        return Task.FromResult(jobId);
    }

    public Task<string> ScheduleAsync(
        Guid tenantId, string jobName, JobQueue queue, TimeSpan delay,
        Func<CancellationToken, Task> work, CancellationToken ct = default)
    {
        var jobId = Guid.NewGuid().ToString("N");
        _ = Task.Run(async () =>
        {
            await Task.Delay(delay, ct);
            await RunAsync(tenantId, jobName, queue, jobId, work);
        }, ct);
        return Task.FromResult(jobId);
    }

    private async Task RunAsync(
        Guid tenantId, string jobName, JobQueue queue, string externalJobId, Func<CancellationToken, Task> work)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<GastroErp.Application.Common.Interfaces.IApplicationDbContext>();
        var log = JobExecutionLog.Create(tenantId, jobName, queue, externalJobId: externalJobId);
        context.JobExecutionLogs.Add(log);
        await context.SaveChangesAsync();

        try
        {
            log.Start();
            context.JobExecutionLogs.Update(log);
            await context.SaveChangesAsync();
            await work(CancellationToken.None);
            log.Succeed();
        }
        catch (Exception ex)
        {
            log.Fail(ex.Message);
            _logger.LogError(ex, "Background job {JobName} failed for tenant {TenantId}", jobName, tenantId);
        }

        context.JobExecutionLogs.Update(log);
        await context.SaveChangesAsync();
    }
}
