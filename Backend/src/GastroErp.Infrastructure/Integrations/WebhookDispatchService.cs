using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Features.Automation.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GastroErp.Infrastructure.Integrations;

public sealed class WebhookDispatchService : IWebhookDispatchService
{
    private readonly IApplicationDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WebhookDispatchService> _logger;

    public WebhookDispatchService(
        IApplicationDbContext context, IHttpClientFactory httpClientFactory,
        ILogger<WebhookDispatchService> logger)
        => (_context, _httpClientFactory, _logger) = (context, httpClientFactory, logger);

    public async Task DispatchAsync(Guid tenantId, string eventName, object payload, CancellationToken ct = default)
    {
        var subs = await _context.WebhookSubscriptions.AsNoTracking()
            .Where(w => w.TenantId == tenantId && w.IsActive && w.EventName == eventName)
            .ToListAsync(ct);

        var json = System.Text.Json.JsonSerializer.Serialize(payload);
        var client = _httpClientFactory.CreateClient("Webhooks");

        foreach (var sub in subs)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, sub.TargetUrl);
                request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                request.Headers.Add("X-Webhook-Event", eventName);
                await client.SendAsync(request, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Webhook dispatch failed for {Url}", sub.TargetUrl);
            }
        }
    }
}
