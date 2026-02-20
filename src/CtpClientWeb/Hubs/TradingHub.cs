using Microsoft.AspNetCore.SignalR;

namespace CtpClientWeb.Hubs;

public sealed class TradingHub : Hub
{
    public Task JoinAccount(string accountId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, accountId);
    }

    public Task LeaveAccount(string accountId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, accountId);
    }
}
