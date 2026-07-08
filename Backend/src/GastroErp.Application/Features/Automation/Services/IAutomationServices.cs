using GastroErp.Application.Features.Automation.DTOs;
using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Automation.Services;

public interface IBackgroundJobManager
{
    Task<string> EnqueueAsync(Guid tenantId, string jobName, JobQueue queue, Func<CancellationToken, Task> work, CancellationToken ct = default);
    Task<string> ScheduleAsync(Guid tenantId, string jobName, JobQueue queue, TimeSpan delay, Func<CancellationToken, Task> work, CancellationToken ct = default);
}

public interface IJobHistoryService
{
    Task<IReadOnlyList<JobHistoryDto>> GetHistoryAsync(Guid tenantId, int take = 50, CancellationToken ct = default);
    Task<JobMonitoringDto> GetMonitoringAsync(Guid tenantId, CancellationToken ct = default);
}

public interface IJobMonitoringService
{
    Task<JobMonitoringDto> GetStatusAsync(Guid tenantId, CancellationToken ct = default);
}

public interface IRecurringJobService
{
    void RegisterRecurringJobs();
}

public interface INotificationOrchestrator
{
    Task<NotificationDto> SendAsync(Guid tenantId, SendNotificationDto dto, CancellationToken ct = default);
    Task SendFromTemplateAsync(Guid tenantId, NotificationType type, string language, object model, Guid? userId = null, CancellationToken ct = default);
}

public interface INotificationTemplateService
{
    (string Subject, string Body) Render(NotificationType type, string language, object model);
}

public interface INotificationInboxService
{
    Task<IReadOnlyList<NotificationDto>> GetUserNotificationsAsync(Guid tenantId, Guid userId, NotificationFilterDto filter, CancellationToken ct = default);
    Task MarkReadAsync(Guid notificationId, CancellationToken ct = default);
    Task ArchiveAsync(Guid notificationId, CancellationToken ct = default);
}

public interface IIntegrationRegistryService
{
    Task<IReadOnlyList<IntegrationDto>> GetAllAsync(Guid tenantId, CancellationToken ct = default);
    Task<IntegrationDto> UpsertAsync(Guid tenantId, UpsertIntegrationDto dto, CancellationToken ct = default);
    Task<IntegrationStatusDto> TestConnectionAsync(Guid tenantId, TestIntegrationDto dto, CancellationToken ct = default);
}

public interface IWebhookDispatchService
{
    Task DispatchAsync(Guid tenantId, string eventName, object payload, CancellationToken ct = default);
}

public interface IInboundWebhookService
{
    Task ProcessAsync(Guid tenantId, InboundWebhookDto dto, CancellationToken ct = default);
}

public interface IPaymentGatewayAdapter
{
    IntegrationProviderName Provider { get; }
    Task<bool> TestConnectionAsync(string settingsJson, CancellationToken ct = default);
}

public interface IRetryPolicyService
{
    TimeSpan GetDelay(int retryCount);
    bool ShouldRetry(int retryCount, int maxRetries = 5);
}
