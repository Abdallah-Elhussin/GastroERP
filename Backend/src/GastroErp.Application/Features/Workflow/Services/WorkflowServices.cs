using System.Text.Json;
using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Features.Automation.Services;
using GastroErp.Application.Features.Workflow.DTOs;
using GastroErp.Domain.Entities.Workflow;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Workflow.Services;

public interface IWorkflowDefinitionService
{
    Task<WorkflowDefinitionDto> CreateAsync(Guid tenantId, CreateWorkflowDefinitionDto dto, CancellationToken ct = default);
    Task<WorkflowDefinitionDto?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<WorkflowDefinitionDto?> GetByCodeAsync(Guid tenantId, string code, CancellationToken ct = default);
    Task<IReadOnlyList<WorkflowDefinitionDto>> ListAsync(Guid tenantId, WorkflowFilterDto filter, CancellationToken ct = default);
    Task<WorkflowDefinitionDto> UpdateAsync(Guid tenantId, Guid id, UpdateWorkflowDefinitionDto dto, CancellationToken ct = default);
    Task<WorkflowDefinitionDto> PublishAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task ActivateAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task DeactivateAsync(Guid tenantId, Guid id, CancellationToken ct = default);
}

public interface IWorkflowEngine
{
    Task<WorkflowInstanceDto> StartAsync(Guid tenantId, Guid userId, StartWorkflowDto dto, CancellationToken ct = default);
    Task<WorkflowInstanceDto> ApproveAsync(Guid tenantId, Guid userId, ApproveWorkflowDto dto, CancellationToken ct = default);
    Task<WorkflowInstanceDto> RejectAsync(Guid tenantId, Guid userId, RejectWorkflowDto dto, CancellationToken ct = default);
    Task<WorkflowInstanceDto> CancelAsync(Guid tenantId, Guid userId, CancelWorkflowDto dto, CancellationToken ct = default);
    Task<WorkflowInstanceDto?> GetInstanceAsync(Guid tenantId, Guid instanceId, CancellationToken ct = default);
    Task<WorkflowInstanceDto?> GetByReferenceAsync(Guid tenantId, string referenceType, Guid referenceId, CancellationToken ct = default);
    Task<WorkflowInstanceDto> RestartAsync(Guid tenantId, Guid userId, Guid instanceId, CancellationToken ct = default);
    Task<WorkflowInstanceDto> ReturnToPreviousStepAsync(Guid tenantId, Guid userId, Guid instanceId, CancellationToken ct = default);
    Task<bool> EvaluateConditionsAsync(Guid tenantId, Guid definitionId, string? contextJson, CancellationToken ct = default);
}

public interface IApprovalService
{
    Task<IReadOnlyList<WorkflowApprovalDto>> GetApprovalsAsync(Guid tenantId, Guid instanceId, CancellationToken ct = default);
    Task<IReadOnlyList<WorkflowInstanceDto>> GetPendingApprovalsAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<UserTaskDto>> GetUserTasksAsync(Guid tenantId, Guid userId, CancellationToken ct = default);
}

public interface IWorkflowHistoryService
{
    Task<IReadOnlyList<WorkflowHistoryDto>> GetHistoryAsync(Guid tenantId, Guid instanceId, CancellationToken ct = default);
    Task LogAsync(Guid tenantId, Guid instanceId, string action, string? oldStatus, string? newStatus, Guid? userId, string? details = null, CancellationToken ct = default);
}

public interface IDelegateService
{
    Task<ApprovalDelegateDto> CreateAsync(Guid tenantId, Guid userId, CreateApprovalDelegateDto dto, CancellationToken ct = default);
    Task<ApprovalDelegateDto> UpdateAsync(Guid tenantId, Guid id, UpdateApprovalDelegateDto dto, CancellationToken ct = default);
    Task DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ApprovalDelegateDto>> GetActiveDelegationsAsync(Guid tenantId, Guid? userId = null, CancellationToken ct = default);
    Task<Guid?> ResolveApproverAsync(Guid tenantId, Guid userId, CancellationToken ct = default);
}

public interface IEscalationService
{
    Task RunEscalationsAsync(Guid tenantId, CancellationToken ct = default);
    Task RunRemindersAsync(Guid tenantId, CancellationToken ct = default);
}

public interface IWorkflowJobExecutor
{
    Task RunEscalationJobAsync(Guid tenantId, CancellationToken ct = default);
    Task RunReminderJobAsync(Guid tenantId, CancellationToken ct = default);
    Task RunCleanupJobAsync(Guid tenantId, CancellationToken ct = default);
    Task RunDelegationExpiryJobAsync(Guid tenantId, CancellationToken ct = default);
    Task RunRetryJobAsync(Guid tenantId, CancellationToken ct = default);
    Task RunTimeoutJobAsync(Guid tenantId, CancellationToken ct = default);
}

public sealed class WorkflowDefinitionService : IWorkflowDefinitionService
{
    private readonly IApplicationDbContext _context;

    public WorkflowDefinitionService(IApplicationDbContext context) => _context = context;

    public async Task<WorkflowDefinitionDto> CreateAsync(Guid tenantId, CreateWorkflowDefinitionDto dto, CancellationToken ct = default)
    {
        var exists = await _context.WorkflowDefinitions.AnyAsync(
            w => w.TenantId == tenantId && w.Code == dto.Code.Trim().ToUpperInvariant(), ct);
        if (exists) throw new InvalidOperationException("Workflow code already exists.");

        var def = WorkflowDefinition.Create(tenantId, dto.Name, dto.Code, dto.Module, dto.Description, dto.Trigger, dto.Priority);
        _context.WorkflowDefinitions.Add(def);

        if (dto.Steps is not null)
            foreach (var s in dto.Steps.OrderBy(x => x.StepOrder))
                _context.WorkflowSteps.Add(WorkflowStep.Create(tenantId, def.Id, s.StepOrder, s.Name, s.ApprovalType, s.IsFinalStep, s.ApproverRoleId));

        if (dto.Conditions is not null)
            foreach (var c in dto.Conditions)
                _context.WorkflowConditions.Add(WorkflowCondition.Create(tenantId, def.Id, c.FieldName, c.Operator, c.Value, c.LogicalOperator));

        await _context.SaveChangesAsync(ct);
        return (await GetByIdAsync(tenantId, def.Id, ct))!;
    }

    public async Task<WorkflowDefinitionDto?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var def = await _context.WorkflowDefinitions.AsNoTracking()
            .FirstOrDefaultAsync(w => w.TenantId == tenantId && w.Id == id, ct);
        return def is null ? null : await MapDefinition(def, ct);
    }

    public async Task<WorkflowDefinitionDto?> GetByCodeAsync(Guid tenantId, string code, CancellationToken ct = default)
    {
        var def = await _context.WorkflowDefinitions.AsNoTracking()
            .Where(w => w.TenantId == tenantId && w.Code == code.Trim().ToUpperInvariant() && w.IsPublished && w.IsActive)
            .OrderByDescending(w => w.Version).FirstOrDefaultAsync(ct);
        return def is null ? null : await MapDefinition(def, ct);
    }

    public async Task<IReadOnlyList<WorkflowDefinitionDto>> ListAsync(Guid tenantId, WorkflowFilterDto filter, CancellationToken ct = default)
    {
        var q = _context.WorkflowDefinitions.AsNoTracking().Where(w => w.TenantId == tenantId);
        if (filter.Module.HasValue) q = q.Where(w => w.Module == filter.Module);
        if (filter.IsActive.HasValue) q = q.Where(w => w.IsActive == filter.IsActive);
        var items = await q.OrderBy(w => w.Name).Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync(ct);
        var result = new List<WorkflowDefinitionDto>();
        foreach (var item in items) result.Add(await MapDefinition(item, ct));
        return result;
    }

    public async Task<WorkflowDefinitionDto> UpdateAsync(Guid tenantId, Guid id, UpdateWorkflowDefinitionDto dto, CancellationToken ct = default)
    {
        var def = await _context.WorkflowDefinitions.FirstAsync(w => w.TenantId == tenantId && w.Id == id, ct);
        def.Update(dto.Name, dto.Description, dto.Trigger, dto.Priority);
        await _context.SaveChangesAsync(ct);
        return (await GetByIdAsync(tenantId, id, ct))!;
    }

    public async Task<WorkflowDefinitionDto> PublishAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var def = await _context.WorkflowDefinitions.FirstAsync(w => w.TenantId == tenantId && w.Id == id, ct);
        var hasSteps = await _context.WorkflowSteps.AnyAsync(s => s.WorkflowDefinitionId == id, ct);
        if (!hasSteps) throw new InvalidOperationException("Workflow must have at least one step.");
        def.Publish();
        await _context.SaveChangesAsync(ct);
        return (await GetByIdAsync(tenantId, id, ct))!;
    }

    public async Task ActivateAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var def = await _context.WorkflowDefinitions.FirstAsync(w => w.TenantId == tenantId && w.Id == id, ct);
        def.Activate();
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeactivateAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var def = await _context.WorkflowDefinitions.FirstAsync(w => w.TenantId == tenantId && w.Id == id, ct);
        def.Deactivate();
        await _context.SaveChangesAsync(ct);
    }

    private async Task<WorkflowDefinitionDto> MapDefinition(WorkflowDefinition def, CancellationToken ct)
    {
        var steps = await _context.WorkflowSteps.AsNoTracking()
            .Where(s => s.WorkflowDefinitionId == def.Id).OrderBy(s => s.StepOrder)
            .Select(s => new WorkflowStepDto(s.Id, s.StepOrder, s.Name, s.ApprovalType, s.IsFinalStep, s.ApproverRoleId))
            .ToListAsync(ct);
        var conditions = await _context.WorkflowConditions.AsNoTracking()
            .Where(c => c.WorkflowDefinitionId == def.Id)
            .Select(c => new WorkflowConditionDto(c.Id, c.FieldName, c.Operator, c.Value))
            .ToListAsync(ct);
        return new WorkflowDefinitionDto(def.Id, def.Name, def.Code, def.Description, def.Module,
            def.Trigger, def.Priority, def.Version, def.IsActive, def.IsPublished, steps, conditions);
    }
}

public sealed class WorkflowEngine : IWorkflowEngine
{
    private readonly IApplicationDbContext _context;
    private readonly IWorkflowHistoryService _history;
    private readonly ILogger<WorkflowEngine> _logger;

    public WorkflowEngine(IApplicationDbContext context, IWorkflowHistoryService history, ILogger<WorkflowEngine> logger)
        => (_context, _history, _logger) = (context, history, logger);

    public async Task<WorkflowInstanceDto> StartAsync(Guid tenantId, Guid userId, StartWorkflowDto dto, CancellationToken ct = default)
    {
        var def = await _context.WorkflowDefinitions.AsNoTracking()
            .Where(w => w.TenantId == tenantId && w.Code == dto.WorkflowCode.Trim().ToUpperInvariant() && w.IsPublished && w.IsActive)
            .OrderByDescending(w => w.Version).FirstOrDefaultAsync(ct)
            ?? throw new InvalidOperationException("Workflow definition not found.");

        if (!await EvaluateConditionsAsync(tenantId, def.Id, dto.ContextJson, ct))
            throw new InvalidOperationException("Workflow conditions not met.");

        var firstStep = await _context.WorkflowSteps.AsNoTracking()
            .Where(s => s.WorkflowDefinitionId == def.Id).OrderBy(s => s.StepOrder).FirstOrDefaultAsync(ct)
            ?? throw new InvalidOperationException("Workflow has no steps.");

        var instance = WorkflowInstance.Start(tenantId, def.Id, dto.ReferenceType, dto.ReferenceId, userId,
            dto.Priority ?? def.Priority, firstStep.Id, firstStep.StepOrder, dto.ContextJson);
        _context.WorkflowInstances.Add(instance);
        await _history.LogAsync(tenantId, instance.Id, "Started", null, WorkflowStatus.InProgress.ToString(), userId, ct: ct);
        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Workflow {Code} started for {RefType}/{RefId}", dto.WorkflowCode, dto.ReferenceType, dto.ReferenceId);
        return await MapInstance(instance, def.Name, firstStep.Name, ct);
    }

    public async Task<WorkflowInstanceDto> ApproveAsync(Guid tenantId, Guid userId, ApproveWorkflowDto dto, CancellationToken ct = default)
    {
        var instance = await _context.WorkflowInstances.FirstAsync(i => i.TenantId == tenantId && i.Id == dto.InstanceId, ct);
        if (instance.Status != WorkflowStatus.InProgress)
            throw new InvalidOperationException("Workflow is not in progress.");

        var currentStep = await _context.WorkflowSteps.AsNoTracking()
            .FirstAsync(s => s.Id == instance.CurrentStepId, ct);

        _context.WorkflowApprovals.Add(WorkflowApproval.Record(
            tenantId, instance.Id, currentStep.Id, userId, ApprovalDecision.Approve, dto.Comments));
        await _history.LogAsync(tenantId, instance.Id, "Approved", instance.Status.ToString(),
            WorkflowStatus.Approved.ToString(), userId, dto.Comments, ct);

        if (currentStep.IsFinalStep)
        {
            instance.Approve(userId);
        }
        else
        {
            var nextStep = await _context.WorkflowSteps.AsNoTracking()
                .Where(s => s.WorkflowDefinitionId == instance.WorkflowDefinitionId && s.StepOrder > currentStep.StepOrder)
                .OrderBy(s => s.StepOrder).FirstOrDefaultAsync(ct);
            if (nextStep is null)
                instance.Approve(userId);
            else
                instance.AdvanceToStep(nextStep.Id, nextStep.StepOrder, nextStep.Name);
        }

        await _context.SaveChangesAsync(ct);
        var defName = await _context.WorkflowDefinitions.AsNoTracking()
            .Where(d => d.Id == instance.WorkflowDefinitionId).Select(d => d.Name).FirstAsync(ct);
        return await MapInstance(instance, defName, currentStep.Name, ct);
    }

    public async Task<WorkflowInstanceDto> RejectAsync(Guid tenantId, Guid userId, RejectWorkflowDto dto, CancellationToken ct = default)
    {
        var instance = await _context.WorkflowInstances.FirstAsync(i => i.TenantId == tenantId && i.Id == dto.InstanceId, ct);
        if (instance.Status != WorkflowStatus.InProgress)
            throw new InvalidOperationException("Workflow is not in progress.");

        var stepId = instance.CurrentStepId ?? Guid.Empty;
        _context.WorkflowApprovals.Add(WorkflowApproval.Record(
            tenantId, instance.Id, stepId, userId, ApprovalDecision.Reject, dto.Reason));
        instance.Reject(userId, dto.Reason);
        await _history.LogAsync(tenantId, instance.Id, "Rejected", WorkflowStatus.InProgress.ToString(),
            WorkflowStatus.Rejected.ToString(), userId, dto.Reason, ct);
        await _context.SaveChangesAsync(ct);

        var defName = await _context.WorkflowDefinitions.AsNoTracking()
            .Where(d => d.Id == instance.WorkflowDefinitionId).Select(d => d.Name).FirstAsync(ct);
        return await MapInstance(instance, defName, null, ct);
    }

    public async Task<WorkflowInstanceDto> CancelAsync(Guid tenantId, Guid userId, CancelWorkflowDto dto, CancellationToken ct = default)
    {
        var instance = await _context.WorkflowInstances.FirstAsync(i => i.TenantId == tenantId && i.Id == dto.InstanceId, ct);
        if (instance.Status is WorkflowStatus.Completed or WorkflowStatus.Approved or WorkflowStatus.Rejected or WorkflowStatus.Cancelled)
            throw new InvalidOperationException("Workflow cannot be cancelled.");

        instance.Cancel(userId, dto.Reason);
        await _history.LogAsync(tenantId, instance.Id, "Cancelled", instance.Status.ToString(),
            WorkflowStatus.Cancelled.ToString(), userId, dto.Reason, ct);
        await _context.SaveChangesAsync(ct);

        var defName = await _context.WorkflowDefinitions.AsNoTracking()
            .Where(d => d.Id == instance.WorkflowDefinitionId).Select(d => d.Name).FirstAsync(ct);
        return await MapInstance(instance, defName, null, ct);
    }

    public async Task<WorkflowInstanceDto?> GetInstanceAsync(Guid tenantId, Guid instanceId, CancellationToken ct = default)
    {
        var instance = await _context.WorkflowInstances.AsNoTracking()
            .FirstOrDefaultAsync(i => i.TenantId == tenantId && i.Id == instanceId, ct);
        if (instance is null) return null;
        var defName = await _context.WorkflowDefinitions.AsNoTracking()
            .Where(d => d.Id == instance.WorkflowDefinitionId).Select(d => d.Name).FirstAsync(ct);
        string? stepName = null;
        if (instance.CurrentStepId.HasValue)
            stepName = await _context.WorkflowSteps.AsNoTracking()
                .Where(s => s.Id == instance.CurrentStepId).Select(s => s.Name).FirstOrDefaultAsync(ct);
        return await MapInstance(instance, defName, stepName, ct);
    }

    public async Task<WorkflowInstanceDto?> GetByReferenceAsync(Guid tenantId, string referenceType, Guid referenceId, CancellationToken ct = default)
    {
        var instance = await _context.WorkflowInstances.AsNoTracking()
            .Where(i => i.TenantId == tenantId && i.ReferenceType == referenceType && i.ReferenceId == referenceId)
            .OrderByDescending(i => i.CreatedAt).FirstOrDefaultAsync(ct);
        if (instance is null) return null;
        return await GetInstanceAsync(tenantId, instance.Id, ct);
    }

    public async Task<WorkflowInstanceDto> RestartAsync(Guid tenantId, Guid userId, Guid instanceId, CancellationToken ct = default)
    {
        var old = await _context.WorkflowInstances.FirstAsync(i => i.TenantId == tenantId && i.Id == instanceId, ct);
        if (old.Status is not WorkflowStatus.Rejected and not WorkflowStatus.Cancelled)
            throw new InvalidOperationException("Only rejected or cancelled workflows can be restarted.");

        var def = await _context.WorkflowDefinitions.AsNoTracking()
            .FirstAsync(d => d.Id == old.WorkflowDefinitionId, ct);
        await _history.LogAsync(tenantId, old.Id, "Restarted", old.Status.ToString(), old.Status.ToString(), userId, ct: ct);

        var dto = new StartWorkflowDto(def.Code, old.ReferenceType, old.ReferenceId, old.ContextJson, old.Priority);
        var restarted = await StartAsync(tenantId, userId, dto, ct);
        old.MarkRestarted(restarted.Id);
        await _context.SaveChangesAsync(ct);
        return restarted;
    }

    public async Task<WorkflowInstanceDto> ReturnToPreviousStepAsync(Guid tenantId, Guid userId, Guid instanceId, CancellationToken ct = default)
    {
        var instance = await _context.WorkflowInstances.FirstAsync(i => i.TenantId == tenantId && i.Id == instanceId, ct);
        if (instance.Status != WorkflowStatus.InProgress || !instance.CurrentStepId.HasValue)
            throw new InvalidOperationException("Workflow cannot be returned.");

        var currentStep = await _context.WorkflowSteps.AsNoTracking()
            .FirstAsync(s => s.Id == instance.CurrentStepId, ct);
        var previousStep = await _context.WorkflowSteps.AsNoTracking()
            .Where(s => s.WorkflowDefinitionId == instance.WorkflowDefinitionId && s.StepOrder < currentStep.StepOrder)
            .OrderByDescending(s => s.StepOrder).FirstOrDefaultAsync(ct)
            ?? throw new InvalidOperationException("No previous step.");

        instance.ReturnToPreviousStep(previousStep.Id, previousStep.StepOrder, previousStep.Name);
        await _history.LogAsync(tenantId, instance.Id, "Returned", currentStep.Name, previousStep.Name, userId, ct: ct);
        await _context.SaveChangesAsync(ct);

        var defName = await _context.WorkflowDefinitions.AsNoTracking()
            .Where(d => d.Id == instance.WorkflowDefinitionId).Select(d => d.Name).FirstAsync(ct);
        return await MapInstance(instance, defName, previousStep.Name, ct);
    }

    public async Task<bool> EvaluateConditionsAsync(Guid tenantId, Guid definitionId, string? contextJson, CancellationToken ct = default)
    {
        var conditions = await _context.WorkflowConditions.AsNoTracking()
            .Where(c => c.TenantId == tenantId && c.WorkflowDefinitionId == definitionId).ToListAsync(ct);
        if (conditions.Count == 0) return true;
        if (string.IsNullOrWhiteSpace(contextJson)) return false;

        Dictionary<string, JsonElement>? ctx;
        try { ctx = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(contextJson); }
        catch { return false; }
        if (ctx is null) return false;

        foreach (var cond in conditions)
        {
            if (!ctx.TryGetValue(cond.FieldName, out var fieldVal)) return false;
            if (!EvaluateSingle(cond.Operator, fieldVal, cond.Value)) return false;
        }
        return true;
    }

    private static bool EvaluateSingle(WorkflowConditionOperator op, JsonElement field, string expected)
    {
        return op switch
        {
            WorkflowConditionOperator.Equals => field.ToString().Equals(expected, StringComparison.OrdinalIgnoreCase),
            WorkflowConditionOperator.NotEquals => !field.ToString().Equals(expected, StringComparison.OrdinalIgnoreCase),
            WorkflowConditionOperator.GreaterThan => decimal.TryParse(field.ToString(), out var a) && decimal.TryParse(expected, out var b) && a > b,
            WorkflowConditionOperator.GreaterThanOrEqual => decimal.TryParse(field.ToString(), out var a) && decimal.TryParse(expected, out var b) && a >= b,
            WorkflowConditionOperator.LessThan => decimal.TryParse(field.ToString(), out var a) && decimal.TryParse(expected, out var b) && a < b,
            WorkflowConditionOperator.LessThanOrEqual => decimal.TryParse(field.ToString(), out var a) && decimal.TryParse(expected, out var b) && a <= b,
            WorkflowConditionOperator.Contains => field.ToString().Contains(expected, StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }

    private static Task<WorkflowInstanceDto> MapInstance(WorkflowInstance i, string defName, string? stepName, CancellationToken _)
        => Task.FromResult(new WorkflowInstanceDto(i.Id, i.WorkflowDefinitionId, defName, i.ReferenceType, i.ReferenceId,
            i.Status, i.Priority, i.CurrentStepOrder, stepName, i.RequestedBy, i.CreatedAt, i.CompletedAt));
}

public sealed class ApprovalService : IApprovalService
{
    private readonly IApplicationDbContext _context;

    public ApprovalService(IApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<WorkflowApprovalDto>> GetApprovalsAsync(Guid tenantId, Guid instanceId, CancellationToken ct = default)
        => await _context.WorkflowApprovals.AsNoTracking()
            .Where(a => a.TenantId == tenantId && a.WorkflowInstanceId == instanceId)
            .OrderByDescending(a => a.DecisionDate)
            .Select(a => new WorkflowApprovalDto(a.Id, a.WorkflowInstanceId, a.WorkflowStepId, a.ApproverId,
                a.Decision, a.Status, a.DecisionDate, a.Comments))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<WorkflowInstanceDto>> GetPendingApprovalsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var instances = await _context.WorkflowInstances.AsNoTracking()
            .Where(i => i.TenantId == tenantId && i.Status == WorkflowStatus.InProgress)
            .OrderByDescending(i => i.CreatedAt).Take(100).ToListAsync(ct);
        return await MapInstances(instances, ct);
    }

    public async Task<IReadOnlyList<UserTaskDto>> GetUserTasksAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var instances = await _context.WorkflowInstances.AsNoTracking()
            .Where(i => i.TenantId == tenantId && i.Status == WorkflowStatus.InProgress).ToListAsync(ct);
        var result = new List<UserTaskDto>();
        foreach (var i in instances)
        {
            var defName = await _context.WorkflowDefinitions.AsNoTracking()
                .Where(d => d.Id == i.WorkflowDefinitionId).Select(d => d.Name).FirstOrDefaultAsync(ct) ?? "Workflow";
            var stepName = i.CurrentStepId.HasValue
                ? await _context.WorkflowSteps.AsNoTracking().Where(s => s.Id == i.CurrentStepId).Select(s => s.Name).FirstOrDefaultAsync(ct) ?? "Step"
                : "Step";
            result.Add(new UserTaskDto(i.Id, defName, i.ReferenceType, i.ReferenceId, stepName, i.Priority, i.CreatedAt));
        }
        return result;
    }

    private async Task<IReadOnlyList<WorkflowInstanceDto>> MapInstances(List<WorkflowInstance> instances, CancellationToken ct)
    {
        var result = new List<WorkflowInstanceDto>();
        foreach (var i in instances)
        {
            var defName = await _context.WorkflowDefinitions.AsNoTracking()
                .Where(d => d.Id == i.WorkflowDefinitionId).Select(d => d.Name).FirstOrDefaultAsync(ct) ?? "Workflow";
            var stepName = i.CurrentStepId.HasValue
                ? await _context.WorkflowSteps.AsNoTracking().Where(s => s.Id == i.CurrentStepId).Select(s => s.Name).FirstOrDefaultAsync(ct)
                : null;
            result.Add(new WorkflowInstanceDto(i.Id, i.WorkflowDefinitionId, defName, i.ReferenceType, i.ReferenceId,
                i.Status, i.Priority, i.CurrentStepOrder, stepName, i.RequestedBy, i.CreatedAt, i.CompletedAt));
        }
        return result;
    }
}

public sealed class WorkflowHistoryService : IWorkflowHistoryService
{
    private readonly IApplicationDbContext _context;

    public WorkflowHistoryService(IApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<WorkflowHistoryDto>> GetHistoryAsync(Guid tenantId, Guid instanceId, CancellationToken ct = default)
        => await _context.WorkflowHistories.AsNoTracking()
            .Where(h => h.TenantId == tenantId && h.WorkflowInstanceId == instanceId)
            .OrderByDescending(h => h.CreatedAt)
            .Select(h => new WorkflowHistoryDto(h.Id, h.WorkflowInstanceId, h.Action, h.OldStatus, h.NewStatus,
                h.UserId, h.Details, h.CreatedAt))
            .ToListAsync(ct);

    public Task LogAsync(Guid tenantId, Guid instanceId, string action, string? oldStatus, string? newStatus,
        Guid? userId, string? details = null, CancellationToken ct = default)
    {
        _context.WorkflowHistories.Add(WorkflowHistory.Record(tenantId, instanceId, action, oldStatus, newStatus, userId, details));
        return Task.CompletedTask;
    }
}

public sealed class DelegateService : IDelegateService
{
    private readonly IApplicationDbContext _context;

    public DelegateService(IApplicationDbContext context) => _context = context;

    public async Task<ApprovalDelegateDto> CreateAsync(Guid tenantId, Guid userId, CreateApprovalDelegateDto dto, CancellationToken ct = default)
    {
        var d = ApprovalDelegate.Create(tenantId, userId, dto.DelegateUserId, dto.FromDate, dto.ToDate, dto.Reason);
        _context.ApprovalDelegates.Add(d);
        await _context.SaveChangesAsync(ct);
        return Map(d);
    }

    public async Task<ApprovalDelegateDto> UpdateAsync(Guid tenantId, Guid id, UpdateApprovalDelegateDto dto, CancellationToken ct = default)
    {
        var d = await _context.ApprovalDelegates.FirstAsync(x => x.TenantId == tenantId && x.Id == id, ct);
        d.Update(dto.DelegateUserId, dto.FromDate, dto.ToDate, dto.Reason);
        await _context.SaveChangesAsync(ct);
        return Map(d);
    }

    public async Task DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var d = await _context.ApprovalDelegates.FirstAsync(x => x.TenantId == tenantId && x.Id == id, ct);
        d.Deactivate();
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<ApprovalDelegateDto>> GetActiveDelegationsAsync(Guid tenantId, Guid? userId = null, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var q = _context.ApprovalDelegates.AsNoTracking()
            .Where(d => d.TenantId == tenantId && d.IsActive && d.FromDate <= today && d.ToDate >= today);
        if (userId.HasValue) q = q.Where(d => d.UserId == userId || d.DelegateUserId == userId);
        return await q.Select(d => new ApprovalDelegateDto(d.Id, d.UserId, d.DelegateUserId, d.FromDate, d.ToDate, d.IsActive, d.Reason))
            .ToListAsync(ct);
    }

    public async Task<Guid?> ResolveApproverAsync(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var delegation = await _context.ApprovalDelegates.AsNoTracking()
            .FirstOrDefaultAsync(d => d.TenantId == tenantId && d.UserId == userId && d.IsActive
                && d.FromDate <= today && d.ToDate >= today, ct);
        return delegation?.DelegateUserId;
    }

    private static ApprovalDelegateDto Map(ApprovalDelegate d)
        => new(d.Id, d.UserId, d.DelegateUserId, d.FromDate, d.ToDate, d.IsActive, d.Reason);
}

public sealed class EscalationService : IEscalationService
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationOrchestrator _notifications;

    public EscalationService(IApplicationDbContext context, INotificationOrchestrator notifications)
        => (_context, _notifications) = (context, notifications);

    public async Task RunEscalationsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var escalations = await _context.ApprovalEscalations.AsNoTracking()
            .Where(e => e.TenantId == tenantId && e.IsActive).ToListAsync(ct);
        var threshold = DateTimeOffset.UtcNow;

        foreach (var esc in escalations)
        {
            var pending = await _context.WorkflowInstances
                .Where(i => i.TenantId == tenantId && i.Status == WorkflowStatus.InProgress && i.CurrentStepId == esc.WorkflowStepId
                    && i.CreatedAt.AddHours(esc.EscalateAfterHours) <= threshold).ToListAsync(ct);
            foreach (var inst in pending)
            {
                inst.MarkEscalated(esc.WorkflowStepId, esc.EscalateToRole);
                await _notifications.SendAsync(tenantId, new Features.Automation.DTOs.SendNotificationDto(
                    "Workflow Escalated", $"Workflow escalated to role {esc.EscalateToRole}.",
                    NotificationType.WorkflowEscalated, NotificationChannel.InApp,
                    ReferenceType: "WorkflowInstance", ReferenceId: inst.Id), ct);
            }
        }
        await _context.SaveChangesAsync(ct);
    }

    public async Task RunRemindersAsync(Guid tenantId, CancellationToken ct = default)
    {
        var count = await _context.WorkflowInstances.CountAsync(
            i => i.TenantId == tenantId && i.Status == WorkflowStatus.InProgress, ct);
        if (count == 0) return;
        await _notifications.SendAsync(tenantId, new Features.Automation.DTOs.SendNotificationDto(
            "Pending Approvals", $"{count} workflow(s) awaiting approval.",
            NotificationType.ApprovalRequested, NotificationChannel.InApp), ct);
    }
}

public sealed class WorkflowJobExecutor : IWorkflowJobExecutor
{
    private readonly IEscalationService _escalation;
    private readonly IDelegateService _delegate;
    private readonly IApplicationDbContext _context;

    public WorkflowJobExecutor(IEscalationService escalation, IDelegateService delegateService, IApplicationDbContext context)
        => (_escalation, _delegate, _context) = (escalation, delegateService, context);

    public Task RunEscalationJobAsync(Guid tenantId, CancellationToken ct = default)
        => _escalation.RunEscalationsAsync(tenantId, ct);

    public Task RunReminderJobAsync(Guid tenantId, CancellationToken ct = default)
        => _escalation.RunRemindersAsync(tenantId, ct);

    public async Task RunCleanupJobAsync(Guid tenantId, CancellationToken ct = default)
    {
        var cutoff = DateTimeOffset.UtcNow.AddYears(-2);
        var old = await _context.WorkflowInstances
            .Where(i => i.TenantId == tenantId && i.CompletedAt != null && i.CompletedAt < cutoff).CountAsync(ct);
        if (old > 0)
            await _escalation.RunRemindersAsync(tenantId, ct);
    }

    public async Task RunDelegationExpiryJobAsync(Guid tenantId, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var expired = await _context.ApprovalDelegates
            .Where(d => d.TenantId == tenantId && d.IsActive && d.ToDate < today).ToListAsync(ct);
        foreach (var d in expired) d.Deactivate();
        await _context.SaveChangesAsync(ct);
    }

    public async Task RunRetryJobAsync(Guid tenantId, CancellationToken ct = default)
    {
        var stuck = await _context.WorkflowInstances
            .Where(i => i.TenantId == tenantId && i.Status == WorkflowStatus.InProgress
                && i.CreatedAt < DateTimeOffset.UtcNow.AddHours(-48)).Take(20).ToListAsync(ct);
        if (stuck.Count > 0)
            await _escalation.RunRemindersAsync(tenantId, ct);
    }

    public async Task RunTimeoutJobAsync(Guid tenantId, CancellationToken ct = default)
    {
        var threshold = DateTimeOffset.UtcNow.AddDays(-30);
        var timedOut = await _context.WorkflowInstances
            .Where(i => i.TenantId == tenantId && i.Status == WorkflowStatus.InProgress && i.CreatedAt < threshold)
            .ToListAsync(ct);
        foreach (var inst in timedOut)
            inst.Cancel(Guid.Empty, "Timed out");
        if (timedOut.Count > 0)
            await _context.SaveChangesAsync(ct);
    }
}
