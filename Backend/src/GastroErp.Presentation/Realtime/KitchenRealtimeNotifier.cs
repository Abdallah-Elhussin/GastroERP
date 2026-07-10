using GastroErp.Application.Common.Interfaces.Realtime;
using GastroErp.Application.Features.Sales.DTOs;
using GastroErp.Presentation.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace GastroErp.Presentation.Realtime;

public sealed class KitchenRealtimeNotifier(IHubContext<GastroHub> hubContext) : IKitchenRealtimeNotifier
{
    public Task NotifyTicketCreatedAsync(KdsTicketViewDto ticket, CancellationToken cancellationToken = default)
        => hubContext.Clients.Group("kitchen").SendAsync("OrderCreated", ticket, cancellationToken);

    public Task NotifyTicketUpdatedAsync(KdsTicketViewDto ticket, CancellationToken cancellationToken = default)
        => hubContext.Clients.Group("kitchen").SendAsync("TicketBumped", new
        {
            ticketId = ticket.Id,
            status = ticket.KdsStatus,
            ticket
        }, cancellationToken);

    public Task NotifyTicketRemovedAsync(Guid ticketId, CancellationToken cancellationToken = default)
        => hubContext.Clients.Group("kitchen").SendAsync("TicketBumped", new
        {
            ticketId,
            status = "completed"
        }, cancellationToken);
}
