using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Interfaces.Logging;
using GastroErp.Application.Common.Notifications;
using GastroErp.Application.Features.Automation.DTOs;
using GastroErp.Application.Features.Automation.Services;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Hr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Hr.EventHandlers;

public sealed class EmployeeHiredEventHandler : INotificationHandler<DomainEventNotification<EmployeeHiredEvent>>
{
    private readonly INotificationOrchestrator _notifications;
    private readonly IAuditLogger _audit;

    public EmployeeHiredEventHandler(INotificationOrchestrator notifications, IAuditLogger audit)
        => (_notifications, _audit) = (notifications, audit);

    public async Task Handle(DomainEventNotification<EmployeeHiredEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        _audit.LogAction("EmployeeHired", nameof(EmployeeHiredEvent), evt.EmployeeId.ToString(), evt);
        await _notifications.SendAsync(evt.TenantId, new SendNotificationDto(
            "Welcome", $"Employee {evt.EmployeeNumber} has joined.",
            NotificationType.HrWelcome, NotificationChannel.InApp,
            ReferenceType: "Employee", ReferenceId: evt.EmployeeId), ct);
    }
}

public sealed class EmployeeTerminatedEventHandler : INotificationHandler<DomainEventNotification<EmployeeTerminatedEvent>>
{
    private readonly IAuditLogger _audit;

    public EmployeeTerminatedEventHandler(IAuditLogger audit) => _audit = audit;

    public Task Handle(DomainEventNotification<EmployeeTerminatedEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        _audit.LogAction("EmployeeTerminated", "Employee", evt.EmployeeId.ToString(), evt);
        return Task.CompletedTask;
    }
}

public sealed class LeaveRequestedEventHandler : INotificationHandler<DomainEventNotification<LeaveRequestedEvent>>
{
    private readonly INotificationOrchestrator _notifications;
    private readonly IAuditLogger _audit;

    public LeaveRequestedEventHandler(INotificationOrchestrator notifications, IAuditLogger audit)
        => (_notifications, _audit) = (notifications, audit);

    public async Task Handle(DomainEventNotification<LeaveRequestedEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        _audit.LogAction("LeaveRequested", "LeaveRequest", evt.LeaveId.ToString(), evt);
        await _notifications.SendAsync(evt.TenantId, new SendNotificationDto(
            "Leave Requested", $"New {evt.Type} leave request submitted.",
            NotificationType.HrLeaveRequested, NotificationChannel.InApp,
            ReferenceType: "LeaveRequest", ReferenceId: evt.LeaveId), ct);
    }
}

public sealed class LeaveApprovedEventHandler : INotificationHandler<DomainEventNotification<LeaveApprovedEvent>>
{
    private readonly INotificationOrchestrator _notifications;
    private readonly IAuditLogger _audit;

    public LeaveApprovedEventHandler(INotificationOrchestrator notifications, IAuditLogger audit)
        => (_notifications, _audit) = (notifications, audit);

    public async Task Handle(DomainEventNotification<LeaveApprovedEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        _audit.LogAction("LeaveApproved", "LeaveRequest", evt.LeaveId.ToString(), evt);
        await _notifications.SendAsync(evt.TenantId, new SendNotificationDto(
            "Leave Approved", "Your leave request has been approved.",
            NotificationType.HrLeaveApproved, NotificationChannel.InApp,
            ReferenceType: "LeaveRequest", ReferenceId: evt.LeaveId), ct);
    }
}

public sealed class LeaveRejectedEventHandler : INotificationHandler<DomainEventNotification<LeaveRejectedEvent>>
{
    private readonly INotificationOrchestrator _notifications;
    private readonly IAuditLogger _audit;

    public LeaveRejectedEventHandler(INotificationOrchestrator notifications, IAuditLogger audit)
        => (_notifications, _audit) = (notifications, audit);

    public async Task Handle(DomainEventNotification<LeaveRejectedEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        _audit.LogAction("LeaveRejected", "LeaveRequest", evt.LeaveId.ToString(), evt);
        await _notifications.SendAsync(evt.TenantId, new SendNotificationDto(
            "Leave Rejected", $"Leave request rejected: {evt.Reason}",
            NotificationType.HrLeaveRejected, NotificationChannel.InApp,
            ReferenceType: "LeaveRequest", ReferenceId: evt.LeaveId), ct);
    }
}

public sealed class PayrollApprovedEventHandler : INotificationHandler<DomainEventNotification<PayrollApprovedEvent>>
{
    private readonly INotificationOrchestrator _notifications;
    private readonly IAuditLogger _audit;

    public PayrollApprovedEventHandler(INotificationOrchestrator notifications, IAuditLogger audit)
        => (_notifications, _audit) = (notifications, audit);

    public async Task Handle(DomainEventNotification<PayrollApprovedEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        _audit.LogAction("PayrollApproved", "PayrollRun", evt.RunId.ToString(), evt);
        await _notifications.SendAsync(evt.TenantId, new SendNotificationDto(
            "Payslip Ready", $"Payroll run approved — net total {evt.TotalNet:N2} SAR.",
            NotificationType.HrPayslipReady, NotificationChannel.InApp,
            ReferenceType: "PayrollRun", ReferenceId: evt.RunId), ct);
    }
}

public sealed class PayrollPostedEventHandler : INotificationHandler<DomainEventNotification<PayrollPostedEvent>>
{
    private readonly INotificationOrchestrator _notifications;
    private readonly IAuditLogger _audit;

    public PayrollPostedEventHandler(INotificationOrchestrator notifications, IAuditLogger audit)
        => (_notifications, _audit) = (notifications, audit);

    public async Task Handle(DomainEventNotification<PayrollPostedEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        _audit.LogAction("PayrollPosted", "PayrollRun", evt.RunId.ToString(), evt);
        await _notifications.SendAsync(evt.TenantId, new SendNotificationDto(
            "Payroll Posted", $"Payroll posted to finance — {evt.TotalNet:N2} SAR.",
            NotificationType.HrPayrollPosted, NotificationChannel.InApp,
            ReferenceType: "PayrollRun", ReferenceId: evt.RunId), ct);
    }
}

public sealed class PerformanceEvaluatedEventHandler : INotificationHandler<DomainEventNotification<PerformanceEvaluatedEvent>>
{
    private readonly IAuditLogger _audit;

    public PerformanceEvaluatedEventHandler(IAuditLogger audit) => _audit = audit;

    public Task Handle(DomainEventNotification<PerformanceEvaluatedEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        _audit.LogAction("PerformanceEvaluated", "PerformanceRecord", evt.RecordId.ToString(), evt);
        return Task.CompletedTask;
    }
}

public sealed class TrainingCompletedEventHandler : INotificationHandler<DomainEventNotification<TrainingCompletedEvent>>
{
    private readonly IAuditLogger _audit;

    public TrainingCompletedEventHandler(IAuditLogger audit) => _audit = audit;

    public Task Handle(DomainEventNotification<TrainingCompletedEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        _audit.LogAction("TrainingCompleted", "EmployeeTrainingRecord", evt.RecordId.ToString(), evt);
        return Task.CompletedTask;
    }
}

public sealed class ApplicantHiredEventHandler : INotificationHandler<DomainEventNotification<ApplicantHiredEvent>>
{
    private readonly IAuditLogger _audit;

    public ApplicantHiredEventHandler(IAuditLogger audit) => _audit = audit;

    public Task Handle(DomainEventNotification<ApplicantHiredEvent> notification, CancellationToken ct)
    {
        var evt = notification.DomainEvent;
        _audit.LogAction("ApplicantHired", "JobApplicant", evt.ApplicantId.ToString(), evt);
        return Task.CompletedTask;
    }
}
