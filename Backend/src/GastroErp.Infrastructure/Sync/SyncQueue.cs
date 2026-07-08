using GastroErp.Application.Common.Interfaces.Sync;
using Microsoft.Extensions.Logging;

namespace GastroErp.Infrastructure.Sync;

public class SyncQueue : ISyncQueue
{
    private readonly ILogger<SyncQueue> _logger;

    public SyncQueue(ILogger<SyncQueue> logger)
    {
        _logger = logger;
    }

    public Task EnqueueAsync(string entityType, string entityId, string action, object payload, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Enqueuing sync item: {Action} on {EntityType} ({EntityId})", action, entityType, entityId);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<object>> GetPendingItemsAsync(int batchSize = 50, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching pending sync items (Skeleton)");
        return Task.FromResult(Enumerable.Empty<object>());
    }

    public Task MarkAsSyncedAsync(string syncItemId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Marking sync item as synced: {SyncItemId}", syncItemId);
        return Task.CompletedTask;
    }

    public Task MarkAsFailedAsync(string syncItemId, string errorReason, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Marking sync item as failed: {SyncItemId}. Reason: {Reason}", syncItemId, errorReason);
        return Task.CompletedTask;
    }
}
