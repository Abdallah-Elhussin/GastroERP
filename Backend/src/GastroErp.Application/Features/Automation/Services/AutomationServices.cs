using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Features.Automation.DTOs;
using GastroErp.Application.Features.Reporting.DTOs;
using GastroErp.Application.Features.Reporting.Services;
using GastroErp.Domain.Entities.Automation;
using GastroErp.Domain.Entities.Inventory.Catalog;
using GastroErp.Domain.Entities.Sales;
using GastroErp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Application.Features.Automation.Services;

public sealed class JobHistoryService : IJobHistoryService
{
    private readonly IApplicationDbContext _context;
    public JobHistoryService(IApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<JobHistoryDto>> GetHistoryAsync(Guid tenantId, int take = 50, CancellationToken ct = default)
        => await _context.JobExecutionLogs.AsNoTracking()
            .Where(j => j.TenantId == tenantId)
            .OrderByDescending(j => j.CreatedAt)
            .Take(take)
            .Select(j => new JobHistoryDto(j.Id, j.JobName, j.Status, j.CreatedAt, j.FinishedAt, j.ErrorMessage))
            .ToListAsync(ct);

    public async Task<JobMonitoringDto> GetMonitoringAsync(Guid tenantId, CancellationToken ct = default)
        => await new JobMonitoringService(_context).GetStatusAsync(tenantId, ct);
}

public sealed class JobMonitoringService : IJobMonitoringService
{
    private readonly IApplicationDbContext _context;
    public JobMonitoringService(IApplicationDbContext context) => _context = context;

    public async Task<JobMonitoringDto> GetStatusAsync(Guid tenantId, CancellationToken ct = default)
    {
        var logs = await _context.JobExecutionLogs.AsNoTracking()
            .Where(j => j.TenantId == tenantId && j.CreatedAt >= DateTimeOffset.UtcNow.AddDays(-7))
            .GroupBy(j => new { j.Queue, j.Status })
            .Select(g => new { g.Key.Queue, g.Key.Status, Count = g.Count() })
            .ToListAsync(ct);

        var queues = Enum.GetValues<JobQueue>().Select(q =>
        {
            var queued = logs.Where(l => l.Queue == q && l.Status == JobExecutionStatus.Queued).Sum(l => l.Count);
            var running = logs.Where(l => l.Queue == q && l.Status == JobExecutionStatus.Running).Sum(l => l.Count);
            var failed = logs.Where(l => l.Queue == q && l.Status == JobExecutionStatus.Failed).Sum(l => l.Count);
            return new QueueStatusDto(q, queued, running, failed);
        }).ToList();

        return new JobMonitoringDto(
            logs.Where(l => l.Status == JobExecutionStatus.Running).Sum(l => l.Count),
            logs.Where(l => l.Status == JobExecutionStatus.Failed).Sum(l => l.Count),
            logs.Where(l => l.Status == JobExecutionStatus.Queued).Sum(l => l.Count),
            queues);
    }
}

public sealed class RetryPolicyService : IRetryPolicyService
{
    public TimeSpan GetDelay(int retryCount) => TimeSpan.FromSeconds(Math.Pow(2, retryCount));
    public bool ShouldRetry(int retryCount, int maxRetries = 5) => retryCount < maxRetries;
}

public sealed class NotificationTemplateService : INotificationTemplateService
{
    public (string Subject, string Body) Render(NotificationType type, string language, object model)
    {
        var isAr = language.StartsWith("ar", StringComparison.OrdinalIgnoreCase);
        return type switch
        {
            NotificationType.OrderReady => isAr
                ? ("طلبك جاهز", "طلبك جاهز للاستلام.")
                : ("Order Ready", "Your order is ready for pickup."),
            NotificationType.PaymentReceived => isAr
                ? ("تم استلام الدفع", "تم استلام دفعتك بنجاح.")
                : ("Payment Received", "Your payment was received successfully."),
            NotificationType.LowStock => isAr
                ? ("مخزون منخفض", "يوجد أصناف بمخزون منخفض.")
                : ("Low Stock", "Some items are below reorder level."),
            _ => isAr ? ("إشعار", "لديك إشعار جديد.") : ("Notification", "You have a new notification.")
        };
    }
}

public sealed class NotificationOrchestrator : INotificationOrchestrator
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailSender _email;
    private readonly ISmsSender _sms;
    private readonly INotificationTemplateService _templates;
    private readonly ILogger<NotificationOrchestrator> _logger;

    public NotificationOrchestrator(
        IApplicationDbContext context, IEmailSender email, ISmsSender sms,
        INotificationTemplateService templates, ILogger<NotificationOrchestrator> logger)
        => (_context, _email, _sms, _templates, _logger) = (context, email, sms, templates, logger);

    public async Task<NotificationDto> SendAsync(Guid tenantId, SendNotificationDto dto, CancellationToken ct = default)
    {
        var msg = NotificationMessage.Create(
            tenantId, dto.Title, dto.Body, dto.Type, dto.Channel,
            dto.UserId, dto.ReferenceType, dto.ReferenceId);

        try
        {
            if (dto.Channel == NotificationChannel.Email && dto.UserId.HasValue)
            {
                var user = await _context.AppUsers.AsNoTracking().FirstOrDefaultAsync(u => u.Id == dto.UserId, ct);
                if (user?.Email is not null)
                    await _email.SendEmailAsync(user.Email, dto.Title, dto.Body, ct);
            }
            else if (dto.Channel == NotificationChannel.Sms && dto.UserId.HasValue)
            {
                var user = await _context.AppUsers.AsNoTracking().FirstOrDefaultAsync(u => u.Id == dto.UserId, ct);
                if (user?.PhoneNumber is not null)
                    await _sms.SendSmsAsync(user.PhoneNumber, dto.Body, ct);
            }

            msg.MarkSent();
        }
        catch (Exception ex)
        {
            msg.MarkFailed(ex.Message);
            _logger.LogError(ex, "Failed to send notification {Type}", dto.Type);
        }

        _context.NotificationMessages.Add(msg);
        await _context.SaveChangesAsync(ct);
        return Map(msg);
    }

    public async Task SendFromTemplateAsync(
        Guid tenantId, NotificationType type, string language, object model,
        Guid? userId = null, CancellationToken ct = default)
    {
        var (subject, body) = _templates.Render(type, language, model);
        await SendAsync(tenantId, new SendNotificationDto(subject, body, type, NotificationChannel.InApp, userId), ct);
    }

    private static NotificationDto Map(NotificationMessage m) =>
        new(m.Id, m.Title, m.Body, m.Type, m.Channel, m.Status, m.CreatedAt, m.ReadAt);
}

public sealed class NotificationInboxService : INotificationInboxService
{
    private readonly IApplicationDbContext _context;
    public NotificationInboxService(IApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<NotificationDto>> GetUserNotificationsAsync(
        Guid tenantId, Guid userId, NotificationFilterDto filter, CancellationToken ct = default)
    {
        var query = _context.NotificationMessages.AsNoTracking()
            .Where(n => n.TenantId == tenantId && n.UserId == userId);
        if (filter.Status.HasValue) query = query.Where(n => n.Status == filter.Status);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);
        return await query.OrderByDescending(n => n.CreatedAt)
            .Skip((Math.Max(filter.Page, 1) - 1) * pageSize).Take(pageSize)
            .Select(n => new NotificationDto(n.Id, n.Title, n.Body, n.Type, n.Channel, n.Status, n.CreatedAt, n.ReadAt))
            .ToListAsync(ct);
    }

    public async Task MarkReadAsync(Guid notificationId, CancellationToken ct = default)
    {
        var n = await _context.NotificationMessages.FirstOrDefaultAsync(x => x.Id == notificationId, ct);
        if (n is null) return;
        n.MarkRead();
        _context.NotificationMessages.Update(n);
        await _context.SaveChangesAsync(ct);
    }

    public async Task ArchiveAsync(Guid notificationId, CancellationToken ct = default)
    {
        var n = await _context.NotificationMessages.FirstOrDefaultAsync(x => x.Id == notificationId, ct);
        if (n is null) return;
        n.Archive();
        _context.NotificationMessages.Update(n);
        await _context.SaveChangesAsync(ct);
    }
}

public sealed class IntegrationRegistryService : IIntegrationRegistryService
{
    private readonly IApplicationDbContext _context;
    private readonly IEnumerable<IPaymentGatewayAdapter> _paymentAdapters;

    public IntegrationRegistryService(IApplicationDbContext context, IEnumerable<IPaymentGatewayAdapter> paymentAdapters)
        => (_context, _paymentAdapters) = (context, paymentAdapters);

    public async Task<IReadOnlyList<IntegrationDto>> GetAllAsync(Guid tenantId, CancellationToken ct = default)
        => await _context.IntegrationConfigurations.AsNoTracking()
            .Where(i => i.TenantId == tenantId)
            .Select(i => new IntegrationDto(i.Id, i.ProviderType, i.ProviderName, i.IsActive, i.UpdatedAt ?? i.CreatedAt))
            .ToListAsync(ct);

    public async Task<IntegrationDto> UpsertAsync(Guid tenantId, UpsertIntegrationDto dto, CancellationToken ct = default)
    {
        var existing = await _context.IntegrationConfigurations
            .FirstOrDefaultAsync(i => i.TenantId == tenantId && i.ProviderType == dto.ProviderType && i.ProviderName == dto.ProviderName, ct);

        if (existing is null)
        {
            existing = IntegrationConfiguration.Create(tenantId, dto.ProviderType, dto.ProviderName, dto.SettingsJson);
            _context.IntegrationConfigurations.Add(existing);
        }
        else
        {
            existing.UpdateSettings(dto.SettingsJson);
            if (dto.IsActive) existing.Activate(); else existing.Deactivate();
            _context.IntegrationConfigurations.Update(existing);
        }

        await _context.SaveChangesAsync(ct);
        return new IntegrationDto(existing.Id, existing.ProviderType, existing.ProviderName, existing.IsActive, existing.UpdatedAt ?? existing.CreatedAt);
    }

    public async Task<IntegrationStatusDto> TestConnectionAsync(Guid tenantId, TestIntegrationDto dto, CancellationToken ct = default)
    {
        var config = await _context.IntegrationConfigurations.AsNoTracking()
            .FirstOrDefaultAsync(i => i.TenantId == tenantId && i.ProviderType == dto.ProviderType && i.ProviderName == dto.ProviderName, ct);

        if (config is null) return new IntegrationStatusDto(dto.ProviderName, false, false, "Not configured");

        if (dto.ProviderType == IntegrationProviderType.PaymentGateway)
        {
            var adapter = _paymentAdapters.FirstOrDefault(a => a.Provider == dto.ProviderName);
            if (adapter is null) return new IntegrationStatusDto(dto.ProviderName, true, false, "Adapter not registered");
            var ok = await adapter.TestConnectionAsync(config.SettingsJson, ct);
            return new IntegrationStatusDto(dto.ProviderName, true, ok, ok ? "OK" : "Connection failed");
        }

        return new IntegrationStatusDto(dto.ProviderName, true, true, "OK");
    }
}

public sealed class InboundWebhookService : IInboundWebhookService
{
    private readonly IApplicationDbContext _context;
    public InboundWebhookService(IApplicationDbContext context) => _context = context;

    public async Task ProcessAsync(Guid tenantId, InboundWebhookDto dto, CancellationToken ct = default)
    {
        var exists = await _context.ExternalEventLogs
            .AnyAsync(e => e.TenantId == tenantId && e.Provider == dto.Provider && e.ExternalEventId == dto.EventId, ct);
        if (exists) return;

        var log = ExternalEventLog.Create(tenantId, dto.Provider, dto.EventId, dto.Payload);
        log.MarkProcessed();
        _context.ExternalEventLogs.Add(log);
        await _context.SaveChangesAsync(ct);
    }
}

public sealed class ScheduledJobExecutor
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationOrchestrator _notifications;
    private readonly IDeliveryAnalyticsService _delivery;
    private readonly IKitchenAnalyticsService _kitchen;
    private readonly ILogger<ScheduledJobExecutor> _logger;

    public ScheduledJobExecutor(
        IApplicationDbContext context, INotificationOrchestrator notifications,
        IDeliveryAnalyticsService delivery, IKitchenAnalyticsService kitchen,
        ILogger<ScheduledJobExecutor> logger)
        => (_context, _notifications, _delivery, _kitchen, _logger) = (context, notifications, delivery, kitchen, logger);

    public async Task RunForAllTenantsAsync(string jobName, Func<Guid, CancellationToken, Task> action, CancellationToken ct = default)
    {
        var tenantIds = await _context.Tenants.AsNoTracking().Select(t => t.Id).ToListAsync(ct);
        foreach (var tenantId in tenantIds)
            await RunWithLogAsync(tenantId, jobName, JobQueue.System, action, ct);
    }

    public async Task RunWithLogAsync(
        Guid tenantId, string jobName, JobQueue queue,
        Func<Guid, CancellationToken, Task> action, CancellationToken ct = default)
    {
        var log = JobExecutionLog.Create(tenantId, jobName, queue);
        _context.JobExecutionLogs.Add(log);
        await _context.SaveChangesAsync(ct);

        try
        {
            log.Start();
            _context.JobExecutionLogs.Update(log);
            await _context.SaveChangesAsync(ct);
            await action(tenantId, ct);
            log.Succeed();
        }
        catch (Exception ex)
        {
            log.Fail(ex.Message);
            _logger.LogError(ex, "Job {JobName} failed for tenant {TenantId}", jobName, tenantId);
        }

        _context.JobExecutionLogs.Update(log);
        await _context.SaveChangesAsync(ct);
    }

    public Task AutoCloseFiscalPeriodAsync(Guid tenantId, CancellationToken ct) =>
        RunWithLogAsync(tenantId, "AutoCloseFiscalPeriod", JobQueue.Accounting, async (tid, token) =>
        {
            var periods = await _context.FiscalPeriods
                .Where(p => p.TenantId == tid && p.Status == FiscalPeriodStatus.Open && p.EndDate < DateOnly.FromDateTime(DateTime.UtcNow))
                .ToListAsync(token);
            foreach (var p in periods) { p.Close(); _context.FiscalPeriods.Update(p); }
            await _context.SaveChangesAsync(token);
        }, ct);

    public Task LowStockCheckAsync(Guid tenantId, CancellationToken ct) =>
        RunWithLogAsync(tenantId, "LowStockCheck", JobQueue.Inventory, async (tid, token) =>
        {
            var balances = await _context.StockMovements.AsNoTracking()
                .Where(m => m.TenantId == tid)
                .GroupBy(m => m.InventoryItemId)
                .Select(g => new { ItemId = g.Key, Qty = g.Sum(x => x.QuantityChange) })
                .ToListAsync(token);

            var itemIds = balances.Select(b => b.ItemId).ToList();
            var items = await _context.InventoryItems.AsNoTracking()
                .Where(i => itemIds.Contains(i.Id) && i.IsActive).ToListAsync(token);

            foreach (var item in items)
            {
                var qty = balances.FirstOrDefault(b => b.ItemId == item.Id)?.Qty ?? 0;
                if (qty <= item.ReorderLevel)
                    await _notifications.SendAsync(tid, new SendNotificationDto(
                        "Low Stock", $"{item.NameAr} below reorder level", NotificationType.LowStock), token);
            }
        }, ct);

    public Task LoyaltyPointsExpiryAsync(Guid tenantId, CancellationToken ct) =>
        RunWithLogAsync(tenantId, "LoyaltyPointsExpiry", JobQueue.Notifications, async (tid, token) =>
        {
            var accounts = await _context.LoyaltyAccounts.Where(a => a.TenantId == tid && a.CurrentPoints > 0).ToListAsync(token);
            foreach (var account in accounts)
            {
                var lastTxn = await _context.LoyaltyTransactions.AsNoTracking()
                    .Where(t => t.LoyaltyAccountId == account.Id)
                    .OrderByDescending(t => t.CreatedAt).FirstOrDefaultAsync(token);
                if (lastTxn is not null && lastTxn.CreatedAt < DateTimeOffset.UtcNow.AddYears(-1))
                    account.ExpirePoints(account.CurrentPoints, "Annual expiry");
            }
            await _context.SaveChangesAsync(token);
        }, ct);

    public Task CancelExpiredOrdersAsync(Guid tenantId, CancellationToken ct) =>
        RunWithLogAsync(tenantId, "CancelExpiredOrders", JobQueue.System, async (tid, token) =>
        {
            var cutoff = DateTimeOffset.UtcNow.AddHours(-24);
            var orders = await _context.SalesOrders
                .Where(o => o.TenantId == tid && o.Status == OrderStatus.Pending && o.CreatedAt < cutoff)
                .ToListAsync(token);
            foreach (var o in orders)
            {
                o.Cancel("Auto-cancelled: expired", Guid.Empty, o.DeviceId);
                _context.SalesOrders.Update(o);
            }
            await _context.SaveChangesAsync(token);
        }, ct);

    public Task SyncDeliveryStatusAsync(Guid tenantId, CancellationToken ct) =>
        RunWithLogAsync(tenantId, "SyncDeliveryStatus", JobQueue.System, async (tid, token) =>
        {
            await _delivery.GetDeliverySummaryAsync(tid, new Reporting.DTOs.ReportFilterDto(), token);
        }, ct);

    public Task KitchenDelayedAlertAsync(Guid tenantId, CancellationToken ct) =>
        RunWithLogAsync(tenantId, "KitchenDelayedAlert", JobQueue.Notifications, async (tid, token) =>
        {
            var perf = await _kitchen.GetKitchenPerformanceAsync(tid, new Reporting.DTOs.ReportFilterDto(), token);
            if (perf.DelayedTickets > 0)
                await _notifications.SendAsync(tid, new SendNotificationDto(
                    "Kitchen Delay", $"{perf.DelayedTickets} delayed tickets", NotificationType.System), token);
        }, ct);

    public Task CleanupCacheAsync(Guid tenantId, CancellationToken ct) =>
        RunWithLogAsync(tenantId, "CacheCleanup", JobQueue.System, (_, _) => Task.CompletedTask, ct);
}
