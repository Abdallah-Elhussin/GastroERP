using GastroErp.Application.Common.Interfaces;
using GastroErp.Application.Common.Notifications;
using GastroErp.Domain.Common;
using MediatR;

namespace GastroErp.Application.Common.Messaging;

public sealed class MediatRDomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IPublisher _publisher;

    public MediatRDomainEventDispatcher(IPublisher publisher) => _publisher = publisher;

    public async Task DispatchEventsAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in events)
        {
            var wrapperType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
            var notification = Activator.CreateInstance(wrapperType, domainEvent);
            if (notification is not null)
                await _publisher.Publish(notification, cancellationToken);
        }
    }
}
