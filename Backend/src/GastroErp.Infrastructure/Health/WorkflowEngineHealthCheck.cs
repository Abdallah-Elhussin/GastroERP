using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Features.Workflow.Services;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GastroErp.Infrastructure.Health;

public sealed class WorkflowEngineHealthCheck : IHealthCheck
{
    private readonly IWorkflowEngine _engine;
    private readonly IWorkflowDefinitionService _definitions;
    private readonly IApplicationDbContext _context;

    public WorkflowEngineHealthCheck(
        IWorkflowEngine engine, IWorkflowDefinitionService definitions, IApplicationDbContext context)
        => (_engine, _definitions, _context) = (engine, definitions, context);

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (_engine is null || _definitions is null)
            return HealthCheckResult.Unhealthy("Workflow engine not available");

        var pending = await _context.WorkflowInstances.CountAsync(
            i => i.Status == WorkflowStatus.InProgress, cancellationToken);
        var escalations = await _context.ApprovalEscalations.CountAsync(e => e.IsActive, cancellationToken);
        var delegations = await _context.ApprovalDelegates.CountAsync(d => d.IsActive, cancellationToken);

        var data = new Dictionary<string, object>
        {
            ["pendingWorkflows"] = pending,
            ["activeEscalations"] = escalations,
            ["activeDelegations"] = delegations
        };

        return pending > 500
            ? HealthCheckResult.Degraded("High pending workflow queue", data: data)
            : HealthCheckResult.Healthy($"Workflow engine OK — {pending} pending", data);
    }
}
