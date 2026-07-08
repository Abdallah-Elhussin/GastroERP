using GastroErp.Domain.Enums;

namespace GastroErp.Application.Features.Automation.DTOs;

public record JobDto(
    Guid Id, string JobName, JobQueue Queue, JobExecutionStatus Status,
    string? ExternalJobId, DateTimeOffset? StartedAt, DateTimeOffset? FinishedAt,
    int RetryCount, string? ErrorMessage);

public record JobHistoryDto(
    Guid Id, string JobName, JobExecutionStatus Status,
    DateTimeOffset CreatedAt, DateTimeOffset? FinishedAt, string? ErrorMessage);

public record QueueStatusDto(JobQueue Queue, int Queued, int Running, int Failed);

public record JobMonitoringDto(
    int RunningJobs, int FailedJobs, int QueuedJobs, IReadOnlyList<QueueStatusDto> Queues);

public record ExecuteJobDto(string JobName, Guid? TenantId = null, string? Payload = null);

public record NotificationDto(
    Guid Id, string Title, string Body, NotificationType Type,
    NotificationChannel Channel, NotificationStatus Status,
    DateTimeOffset CreatedAt, DateTimeOffset? ReadAt);

public record SendNotificationDto(
    string Title, string Body, NotificationType Type,
    NotificationChannel Channel = NotificationChannel.InApp,
    Guid? UserId = null, string? ReferenceType = null, Guid? ReferenceId = null);

public record NotificationFilterDto(
    NotificationStatus? Status = null, int Page = 1, int PageSize = 50);

public record IntegrationDto(
    Guid Id, IntegrationProviderType ProviderType, IntegrationProviderName ProviderName,
    bool IsActive, DateTimeOffset UpdatedAt);

public record UpsertIntegrationDto(
    IntegrationProviderType ProviderType, IntegrationProviderName ProviderName,
    string SettingsJson, bool IsActive = true);

public record TestIntegrationDto(IntegrationProviderType ProviderType, IntegrationProviderName ProviderName);

public record IntegrationStatusDto(
    IntegrationProviderName ProviderName, bool IsConfigured, bool IsHealthy, string? Message);

public record WebhookSubscriptionDto(
    Guid Id, string EventName, string TargetUrl, bool IsActive);

public record CreateWebhookDto(string EventName, string TargetUrl, string Secret);

public record InboundWebhookDto(string Provider, string EventId, string Payload);
