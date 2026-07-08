using GastroErp.Domain.Common;

namespace GastroErp.Application.Common.Interfaces;

public interface IDomainEventDispatcher
{
    Task DispatchEventsAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default);
}
