using GastroErp.Domain.Entities.Reporting;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.UnitTests.Reporting;

public class DashboardEntityTests
{
    [Fact]
    public void Create_raises_dashboard_created_event()
    {
        var dashboard = Dashboard.Create(Guid.NewGuid(), "Executive", Guid.NewGuid());
        Assert.Equal("Executive", dashboard.Name);
        Assert.Contains(dashboard.DomainEvents, e => e.GetType().Name == "DashboardCreatedEvent");
    }

    [Fact]
    public void AddWidget_increases_widget_count()
    {
        var dashboard = Dashboard.Create(Guid.NewGuid(), "Sales", null);
        dashboard.AddWidget(WidgetType.KpiCard, "Revenue", 0, 4, 2);
        Assert.Single(dashboard.Widgets);
    }
}

public class ReportDefinitionTests
{
    [Fact]
    public void Publish_sets_published_flag()
    {
        var report = ReportDefinition.Create(Guid.NewGuid(), "Daily Sales", "DAILY-SALES",
            ReportModule.Sales, ReportCategory.Operational, "daily-sales");
        report.Publish();
        Assert.True(report.IsPublished);
    }

    [Fact]
    public void Update_throws_when_published()
    {
        var report = ReportDefinition.Create(Guid.NewGuid(), "Daily Sales", "DAILY-SALES",
            ReportModule.Sales, ReportCategory.Operational, "daily-sales");
        report.Publish();
        Assert.Throws<InvalidOperationException>(() =>
            report.Update("X", ReportCategory.Operational, "daily-sales", null, null));
    }
}

public class ReportExecutionTests
{
    [Fact]
    public void Complete_sets_status_and_raises_event()
    {
        var execution = ReportExecution.Start(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        execution.Complete(120, "{}");
        Assert.Equal(ReportStatus.Completed, execution.Status);
        Assert.Contains(execution.DomainEvents, e => e.GetType().Name == "ReportGeneratedEvent");
    }
}

public class KpiDefinitionTests
{
    [Fact]
    public void Snapshot_raises_kpi_calculated_event()
    {
        var snap = KpiSnapshot.Record(Guid.NewGuid(), Guid.NewGuid(), 100m, KpiTrend.Up, DateOnly.FromDateTime(DateTime.UtcNow));
        Assert.Contains(snap.DomainEvents, e => e.GetType().Name == "KpiCalculatedEvent");
    }
}

public class ScheduledReportTests
{
    [Fact]
    public void MarkRun_raises_scheduled_event()
    {
        var scheduled = ScheduledReport.Create(Guid.NewGuid(), Guid.NewGuid(),
            ScheduleFrequency.Daily, ReportExportFormat.Pdf);
        scheduled.MarkRun(true);
        Assert.NotNull(scheduled.LastRunAt);
        Assert.Contains(scheduled.DomainEvents, e => e.GetType().Name == "ScheduledReportExecutedEvent");
    }
}
