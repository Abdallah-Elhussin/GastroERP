using GastroErp.Application.Common.Interfaces.Sync;
using Microsoft.Extensions.Logging;

namespace GastroErp.Infrastructure.Sync;

public class SyncDispatcher : ISyncDispatcher
{
    private readonly ILogger<SyncDispatcher> _logger;

    public SyncDispatcher(ILogger<SyncDispatcher> logger)
    {
        _logger = logger;
    }

    public Task DispatchAsync(object syncPayload, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Dispatching sync payload to target system (Skeleton)");
        return Task.CompletedTask;
    }
}
