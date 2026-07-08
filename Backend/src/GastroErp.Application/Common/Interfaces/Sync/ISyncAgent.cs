namespace GastroErp.Application.Common.Interfaces.Sync;

public interface ISyncAgent
{
    Task SyncToServerAsync(CancellationToken cancellationToken = default);
    Task SyncFromServerAsync(CancellationToken cancellationToken = default);
}
