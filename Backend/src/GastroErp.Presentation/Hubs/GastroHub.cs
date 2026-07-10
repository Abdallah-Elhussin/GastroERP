using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace GastroErp.Presentation.Hubs;

[Authorize]
public sealed class GastroHub : Hub
{
    public override Task OnConnectedAsync()
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, "kitchen");
    }
}
