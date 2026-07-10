using GastroErp.Application.Features.Sales.DTOs;

namespace GastroErp.Application.Common.Interfaces.Realtime;

public interface IKitchenRealtimeNotifier
{
    Task NotifyTicketCreatedAsync(KdsTicketViewDto ticket, CancellationToken cancellationToken = default);
    Task NotifyTicketUpdatedAsync(KdsTicketViewDto ticket, CancellationToken cancellationToken = default);
    Task NotifyTicketRemovedAsync(Guid ticketId, CancellationToken cancellationToken = default);
}
