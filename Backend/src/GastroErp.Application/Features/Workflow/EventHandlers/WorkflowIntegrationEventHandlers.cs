using System.Text.Json;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Notifications;
using GastroErp.Application.Features.Workflow.Services;
using GastroErp.Domain.Entities.Workflow;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Hr;
using GastroErp.Domain.Events.Inventory;
using GastroErp.Domain.Events.Workflow;
using GastroErp.Domain.Workflow;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Workflow.EventHandlers;

/// <summary>Starts workflows when module entities are submitted.</summary>
public sealed class WorkflowSubmissionIntegrationHandlers :
    INotificationHandler<DomainEventNotification<LeaveRequestedEvent>>,
    INotificationHandler<DomainEventNotification<HrWorkflowRequestSubmittedEvent>>,
    INotificationHandler<DomainEventNotification<PayrollGeneratedEvent>>,
    INotificationHandler<DomainEventNotification<PerformanceEvaluatedEvent>>,
    INotificationHandler<DomainEventNotification<RecruitmentApprovalRequestedEvent>>,
    INotificationHandler<DomainEventNotification<PurchaseOrderSubmittedEvent>>,
    INotificationHandler<DomainEventNotification<StockCountSubmittedEvent>>,
    INotificationHandler<DomainEventNotification<StockAdjustmentSubmittedEvent>>,
    INotificationHandler<DomainEventNotification<StockTransferSubmittedEvent>>,
    INotificationHandler<DomainEventNotification<RefundRequestedEvent>>
{
    private readonly IWorkflowIntegrationService _integration;

    public WorkflowSubmissionIntegrationHandlers(IWorkflowIntegrationService integration) => _integration = integration;

    public Task Handle(DomainEventNotification<LeaveRequestedEvent> n, CancellationToken ct)
        => Start(n.DomainEvent.TenantId, n.DomainEvent.EmployeeId, WorkflowIntegrationReferenceTypes.LeaveRequest,
            n.DomainEvent.LeaveId, new { leaveType = n.DomainEvent.Type.ToString() }, ct);

    public Task Handle(DomainEventNotification<HrWorkflowRequestSubmittedEvent> n, CancellationToken ct)
    {
        var code = _integration.ResolveWorkflowCode(WorkflowIntegrationReferenceTypes.HrWorkflowRequest, n.DomainEvent.Type.ToString());
        return code is null ? Task.CompletedTask
            : Start(n.DomainEvent.TenantId, n.DomainEvent.EmployeeId, code,
                WorkflowIntegrationReferenceTypes.HrWorkflowRequest, n.DomainEvent.RequestId,
                new { type = n.DomainEvent.Type.ToString(), amount = n.DomainEvent.Amount }, ct);
    }

    public Task Handle(DomainEventNotification<PayrollGeneratedEvent> n, CancellationToken ct)
        => Start(n.DomainEvent.TenantId, n.DomainEvent.CompanyId, WorkflowIntegrationCodes.PayrollApproval,
            WorkflowIntegrationReferenceTypes.PayrollRun, n.DomainEvent.RunId,
            new { totalNet = n.DomainEvent.TotalNet, year = n.DomainEvent.Year, month = n.DomainEvent.Month }, ct);

    public Task Handle(DomainEventNotification<PerformanceEvaluatedEvent> n, CancellationToken ct)
        => Start(n.DomainEvent.TenantId, n.DomainEvent.EmployeeId, WorkflowIntegrationCodes.PerformanceApproval,
            WorkflowIntegrationReferenceTypes.PerformanceRecord, n.DomainEvent.RecordId,
            new { recordType = n.DomainEvent.RecordType.ToString() }, ct);

    public Task Handle(DomainEventNotification<RecruitmentApprovalRequestedEvent> n, CancellationToken ct)
        => Start(n.DomainEvent.TenantId, n.DomainEvent.CompanyId, WorkflowIntegrationCodes.RecruitmentApproval,
            WorkflowIntegrationReferenceTypes.JobApplicant, n.DomainEvent.ApplicantId,
            new { companyId = n.DomainEvent.CompanyId }, ct);

    public Task Handle(DomainEventNotification<PurchaseOrderSubmittedEvent> n, CancellationToken ct)
        => Start(n.DomainEvent.TenantId, n.DomainEvent.PurchaseOrderId, WorkflowIntegrationCodes.PurchaseOrderApproval,
            WorkflowIntegrationReferenceTypes.PurchaseOrder, n.DomainEvent.PurchaseOrderId,
            new { amount = n.DomainEvent.TotalAmount }, ct);

    public Task Handle(DomainEventNotification<StockCountSubmittedEvent> n, CancellationToken ct)
        => Start(n.DomainEvent.TenantId, n.DomainEvent.WarehouseId, WorkflowIntegrationCodes.StockCountApproval,
            WorkflowIntegrationReferenceTypes.StockCount, n.DomainEvent.StockCountId,
            new { warehouseId = n.DomainEvent.WarehouseId }, ct);

    public Task Handle(DomainEventNotification<StockAdjustmentSubmittedEvent> n, CancellationToken ct)
        => Start(n.DomainEvent.TenantId, n.DomainEvent.AdjustmentId, WorkflowIntegrationCodes.StockAdjustmentApproval,
            WorkflowIntegrationReferenceTypes.StockAdjustment, n.DomainEvent.AdjustmentId, null, ct);

    public Task Handle(DomainEventNotification<StockTransferSubmittedEvent> n, CancellationToken ct)
        => Start(n.DomainEvent.TenantId, n.DomainEvent.TransferId, WorkflowIntegrationCodes.StockTransferApproval,
            WorkflowIntegrationReferenceTypes.StockTransfer, n.DomainEvent.TransferId, null, ct);

    public Task Handle(DomainEventNotification<RefundRequestedEvent> n, CancellationToken ct)
        => Start(n.DomainEvent.TenantId, n.DomainEvent.PaymentId, WorkflowIntegrationCodes.PosRefundApproval,
            WorkflowIntegrationReferenceTypes.Refund, n.DomainEvent.RefundId,
            new { amount = n.DomainEvent.Amount, paymentId = n.DomainEvent.PaymentId }, ct);

    private Task Start(Guid tenantId, Guid userId, string code, string refType, Guid refId, object? ctx, CancellationToken ct)
        => _integration.TryStartWorkflowAsync(tenantId, userId, code, refType, refId, ctx, ct);

    private Task Start(Guid tenantId, Guid userId, string refType, Guid refId, object? ctx, CancellationToken ct)
    {
        var code = _integration.ResolveWorkflowCode(refType) ?? throw new InvalidOperationException($"No workflow for {refType}");
        return _integration.TryStartWorkflowAsync(tenantId, userId, code, refType, refId, ctx, ct);
    }
}

/// <summary>Applies module business actions when workflows complete, reject, or cancel.</summary>
public sealed class WorkflowOutcomeIntegrationHandler :
    INotificationHandler<DomainEventNotification<WorkflowCompletedEvent>>,
    INotificationHandler<DomainEventNotification<WorkflowRejectedEvent>>,
    INotificationHandler<DomainEventNotification<WorkflowCancelledEvent>>
{
    private readonly IApplicationDbContext _context;
    private readonly IWorkflowIntegrationService _integration;

    public WorkflowOutcomeIntegrationHandler(IApplicationDbContext context, IWorkflowIntegrationService integration)
        => (_context, _integration) = (context, integration);

    public async Task Handle(DomainEventNotification<WorkflowCompletedEvent> n, CancellationToken ct)
    {
        var evt = n.DomainEvent;
        var instance = await _context.WorkflowInstances.AsNoTracking()
            .FirstAsync(i => i.Id == evt.InstanceId && i.TenantId == evt.TenantId, ct);
        await _integration.ApplyWorkflowOutcomeAsync(instance, approved: true, instance.RequestedBy, null, ct);
    }

    public async Task Handle(DomainEventNotification<WorkflowRejectedEvent> n, CancellationToken ct)
    {
        var evt = n.DomainEvent;
        var instance = await _context.WorkflowInstances.AsNoTracking()
            .FirstAsync(i => i.Id == evt.InstanceId && i.TenantId == evt.TenantId, ct);
        await _integration.ApplyWorkflowOutcomeAsync(instance, approved: false, evt.UserId, evt.Reason, ct);
    }

    public async Task Handle(DomainEventNotification<WorkflowCancelledEvent> n, CancellationToken ct)
    {
        var evt = n.DomainEvent;
        var instance = await _context.WorkflowInstances.AsNoTracking()
            .FirstAsync(i => i.Id == evt.InstanceId && i.TenantId == evt.TenantId, ct);
        await _integration.ApplyWorkflowOutcomeAsync(instance, approved: false, evt.UserId, evt.Reason, ct);
    }
}
