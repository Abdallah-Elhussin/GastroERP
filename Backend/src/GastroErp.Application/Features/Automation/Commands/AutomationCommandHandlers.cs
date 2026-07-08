using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Responses;
using GastroErp.Application.Features.Automation.Commands;
using GastroErp.Application.Features.Automation.DTOs;
using GastroErp.Application.Features.Automation.Services;
using GastroErp.Domain.Entities.Automation;
using GastroErp.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GastroErp.Application.Features.Automation.Commands;

public sealed class ExecuteJobCommandHandler : IRequestHandler<ExecuteJobCommand, Result<JobDto>>
{
    private readonly IScheduledJobCatalog _catalog;
    private readonly IApplicationDbContext _context;

    public ExecuteJobCommandHandler(IScheduledJobCatalog catalog, IApplicationDbContext context)
        => (_catalog, _context) = (catalog, context);

    public async Task<Result<JobDto>> Handle(ExecuteJobCommand request, CancellationToken cancellationToken)
    {
        var tenantId = request.Dto.TenantId ?? request.TenantId;
        await _catalog.ExecuteNamedJobAsync(tenantId, request.Dto.JobName, cancellationToken);

        var log = await _context.JobExecutionLogs.AsNoTracking()
            .Where(j => j.TenantId == tenantId && j.JobName == request.Dto.JobName)
            .OrderByDescending(j => j.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (log is null)
            return Result<JobDto>.Failure("JobNotFound", "Job execution log not found.");

        return Result<JobDto>.Success(Map(log));
    }

    internal static JobDto Map(JobExecutionLog j) =>
        new(j.Id, j.JobName, j.Queue, j.Status, j.ExternalJobId, j.StartedAt, j.FinishedAt, j.RetryCount, j.ErrorMessage);
}

public sealed class RetryJobCommandHandler : IRequestHandler<RetryJobCommand, Result<JobDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IScheduledJobCatalog _catalog;
    private readonly IRetryPolicyService _retry;

    public RetryJobCommandHandler(
        IApplicationDbContext context, IScheduledJobCatalog catalog, IRetryPolicyService retry)
        => (_context, _catalog, _retry) = (context, catalog, retry);

    public async Task<Result<JobDto>> Handle(RetryJobCommand request, CancellationToken cancellationToken)
    {
        var log = await _context.JobExecutionLogs
            .FirstOrDefaultAsync(j => j.Id == request.JobLogId && j.TenantId == request.TenantId, cancellationToken);
        if (log is null) return Result<JobDto>.Failure("NotFound", "Job log not found.");
        if (log.Status != JobExecutionStatus.Failed && log.Status != JobExecutionStatus.DeadLetter)
            return Result<JobDto>.Failure("InvalidStatus", "Only failed jobs can be retried.");
        if (!_retry.ShouldRetry(log.RetryCount))
            return Result<JobDto>.Failure("MaxRetries", "Maximum retry count exceeded.");

        await Task.Delay(_retry.GetDelay(log.RetryCount), cancellationToken);
        await _catalog.ExecuteNamedJobAsync(request.TenantId, log.JobName, cancellationToken);

        var updated = await _context.JobExecutionLogs.AsNoTracking()
            .Where(j => j.TenantId == request.TenantId && j.JobName == log.JobName)
            .OrderByDescending(j => j.CreatedAt)
            .FirstAsync(cancellationToken);

        return Result<JobDto>.Success(ExecuteJobCommandHandler.Map(updated));
    }
}

public sealed class CancelJobCommandHandler : IRequestHandler<CancelJobCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public CancelJobCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result> Handle(CancelJobCommand request, CancellationToken cancellationToken)
    {
        var log = await _context.JobExecutionLogs
            .FirstOrDefaultAsync(j => j.Id == request.JobLogId && j.TenantId == request.TenantId, cancellationToken);
        if (log is null) return Result.Failure("NotFound", "Job log not found.");
        if (log.Status is JobExecutionStatus.Succeeded or JobExecutionStatus.Cancelled)
            return Result.Failure("InvalidStatus", "Job cannot be cancelled.");

        log.Cancel();
        _context.JobExecutionLogs.Update(log);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class SendNotificationCommandHandler : IRequestHandler<SendNotificationCommand, Result<NotificationDto>>
{
    private readonly INotificationOrchestrator _orchestrator;

    public SendNotificationCommandHandler(INotificationOrchestrator orchestrator) => _orchestrator = orchestrator;

    public async Task<Result<NotificationDto>> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        var dto = await _orchestrator.SendAsync(request.TenantId, request.Dto, cancellationToken);
        return Result<NotificationDto>.Success(dto);
    }
}

public sealed class ResendNotificationCommandHandler : IRequestHandler<ResendNotificationCommand, Result<NotificationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationOrchestrator _orchestrator;

    public ResendNotificationCommandHandler(IApplicationDbContext context, INotificationOrchestrator orchestrator)
        => (_context, _orchestrator) = (context, orchestrator);

    public async Task<Result<NotificationDto>> Handle(ResendNotificationCommand request, CancellationToken cancellationToken)
    {
        var msg = await _context.NotificationMessages
            .FirstOrDefaultAsync(n => n.Id == request.NotificationId && n.TenantId == request.TenantId, cancellationToken);
        if (msg is null) return Result<NotificationDto>.Failure("NotFound", "Notification not found.");

        var dto = await _orchestrator.SendAsync(request.TenantId, new SendNotificationDto(
            msg.Title, msg.Body, msg.Type, msg.Channel), cancellationToken);
        return Result<NotificationDto>.Success(dto);
    }
}

public sealed class MarkNotificationReadCommandHandler : IRequestHandler<MarkNotificationReadCommand, Result>
{
    private readonly INotificationInboxService _inbox;

    public MarkNotificationReadCommandHandler(INotificationInboxService inbox) => _inbox = inbox;

    public async Task<Result> Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        await _inbox.MarkReadAsync(request.NotificationId, cancellationToken);
        return Result.Success();
    }
}

public sealed class ArchiveNotificationCommandHandler : IRequestHandler<ArchiveNotificationCommand, Result>
{
    private readonly INotificationInboxService _inbox;

    public ArchiveNotificationCommandHandler(INotificationInboxService inbox) => _inbox = inbox;

    public async Task<Result> Handle(ArchiveNotificationCommand request, CancellationToken cancellationToken)
    {
        await _inbox.ArchiveAsync(request.NotificationId, cancellationToken);
        return Result.Success();
    }
}

public sealed class UpsertIntegrationCommandHandler : IRequestHandler<UpsertIntegrationCommand, Result<IntegrationDto>>
{
    private readonly IIntegrationRegistryService _registry;

    public UpsertIntegrationCommandHandler(IIntegrationRegistryService registry) => _registry = registry;

    public async Task<Result<IntegrationDto>> Handle(UpsertIntegrationCommand request, CancellationToken cancellationToken)
    {
        var dto = await _registry.UpsertAsync(request.TenantId, request.Dto, cancellationToken);
        return Result<IntegrationDto>.Success(dto);
    }
}

public sealed class TestIntegrationCommandHandler : IRequestHandler<TestIntegrationCommand, Result<IntegrationStatusDto>>
{
    private readonly IIntegrationRegistryService _registry;

    public TestIntegrationCommandHandler(IIntegrationRegistryService registry) => _registry = registry;

    public async Task<Result<IntegrationStatusDto>> Handle(TestIntegrationCommand request, CancellationToken cancellationToken)
    {
        var status = await _registry.TestConnectionAsync(request.TenantId, request.Dto, cancellationToken);
        return Result<IntegrationStatusDto>.Success(status);
    }
}

public sealed class ProcessInboundWebhookCommandHandler : IRequestHandler<ProcessInboundWebhookCommand, Result>
{
    private readonly IInboundWebhookService _webhooks;

    public ProcessInboundWebhookCommandHandler(IInboundWebhookService webhooks) => _webhooks = webhooks;

    public async Task<Result> Handle(ProcessInboundWebhookCommand request, CancellationToken cancellationToken)
    {
        await _webhooks.ProcessAsync(request.TenantId, request.Dto, cancellationToken);
        return Result.Success();
    }
}
