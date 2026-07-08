using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Features.ReportingPlatform.Services;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GastroErp.Infrastructure.Health;

public sealed class ReportingHealthCheck : IHealthCheck
{
    private readonly IApplicationDbContext _context;
    private readonly IReportExecutionService _execution;
    private readonly IKpiAnalyticsEngine _kpi;
    private readonly IPlatformExportService _export;

    public ReportingHealthCheck(
        IApplicationDbContext context, IReportExecutionService execution,
        IKpiAnalyticsEngine kpi, IPlatformExportService export)
        => (_context, _execution, _kpi, _export) = (context, execution, kpi, export);

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (_execution is null || _kpi is null || _export is null)
            return HealthCheckResult.Unhealthy("Reporting platform services not available");

        var definitions = await _context.ReportDefinitions.CountAsync(cancellationToken);
        var scheduled = await _context.ScheduledReports.CountAsync(s => s.IsEnabled, cancellationToken);
        var pendingExecutions = await _context.ReportExecutions.CountAsync(
            e => e.Status == ReportStatus.Running, cancellationToken);
        var kpis = await _context.KpiDefinitions.CountAsync(k => k.IsActive, cancellationToken);

        var data = new Dictionary<string, object>
        {
            ["reportDefinitions"] = definitions,
            ["enabledSchedules"] = scheduled,
            ["runningExecutions"] = pendingExecutions,
            ["activeKpis"] = kpis
        };

        return pendingExecutions > 100
            ? HealthCheckResult.Degraded("High report execution queue", data: data)
            : HealthCheckResult.Healthy($"Reporting platform OK — {definitions} reports", data);
    }
}
