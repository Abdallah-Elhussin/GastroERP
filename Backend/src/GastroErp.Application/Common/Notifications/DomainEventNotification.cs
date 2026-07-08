using GastroErp.Domain.Common;
using MediatR;

namespace GastroErp.Application.Common.Notifications;

public sealed record DomainEventNotification<TEvent>(TEvent DomainEvent) : INotification
    where TEvent : IDomainEvent;
