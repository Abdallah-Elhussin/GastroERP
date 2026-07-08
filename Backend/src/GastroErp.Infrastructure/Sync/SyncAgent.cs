using GastroErp.Application.Common.Interfaces.Sync;
using Microsoft.Extensions.Logging;

namespace GastroErp.Infrastructure.Sync;

public class SyncAgent : ISyncAgent
{
    private readonly ILogger<SyncAgent> _logger;

    public SyncAgent(ILogger<SyncAgent> logger)
    {
        _logger = logger;
    }

    public Task SyncToServerAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Syncing local changes to server (Skeleton)");
        return Task.CompletedTask;
    }

    public Task SyncFromServerAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Syncing server changes to local DB (Skeleton)");
        return Task.CompletedTask;
    }
}
