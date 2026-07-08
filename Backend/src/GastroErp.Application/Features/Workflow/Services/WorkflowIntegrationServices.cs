using System.Text.Json;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Features.Hr.DTOs;
using GastroErp.Application.Features.Hr.Services;
using GastroErp.Application.Features.Workflow.DTOs;
using GastroErp.Domain.Entities.HR;
using GastroErp.Domain.Entities.Inventory.Counting;
using GastroErp.Domain.Entities.Inventory.Purchasing;
using GastroErp.Domain.Entities.Inventory.Warehouse;
using GastroErp.Domain.Entities.Sales;
using GastroErp.Domain.Entities.Workflow;
using GastroErp.Domain.Enums;
using GastroErp.Domain.Events.Workflow;
using GastroErp.Domain.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Workflow.Services;

public interface IWorkflowIntegrationService
{
    Task<bool> TryStartWorkflowAsync(Guid tenantId, Guid userId, string workflowCode, string referenceType,
        Guid referenceId, object? context = null, CancellationToken ct = default);
    Task<WorkflowInstanceDto?> GetStatusByReferenceAsync(Guid tenantId, string referenceType, Guid referenceId, CancellationToken ct = default);
    Task<WorkflowTimelineDto?> GetTimelineAsync(Guid tenantId, Guid instanceId, CancellationToken ct = default);
    Task ApplyWorkflowOutcomeAsync(WorkflowInstance instance, bool approved, Guid actorId, string? reason, CancellationToken ct = default);
    string? ResolveWorkflowCode(string referenceType, string? subType = null);
}

public interface IWorkflowModuleOutcomeService
{
    Task ApplyApprovedAsync(Guid tenantId, string referenceType, Guid referenceId, Guid approverId, CancellationToken ct = default);
    Task ApplyRejectedAsync(Guid tenantId, string referenceType, Guid referenceId, string reason, CancellationToken ct = default);
    Task ApplyCancelledAsync(Guid tenantId, string referenceType, Guid referenceId, CancellationToken ct = default);
}

public sealed class WorkflowIntegrationService : IWorkflowIntegrationService
{
    private readonly IWorkflowEngine _engine;
    private readonly IWorkflowHistoryService _history;
    private readonly IApprovalService _approvals;
    private readonly IWorkflowModuleOutcomeService _outcomes;
    private readonly ILogger<WorkflowIntegrationService> _logger;

    public WorkflowIntegrationService(
        IWorkflowEngine engine, IWorkflowHistoryService history, IApprovalService approvals,
        IWorkflowModuleOutcomeService outcomes, ILogger<WorkflowIntegrationService> logger)
        => (_engine, _history, _approvals, _outcomes, _logger) = (engine, history, approvals, outcomes, logger);

    public string? ResolveWorkflowCode(string referenceType, string? subType = null) => referenceType switch
    {
        WorkflowIntegrationReferenceTypes.LeaveRequest => WorkflowIntegrationCodes.LeaveApproval,
        WorkflowIntegrationReferenceTypes.HrWorkflowRequest => subType switch
        {
            nameof(HrWorkflowRequestType.Overtime) => WorkflowIntegrationCodes.OvertimeApproval,
            nameof(HrWorkflowRequestType.Loan) => WorkflowIntegrationCodes.LoanApproval,
            nameof(HrWorkflowRequestType.SalaryAdvance) => WorkflowIntegrationCodes.SalaryAdvanceApproval,
            nameof(HrWorkflowRequestType.Resignation) => WorkflowIntegrationCodes.ResignationApproval,
            nameof(HrWorkflowRequestType.Promotion) => WorkflowIntegrationCodes.PromotionApproval,
            nameof(HrWorkflowRequestType.Transfer) => WorkflowIntegrationCodes.TransferApproval,
            _ => null
        },
        WorkflowIntegrationReferenceTypes.PayrollRun => WorkflowIntegrationCodes.PayrollApproval,
        WorkflowIntegrationReferenceTypes.PerformanceRecord => WorkflowIntegrationCodes.PerformanceApproval,
        WorkflowIntegrationReferenceTypes.JobApplicant => WorkflowIntegrationCodes.RecruitmentApproval,
        WorkflowIntegrationReferenceTypes.PurchaseOrder => WorkflowIntegrationCodes.PurchaseOrderApproval,
        WorkflowIntegrationReferenceTypes.StockCount => WorkflowIntegrationCodes.StockCountApproval,
        WorkflowIntegrationReferenceTypes.StockAdjustment => WorkflowIntegrationCodes.StockAdjustmentApproval,
        WorkflowIntegrationReferenceTypes.StockTransfer => WorkflowIntegrationCodes.StockTransferApproval,
        WorkflowIntegrationReferenceTypes.JournalEntry => WorkflowIntegrationCodes.JournalApproval,
        WorkflowIntegrationReferenceTypes.Refund => WorkflowIntegrationCodes.PosRefundApproval,
        _ => null
    };

    public async Task<bool> TryStartWorkflowAsync(Guid tenantId, Guid userId, string workflowCode, string referenceType,
        Guid referenceId, object? context = null, CancellationToken ct = default)
    {
        try
        {
            var contextJson = context is null ? null : JsonSerializer.Serialize(context);
            await _engine.StartAsync(tenantId, userId, new StartWorkflowDto(workflowCode, referenceType, referenceId, contextJson), ct);
            return true;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Workflow {Code} not configured for {RefType}/{RefId}", workflowCode, referenceType, referenceId);
            return false;
        }
    }

    public Task<WorkflowInstanceDto?> GetStatusByReferenceAsync(Guid tenantId, string referenceType, Guid referenceId, CancellationToken ct = default)
        => _engine.GetByReferenceAsync(tenantId, referenceType, referenceId, ct);

    public async Task<WorkflowTimelineDto?> GetTimelineAsync(Guid tenantId, Guid instanceId, CancellationToken ct = default)
    {
        var instance = await _engine.GetInstanceAsync(tenantId, instanceId, ct);
        if (instance is null) return null;
        var history = await _history.GetHistoryAsync(tenantId, instanceId, ct);
        var approvals = await _approvals.GetApprovalsAsync(tenantId, instanceId, ct);
        return new WorkflowTimelineDto(instance, history, approvals);
    }

    public async Task ApplyWorkflowOutcomeAsync(WorkflowInstance instance, bool approved, Guid actorId, string? reason, CancellationToken ct = default)
    {
        if (approved)
            await _outcomes.ApplyApprovedAsync(instance.TenantId, instance.ReferenceType, instance.ReferenceId, actorId, ct);
        else if (instance.Status == WorkflowStatus.Rejected)
            await _outcomes.ApplyRejectedAsync(instance.TenantId, instance.ReferenceType, instance.ReferenceId, reason ?? "Rejected", ct);
        else if (instance.Status == WorkflowStatus.Cancelled)
            await _outcomes.ApplyCancelledAsync(instance.TenantId, instance.ReferenceType, instance.ReferenceId, ct);
    }
}

public sealed class WorkflowModuleOutcomeService : IWorkflowModuleOutcomeService
{
    private readonly IApplicationDbContext _context;
    private readonly ILeaveManagementService _leave;
    private readonly IPayrollService _payroll;

    public WorkflowModuleOutcomeService(
        IApplicationDbContext context, ILeaveManagementService leave, IPayrollService payroll)
        => (_context, _leave, _payroll) = (context, leave, payroll);

    public async Task ApplyApprovedAsync(Guid tenantId, string referenceType, Guid referenceId, Guid approverId, CancellationToken ct = default)
    {
        switch (referenceType)
        {
            case WorkflowIntegrationReferenceTypes.LeaveRequest:
                await _leave.ProcessApprovalAsync(tenantId, approverId, new ApproveLeaveDto(referenceId, true), ct);
                break;

            case WorkflowIntegrationReferenceTypes.HrWorkflowRequest:
                var hrReq = await _context.HrWorkflowRequests.FirstAsync(r => r.TenantId == tenantId && r.Id == referenceId, ct);
                hrReq.Approve(approverId);
                break;

            case WorkflowIntegrationReferenceTypes.PayrollRun:
                await _payroll.ApproveRunAsync(tenantId, referenceId, approverId, ct);
                break;

            case WorkflowIntegrationReferenceTypes.PerformanceRecord:
                // Performance records are informational; approval acknowledges the record.
                break;

            case WorkflowIntegrationReferenceTypes.JobApplicant:
                // Recruitment approval confirms the offer stage.
                break;

            case WorkflowIntegrationReferenceTypes.PurchaseOrder:
                var po = await _context.PurchaseOrders.FirstAsync(p => p.TenantId == tenantId && p.Id == referenceId, ct);
                po.Approve();
                _context.PurchaseOrders.Update(po);
                break;

            case WorkflowIntegrationReferenceTypes.StockCount:
                var count = await _context.StockCounts.FirstAsync(c => c.TenantId == tenantId && c.Id == referenceId, ct);
                count.Complete();
                break;

            case WorkflowIntegrationReferenceTypes.StockAdjustment:
                var adj = await _context.StockAdjustments.FirstAsync(a => a.TenantId == tenantId && a.Id == referenceId, ct);
                adj.Complete();
                break;

            case WorkflowIntegrationReferenceTypes.StockTransfer:
                var transfer = await _context.StockTransfers.FirstAsync(t => t.TenantId == tenantId && t.Id == referenceId, ct);
                transfer.MarkAsInTransit();
                break;

            case WorkflowIntegrationReferenceTypes.Refund:
                var refund = await _context.Refunds.FirstAsync(r => r.Id == referenceId, ct);
                refund.Approve(approverId);
                refund.Process();
                var payment = await _context.Payments.FirstAsync(p => p.Id == refund.PaymentId, ct);
                payment.MarkRefunded(refund.RefundAmount, refund.Id, refund.SalesOrderId);
                break;
        }
        await _context.SaveChangesAsync(ct);
    }

    public async Task ApplyRejectedAsync(Guid tenantId, string referenceType, Guid referenceId, string reason, CancellationToken ct = default)
    {
        switch (referenceType)
        {
            case WorkflowIntegrationReferenceTypes.LeaveRequest:
                await _leave.ProcessApprovalAsync(tenantId, Guid.Empty, new ApproveLeaveDto(referenceId, false, reason), ct);
                break;
            case WorkflowIntegrationReferenceTypes.HrWorkflowRequest:
                var hrReq = await _context.HrWorkflowRequests.FirstAsync(r => r.TenantId == tenantId && r.Id == referenceId, ct);
                hrReq.Reject(reason);
                break;
            case WorkflowIntegrationReferenceTypes.PayrollRun:
                var payrollRun = await _context.PayrollRuns.FirstAsync(r => r.TenantId == tenantId && r.Id == referenceId, ct);
                payrollRun.Cancel();
                break;
            case WorkflowIntegrationReferenceTypes.PurchaseOrder:
                var po = await _context.PurchaseOrders.FirstAsync(p => p.TenantId == tenantId && p.Id == referenceId, ct);
                po.Reject();
                break;
            case WorkflowIntegrationReferenceTypes.StockCount:
                var count = await _context.StockCounts.FirstAsync(c => c.TenantId == tenantId && c.Id == referenceId, ct);
                count.Cancel();
                break;
            case WorkflowIntegrationReferenceTypes.Refund:
                var refund = await _context.Refunds.FirstAsync(r => r.Id == referenceId, ct);
                refund.Reject();
                break;
        }
        await _context.SaveChangesAsync(ct);
    }

    public async Task ApplyCancelledAsync(Guid tenantId, string referenceType, Guid referenceId, CancellationToken ct = default)
    {
        switch (referenceType)
        {
            case WorkflowIntegrationReferenceTypes.LeaveRequest:
                var leave = await _context.LeaveRequests.FirstAsync(r => r.TenantId == tenantId && r.Id == referenceId, ct);
                leave.Cancel();
                break;
            case WorkflowIntegrationReferenceTypes.HrWorkflowRequest:
                var hrReq = await _context.HrWorkflowRequests.FirstAsync(r => r.TenantId == tenantId && r.Id == referenceId, ct);
                hrReq.Cancel();
                break;
            case WorkflowIntegrationReferenceTypes.PurchaseOrder:
                var po = await _context.PurchaseOrders.FirstAsync(p => p.TenantId == tenantId && p.Id == referenceId, ct);
                po.Cancel();
                break;
        }
        await _context.SaveChangesAsync(ct);
    }
}
