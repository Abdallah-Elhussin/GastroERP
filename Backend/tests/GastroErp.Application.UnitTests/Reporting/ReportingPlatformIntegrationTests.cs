using GastroErp.Application;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Interfaces.Logging;
using GastroErp.Application.Features.Automation.DTOs;
using GastroErp.Application.Features.Automation.Services;
using GastroErp.Application.Features.ReportingPlatform.DTOs;
using GastroErp.Application.Features.ReportingPlatform.Services;
using GastroErp.Domain.Enums;
using GastroErp.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GastroErp.Application.UnitTests.Reporting;

/// <summary>
/// Integration-style verification against LocalDB after migrations.
/// Run: dotnet test --filter "FullyQualifiedName~ReportingPlatformIntegrationTests"
/// </summary>
public class ReportingPlatformIntegrationTests
{
    private static readonly string ConnectionString =
        "Server=(localdb)\\mssqllocaldb;Database=GastroErpDb;Trusted_Connection=True;MultipleActiveResultSets=true";

    private static ServiceProvider BuildServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<IEmailSender, StubEmailSender>();
        services.AddScoped<ISmsSender, StubSmsSender>();
        services.AddScoped<IAuditLogger, StubAuditLogger>();
        services.AddPersistenceServices(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:DefaultConnection"] = ConnectionString })
            .Build());
        services.AddApplicationLayer();
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task Dashboard_crud_flow_succeeds()
    {
        await using var sp = BuildServices();
        using var scope = sp.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IDashboardManagementService>();
        var tenantId = await GetDefaultTenantIdAsync(scope.ServiceProvider);

        var created = await svc.CreateAsync(tenantId, Guid.NewGuid(), new CreateDashboardDto(
            "Verification Dashboard", "Post-Phase-29 test", false, "[{\"x\":0,\"y\":0}]",
            [new CreateDashboardWidgetDto(WidgetType.KpiCard, "Revenue", 0, 4, 2)]));

        var updated = await svc.UpdateAsync(tenantId, created.Id, new UpdateDashboardDto(
            "Verification Dashboard Updated", "Updated", true, "[{\"x\":1,\"y\":0}]"));
        await svc.SetFavoriteAsync(tenantId, created.Id, true);
        await svc.ShareAsync(tenantId, created.Id, new ShareDashboardDto(true));

        Assert.Equal("Verification Dashboard Updated", updated.Name);
        Assert.True(updated.IsPublic);
    }

    [Fact]
    public async Task Report_definition_execute_and_export_succeed()
    {
        await using var sp = BuildServices();
        using var scope = sp.CreateScope();
        var tenantId = await GetDefaultTenantIdAsync(scope.ServiceProvider);
        var defs = scope.ServiceProvider.GetRequiredService<IReportDefinitionService>();
        var exec = scope.ServiceProvider.GetRequiredService<IReportExecutionService>();
        var export = scope.ServiceProvider.GetRequiredService<IPlatformExportService>();

        var report = await defs.CreateAsync(tenantId, new CreateReportDefinitionDto(
            "Daily Sales Verify", "DAILY-SALES-VERIFY", ReportModule.Sales, ReportCategory.Operational, "daily-sales"));
        await defs.PublishAsync(tenantId, report.Id);

        var preview = await exec.PreviewAsync(tenantId, Guid.NewGuid(), new ExecuteReportDto(report.Id));
        Assert.Equal(ReportStatus.Completed, preview.Status);

        var run = await exec.ExecuteAsync(tenantId, Guid.NewGuid(), new ExecuteReportDto(report.Id));
        Assert.Equal(ReportStatus.Completed, run.Status);

        foreach (var format in new[] { ReportExportFormat.Csv, ReportExportFormat.Excel, ReportExportFormat.Pdf, ReportExportFormat.Json })
        {
            var file = await export.ExportAsync(tenantId, new PlatformExportRequestDto(report.Id, null, format));
            Assert.NotEmpty(file.Content);
            Assert.False(string.IsNullOrWhiteSpace(file.FileName));
        }
    }

    [Fact]
    public async Task Kpi_calculate_and_snapshot_succeed()
    {
        await using var sp = BuildServices();
        using var scope = sp.CreateScope();
        var tenantId = await GetDefaultTenantIdAsync(scope.ServiceProvider);
        var kpi = scope.ServiceProvider.GetRequiredService<IKpiAnalyticsEngine>();

        var def = await kpi.CreateAsync(tenantId, new CreateKpiDefinitionDto(
            "Revenue KPI", "REVENUE", "REVENUE", ReportModule.Sales, 1000, 500, 100));
        var value = await kpi.CalculateAsync(tenantId, def.Id, new GastroErp.Application.Features.Reporting.DTOs.ReportFilterDto());
        var history = await kpi.GetHistoryAsync(tenantId, def.Id);

        Assert.Equal(def.Id, value.KpiDefinitionId);
        Assert.NotEmpty(history);
    }

    [Fact]
    public async Task Scheduled_report_and_jobs_succeed()
    {
        await using var sp = BuildServices();
        using var scope = sp.CreateScope();
        var tenantId = await GetDefaultTenantIdAsync(scope.ServiceProvider);
        var defs = scope.ServiceProvider.GetRequiredService<IReportDefinitionService>();
        var scheduled = scope.ServiceProvider.GetRequiredService<IScheduledReportService>();
        var jobs = scope.ServiceProvider.GetRequiredService<IReportingPlatformJobExecutor>();
        var catalog = scope.ServiceProvider.GetRequiredService<IScheduledJobCatalog>();

        var report = await defs.CreateAsync(tenantId, new CreateReportDefinitionDto(
            "Scheduled Verify", "SCHED-VERIFY", ReportModule.Sales, ReportCategory.Operational, "daily-sales"));
        await defs.PublishAsync(tenantId, report.Id);

        var sched = await scheduled.CreateAsync(tenantId, new CreateScheduledReportDto(
            report.Id, ScheduleFrequency.Daily, ReportExportFormat.Csv));
        await scheduled.EnableAsync(tenantId, sched.Id);
        await scheduled.ExecuteNowAsync(tenantId, sched.Id, Guid.NewGuid());

        await jobs.RunKpiCalculationAsync(tenantId);
        await jobs.RunDashboardCacheRefreshAsync(tenantId);
        await jobs.RunReportCleanupAsync(tenantId);
        await catalog.ExecuteNamedJobAsync(tenantId, "ScheduledReportJob");
        await catalog.ExecuteNamedJobAsync(tenantId, "KpiCalculationJob");
        await catalog.ExecuteNamedJobAsync(tenantId, "DashboardCacheRefreshJob");
        await catalog.ExecuteNamedJobAsync(tenantId, "ReportCleanupJob");

        var history = await scope.ServiceProvider.GetRequiredService<IReportExecutionService>()
            .GetHistoryAsync(tenantId, report.Id);
        Assert.NotEmpty(history);
    }

    private static async Task<Guid> GetDefaultTenantIdAsync(IServiceProvider sp)
    {
        var ctx = sp.GetRequiredService<ApplicationDbContext>();
        var tenant = await ctx.Tenants.AsNoTracking().FirstAsync(t => t.Slug == "default");
        return tenant.Id;
    }

    private sealed class StubEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string to, string subject, string body, CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class StubSmsSender : ISmsSender
    {
        public Task SendSmsAsync(string phoneNumber, string message, CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class StubAuditLogger : IAuditLogger
    {
        public void LogCreate(string entityName, string entityId, object newValues) { }
        public void LogUpdate(string entityName, string entityId, object oldValues, object newValues) { }
        public void LogDelete(string entityName, string entityId) { }
        public void LogAction(string action, string entityName, string entityId, object details) { }
    }
}
