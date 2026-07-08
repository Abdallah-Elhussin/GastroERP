using GastroErp.Application.Common.Interfaces.Logging;
using GastroErp.Application.Common.Notifications;
using GastroErp.Application.Features.Automation.DTOs;
using GastroErp.Application.Features.Automation.Services;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Workflow;
using MediatR;

namespace GastroErp.Application.Features.Workflow.EventHandlers;

public sealed class WorkflowStartedEventHandler : INotificationHandler<DomainEventNotification<WorkflowStartedEvent>>
{
    private readonly INotificationOrchestrator _notifications;
    private readonly IAuditLogger _audit;

    public WorkflowStartedEventHandler(INotificationOrchestrator notifications, IAuditLogger audit)
        => (_notifications, _audit) = (notifications, audit);

    public async Task Handle(DomainEventNotification<WorkflowStartedEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        _audit.LogAction("WorkflowStarted", "WorkflowInstance", evt.InstanceId.ToString(), evt);
        await _notifications.SendAsync(evt.TenantId, new SendNotificationDto(
            "Workflow Started", $"Workflow started for {evt.ReferenceType}.",
            NotificationType.WorkflowStarted, NotificationChannel.InApp,
            ReferenceType: "WorkflowInstance", ReferenceId: evt.InstanceId), ct);
    }
}

public sealed class ApprovalRequestedEventHandler : INotificationHandler<DomainEventNotification<ApprovalRequestedEvent>>
{
    private readonly INotificationOrchestrator _notifications;

    public ApprovalRequestedEventHandler(INotificationOrchestrator notifications) => _notifications = notifications;

    public async Task Handle(DomainEventNotification<ApprovalRequestedEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        await _notifications.SendAsync(evt.TenantId, new SendNotificationDto(
            "Approval Requested", $"Step {evt.StepOrder} requires approval.",
            NotificationType.WorkflowAssigned, NotificationChannel.InApp,
            ReferenceType: "WorkflowInstance", ReferenceId: evt.InstanceId), ct);
    }
}

public sealed class WorkflowApprovedEventHandler : INotificationHandler<DomainEventNotification<WorkflowApprovedEvent>>
{
    private readonly INotificationOrchestrator _notifications;

    public WorkflowApprovedEventHandler(INotificationOrchestrator notifications) => _notifications = notifications;

    public async Task Handle(DomainEventNotification<WorkflowApprovedEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        await _notifications.SendAsync(evt.TenantId, new SendNotificationDto(
            "Approval Approved", "Workflow step approved.",
            NotificationType.ApprovalApproved, NotificationChannel.InApp,
            ReferenceType: "WorkflowInstance", ReferenceId: evt.InstanceId), ct);
    }
}

public sealed class WorkflowRejectedEventHandler : INotificationHandler<DomainEventNotification<WorkflowRejectedEvent>>
{
    private readonly INotificationOrchestrator _notifications;

    public WorkflowRejectedEventHandler(INotificationOrchestrator notifications) => _notifications = notifications;

    public async Task Handle(DomainEventNotification<WorkflowRejectedEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        await _notifications.SendAsync(evt.TenantId, new SendNotificationDto(
            "Approval Rejected", evt.Reason,
            NotificationType.ApprovalRejected, NotificationChannel.InApp,
            ReferenceType: "WorkflowInstance", ReferenceId: evt.InstanceId), ct);
    }
}

public sealed class WorkflowCompletedEventHandler : INotificationHandler<DomainEventNotification<WorkflowCompletedEvent>>
{
    private readonly INotificationOrchestrator _notifications;

    public WorkflowCompletedEventHandler(INotificationOrchestrator notifications) => _notifications = notifications;

    public async Task Handle(DomainEventNotification<WorkflowCompletedEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        await _notifications.SendAsync(evt.TenantId, new SendNotificationDto(
            "Workflow Completed", $"Workflow completed for {evt.ReferenceType}.",
            NotificationType.WorkflowCompleted, NotificationChannel.InApp,
            ReferenceType: "WorkflowInstance", ReferenceId: evt.InstanceId), ct);
    }
}

public sealed class WorkflowEscalatedEventHandler : INotificationHandler<DomainEventNotification<WorkflowEscalatedEvent>>
{
    private readonly INotificationOrchestrator _notifications;

    public WorkflowEscalatedEventHandler(INotificationOrchestrator notifications) => _notifications = notifications;

    public async Task Handle(DomainEventNotification<WorkflowEscalatedEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        await _notifications.SendAsync(evt.TenantId, new SendNotificationDto(
            "Workflow Escalated", $"Escalated to role {evt.EscalateToRole}.",
            NotificationType.WorkflowEscalated, NotificationChannel.InApp,
            ReferenceType: "WorkflowInstance", ReferenceId: evt.InstanceId), ct);
    }
}

public sealed class DelegationAssignedEventHandler : INotificationHandler<DomainEventNotification<DelegationAssignedEvent>>
{
    private readonly INotificationOrchestrator _notifications;

    public DelegationAssignedEventHandler(INotificationOrchestrator notifications) => _notifications = notifications;

    public async Task Handle(DomainEventNotification<DelegationAssignedEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        await _notifications.SendAsync(evt.TenantId, new SendNotificationDto(
            "Delegation Assigned", "Approval delegation has been assigned.",
            NotificationType.DelegationAssigned, NotificationChannel.InApp,
            ReferenceType: "ApprovalDelegate", ReferenceId: evt.DelegateId), ct);
    }
}

public sealed class WorkflowCancelledNotificationHandler : INotificationHandler<DomainEventNotification<WorkflowCancelledEvent>>
{
    private readonly INotificationOrchestrator _notifications;

    public WorkflowCancelledNotificationHandler(INotificationOrchestrator notifications) => _notifications = notifications;

    public async Task Handle(DomainEventNotification<WorkflowCancelledEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        await _notifications.SendAsync(evt.TenantId, new SendNotificationDto(
            "Workflow Cancelled", evt.Reason ?? "Workflow was cancelled.",
            NotificationType.WorkflowCancelled, NotificationChannel.InApp,
            ReferenceType: "WorkflowInstance", ReferenceId: evt.InstanceId), ct);
    }
}

public sealed class WorkflowReturnedNotificationHandler : INotificationHandler<DomainEventNotification<WorkflowReturnedEvent>>
{
    private readonly INotificationOrchestrator _notifications;

    public WorkflowReturnedNotificationHandler(INotificationOrchestrator notifications) => _notifications = notifications;

    public async Task Handle(DomainEventNotification<WorkflowReturnedEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        await _notifications.SendAsync(evt.TenantId, new SendNotificationDto(
            "Workflow Returned", $"Returned to step {evt.ReturnedToStepOrder}.",
            NotificationType.WorkflowReturned, NotificationChannel.InApp,
            ReferenceType: "WorkflowInstance", ReferenceId: evt.InstanceId), ct);
    }
}
