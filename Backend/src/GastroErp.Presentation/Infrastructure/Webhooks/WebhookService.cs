namespace GastroErp.Presentation.Infrastructure.Webhooks;

public interface IWebhookService
{
    Task TriggerWebhookAsync(string eventName, object payload, Guid tenantId);
}

public class WebhookService : IWebhookService
{
    private readonly ILogger<WebhookService> _logger;

    public WebhookService(ILogger<WebhookService> logger)
    {
        _logger = logger;
    }

    public Task TriggerWebhookAsync(string eventName, object payload, Guid tenantId)
    {
        // Passive structure
        _logger.LogInformation("Triggering webhook for event {EventName} for tenant {TenantId}", eventName, tenantId);
        return Task.CompletedTask;
    }
}
