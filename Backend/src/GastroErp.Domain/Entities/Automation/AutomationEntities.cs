using GastroErp.Domain.Common;
using GastroErp.Domain.Enums;

namespace GastroErp.Domain.Entities.Automation;

/// <summary>In-app / queued notification message</summary>
public sealed class NotificationMessage : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid? UserId { get; private set; }
    public string Title { get; private set; }
    public string Body { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public NotificationType Type { get; private set; }
    public NotificationStatus Status { get; private set; }
    public string? ReferenceType { get; private set; }
    public Guid? ReferenceId { get; private set; }
    public DateTimeOffset? SentAt { get; private set; }
    public DateTimeOffset? ReadAt { get; private set; }
    public string? ErrorMessage { get; private set; }

    private NotificationMessage()
    {
        Title = string.Empty;
        Body = string.Empty;
    }

    public static NotificationMessage Create(
        Guid tenantId, string title, string body, NotificationType type,
        NotificationChannel channel = NotificationChannel.InApp,
        Guid? userId = null, string? referenceType = null, Guid? referenceId = null)
    {
        return new NotificationMessage
        {
            TenantId = tenantId,
            UserId = userId,
            Title = title,
            Body = body,
            Type = type,
            Channel = channel,
            Status = NotificationStatus.Pending,
            ReferenceType = referenceType,
            ReferenceId = referenceId
        };
    }

    public void MarkSent() { Status = NotificationStatus.Sent; SentAt = DateTimeOffset.UtcNow; }
    public void MarkFailed(string error) { Status = NotificationStatus.Failed; ErrorMessage = error; }
    public void MarkRead() { Status = NotificationStatus.Read; ReadAt = DateTimeOffset.UtcNow; }
    public void Archive() => Status = NotificationStatus.Archived;
}

/// <summary>Background job execution audit log</summary>
public sealed class JobExecutionLog : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public string JobName { get; private set; }
    public JobQueue Queue { get; private set; }
    public JobExecutionStatus Status { get; private set; }
    public string? ExternalJobId { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? FinishedAt { get; private set; }
    public int RetryCount { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? Payload { get; private set; }

    private JobExecutionLog() { JobName = string.Empty; }

    public static JobExecutionLog Create(Guid tenantId, string jobName, JobQueue queue, string? payload = null, string? externalJobId = null)
    {
        return new JobExecutionLog
        {
            TenantId = tenantId,
            JobName = jobName,
            Queue = queue,
            Status = JobExecutionStatus.Queued,
            Payload = payload,
            ExternalJobId = externalJobId
        };
    }

    public void Start()
    {
        Status = JobExecutionStatus.Running;
        StartedAt = DateTimeOffset.UtcNow;
    }

    public void Succeed()
    {
        Status = JobExecutionStatus.Succeeded;
        FinishedAt = DateTimeOffset.UtcNow;
    }

    public void Fail(string error, bool deadLetter = false)
    {
        Status = deadLetter ? JobExecutionStatus.DeadLetter : JobExecutionStatus.Failed;
        ErrorMessage = error;
        FinishedAt = DateTimeOffset.UtcNow;
        RetryCount++;
    }

    public void Cancel()
    {
        Status = JobExecutionStatus.Cancelled;
        FinishedAt = DateTimeOffset.UtcNow;
    }
}

/// <summary>External integration configuration per tenant</summary>
public sealed class IntegrationConfiguration : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public IntegrationProviderType ProviderType { get; private set; }
    public IntegrationProviderName ProviderName { get; private set; }
    public bool IsActive { get; private set; }
    public string SettingsJson { get; private set; }

    private IntegrationConfiguration() { SettingsJson = "{}"; }

    public static IntegrationConfiguration Create(
        Guid tenantId, IntegrationProviderType type, IntegrationProviderName name, string settingsJson)
    {
        return new IntegrationConfiguration
        {
            TenantId = tenantId,
            ProviderType = type,
            ProviderName = name,
            SettingsJson = settingsJson,
            IsActive = true
        };
    }

    public void UpdateSettings(string settingsJson) => SettingsJson = settingsJson;
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}

/// <summary>Outbound webhook subscription</summary>
public sealed class WebhookSubscription : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public string EventName { get; private set; }
    public string TargetUrl { get; private set; }
    public string Secret { get; private set; }
    public bool IsActive { get; private set; }

    private WebhookSubscription()
    {
        EventName = string.Empty;
        TargetUrl = string.Empty;
        Secret = string.Empty;
    }

    public static WebhookSubscription Create(Guid tenantId, string eventName, string targetUrl, string secret)
    {
        return new WebhookSubscription
        {
            TenantId = tenantId,
            EventName = eventName,
            TargetUrl = targetUrl,
            Secret = secret,
            IsActive = true
        };
    }

    public void Update(string targetUrl, string secret, bool isActive)
    {
        TargetUrl = targetUrl;
        Secret = secret;
        IsActive = isActive;
    }
}

/// <summary>Inbound external event idempotency log</summary>
public sealed class ExternalEventLog : AuditableBaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public string Provider { get; private set; }
    public string ExternalEventId { get; private set; }
    public string Payload { get; private set; }
    public DateTimeOffset ReceivedAt { get; private set; }
    public bool Processed { get; private set; }

    private ExternalEventLog()
    {
        Provider = string.Empty;
        ExternalEventId = string.Empty;
        Payload = string.Empty;
    }

    public static ExternalEventLog Create(Guid tenantId, string provider, string externalEventId, string payload)
    {
        return new ExternalEventLog
        {
            TenantId = tenantId,
            Provider = provider,
            ExternalEventId = externalEventId,
            Payload = payload,
            ReceivedAt = DateTimeOffset.UtcNow
        };
    }

    public void MarkProcessed() => Processed = true;
}
