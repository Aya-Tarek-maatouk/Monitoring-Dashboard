using MonitoringDashboard.Application.Common.Resources;
using Microsoft.AspNetCore.SignalR;
using MonitoringDashboard.Application.Features.Servers;

namespace MonitoringDashboard.WebAPI.Hubs;

public sealed class MonitoringHub(
    IServerService servers,
    MonitoringPresenceTracker presenceTracker) : Hub
{
    public async Task JoinServer(int serverId)
    {
        var cancellationToken = Context.ConnectionAborted;
        if (await servers.GetByIdAsync(serverId, cancellationToken) is null)
        {
            throw new HubException(ValidationMessages.HubServerNotFound);
        }

        var groupName = GetServerGroupName(serverId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName, cancellationToken);
        var connectionCount = presenceTracker.Join(serverId, Context.ConnectionId);
        await Clients.Group(groupName).SendAsync(
            "PresenceUpdated",
            new { ServerId = serverId, ConnectionCount = connectionCount },
            cancellationToken);
    }

    public async Task LeaveServer(int serverId)
    {
        var cancellationToken = Context.ConnectionAborted;
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetServerGroupName(serverId), cancellationToken);
        var connectionCount = presenceTracker.Leave(serverId, Context.ConnectionId);
        if (connectionCount.HasValue)
        {
            await Clients.Group(GetServerGroupName(serverId)).SendAsync(
                "PresenceUpdated",
                new { ServerId = serverId, ConnectionCount = connectionCount.Value },
                cancellationToken);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        foreach (var (serverId, connectionCount) in presenceTracker.RemoveConnection(Context.ConnectionId))
        {
            await Clients.Group(GetServerGroupName(serverId)).SendAsync(
                "PresenceUpdated",
                new { ServerId = serverId, ConnectionCount = connectionCount });
        }

        await base.OnDisconnectedAsync(exception);
    }

    public static string GetServerGroupName(int serverId) => $"server:{serverId}";
}