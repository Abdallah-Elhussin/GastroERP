namespace GastroErp.Application.Common.Interfaces.Sync;

public interface ISyncDispatcher
{
    Task DispatchAsync(object syncPayload, CancellationToken cancellationToken = default);
}
