using GastroErp.Domain.Common;

namespace GastroErp.Domain.Events.Reporting;

public sealed record ReportGeneratedEvent(Guid ExecutionId, Guid TenantId, Guid ReportDefinitionId, Guid ExecutedBy) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record ScheduledReportExecutedEvent(Guid ScheduledReportId, Guid TenantId, bool Succeeded) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record DashboardCreatedEvent(Guid DashboardId, Guid TenantId, string Name) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record DashboardUpdatedEvent(Guid DashboardId, Guid TenantId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record KpiCalculatedEvent(Guid KpiDefinitionId, Guid TenantId, decimal Value, KpiTrend Trend) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
