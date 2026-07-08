namespace GastroErp.Application.Common.Interfaces.Sync;

public interface ISyncQueue
{
    Task EnqueueAsync(string entityType, string entityId, string action, object payload, CancellationToken cancellationToken = default);
    Task<IEnumerable<object>> GetPendingItemsAsync(int batchSize = 50, CancellationToken cancellationToken = default);
    Task MarkAsSyncedAsync(string syncItemId, CancellationToken cancellationToken = default);
    Task MarkAsFailedAsync(string syncItemId, string errorReason, CancellationToken cancellationToken = default);
}
