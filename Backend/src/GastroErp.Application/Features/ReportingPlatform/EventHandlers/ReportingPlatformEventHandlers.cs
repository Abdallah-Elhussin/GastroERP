using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Interfaces.Logging;
using GastroErp.Application.Common.Notifications;
using GastroErp.Application.Features.Automation.DTOs;
using GastroErp.Application.Features.Automation.Services;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Reporting;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.ReportingPlatform.EventHandlers;

public sealed class ReportGeneratedEventHandler : INotificationHandler<DomainEventNotification<ReportGeneratedEvent>>
{
    private readonly INotificationOrchestrator _notifications;
    private readonly IAuditLogger _audit;

    public ReportGeneratedEventHandler(INotificationOrchestrator notifications, IAuditLogger audit)
        => (_notifications, _audit) = (notifications, audit);

    public async Task Handle(DomainEventNotification<ReportGeneratedEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        _audit.LogAction("ReportGenerated", "ReportExecution", evt.ExecutionId.ToString(), evt);
        await _notifications.SendAsync(evt.TenantId, new SendNotificationDto(
            "Report Ready", "Your report is ready to view.",
            NotificationType.ReportReady, NotificationChannel.InApp,
            UserId: evt.ExecutedBy, ReferenceType: "ReportExecution", ReferenceId: evt.ExecutionId), ct);
    }
}

public sealed class ScheduledReportExecutedEventHandler : INotificationHandler<DomainEventNotification<ScheduledReportExecutedEvent>>
{
    private readonly INotificationOrchestrator _notifications;

    public ScheduledReportExecutedEventHandler(INotificationOrchestrator notifications) => _notifications = notifications;

    public async Task Handle(DomainEventNotification<ScheduledReportExecutedEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        var type = evt.Succeeded ? NotificationType.ScheduledReportCompleted : NotificationType.ScheduledReportFailed;
        var title = evt.Succeeded ? "Scheduled Report Completed" : "Scheduled Report Failed";
        await _notifications.SendAsync(evt.TenantId, new SendNotificationDto(
            title, title, type, NotificationChannel.InApp,
            ReferenceType: "ScheduledReport", ReferenceId: evt.ScheduledReportId), ct);
    }
}

public sealed class DashboardCreatedEventHandler : INotificationHandler<DomainEventNotification<DashboardCreatedEvent>>
{
    private readonly IAuditLogger _audit;

    public DashboardCreatedEventHandler(IAuditLogger audit) => _audit = audit;

    public Task Handle(DomainEventNotification<DashboardCreatedEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        _audit.LogAction("DashboardCreated", "Dashboard", evt.DashboardId.ToString(), evt);
        return Task.CompletedTask;
    }
}

public sealed class DashboardSharedEventHandler : INotificationHandler<DomainEventNotification<DashboardUpdatedEvent>>
{
    private readonly INotificationOrchestrator _notifications;

    public DashboardSharedEventHandler(INotificationOrchestrator notifications) => _notifications = notifications;

    public async Task Handle(DomainEventNotification<DashboardUpdatedEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        await _notifications.SendAsync(evt.TenantId, new SendNotificationDto(
            "Dashboard Updated", "A dashboard was updated or shared.",
            NotificationType.DashboardShared, NotificationChannel.InApp,
            ReferenceType: "Dashboard", ReferenceId: evt.DashboardId), ct);
    }
}

public sealed class KpiCalculatedEventHandler : INotificationHandler<DomainEventNotification<KpiCalculatedEvent>>
{
    private readonly INotificationOrchestrator _notifications;
    private readonly IApplicationDbContext _context;

    public KpiCalculatedEventHandler(INotificationOrchestrator notifications, IApplicationDbContext context)
        => (_notifications, _context) = (notifications, context);

    public async Task Handle(DomainEventNotification<KpiCalculatedEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        var kpi = await _context.KpiDefinitions.AsNoTracking()
            .FirstOrDefaultAsync(k => k.Id == evt.KpiDefinitionId && k.TenantId == evt.TenantId, ct);
        if (kpi?.CriticalValue is not null && evt.Value <= kpi.CriticalValue)
        {
            await _notifications.SendAsync(evt.TenantId, new SendNotificationDto(
                "KPI Threshold Exceeded", $"KPI {kpi.Name} exceeded critical threshold.",
                NotificationType.KpiThresholdExceeded, NotificationChannel.InApp,
                ReferenceType: "KpiDefinition", ReferenceId: evt.KpiDefinitionId), ct);
        }
    }
}
