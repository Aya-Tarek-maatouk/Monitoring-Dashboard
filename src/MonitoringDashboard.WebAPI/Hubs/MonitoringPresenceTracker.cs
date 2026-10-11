namespace MonitoringDashboard.WebAPI.Hubs;

public sealed class MonitoringPresenceTracker
{
    private readonly object _gate = new();
    private readonly Dictionary<int, HashSet<string>> _connectionsByServer = [];
    private readonly Dictionary<string, HashSet<int>> _serversByConnection = [];

    public int Join(int serverId, string connectionId)
    {
        lock (_gate)
        {
            if (!_connectionsByServer.TryGetValue(serverId, out var connections))
            {
                connections = [];
                _connectionsByServer.Add(serverId, connections);
            }

            if (!_serversByConnection.TryGetValue(connectionId, out var servers))
            {
                servers = [];
                _serversByConnection.Add(connectionId, servers);
            }

            connections.Add(connectionId);
            servers.Add(serverId);
            return connections.Count;
        }
    }

    public int? Leave(int serverId, string connectionId)
    {
        lock (_gate)
        {
            if (!_connectionsByServer.TryGetValue(serverId, out var connections)
                || !connections.Remove(connectionId))
            {
                return null;
            }

            if (_serversByConnection.TryGetValue(connectionId, out var servers))
            {
                servers.Remove(serverId);
                if (servers.Count == 0)
                {
                    _serversByConnection.Remove(connectionId);
                }
            }

            if (connections.Count == 0)
            {
                _connectionsByServer.Remove(serverId);
                return 0;
            }

            return connections.Count;
        }
    }

    public IReadOnlyList<(int ServerId, int ConnectionCount)> RemoveConnection(string connectionId)
    {
        lock (_gate)
        {
            if (!_serversByConnection.Remove(connectionId, out var servers))
            {
                return [];
            }

            var updates = new List<(int ServerId, int ConnectionCount)>(servers.Count);
            foreach (var serverId in servers)
            {
                if (!_connectionsByServer.TryGetValue(serverId, out var connections))
                {
                    continue;
                }

                connections.Remove(connectionId);
                if (connections.Count == 0)
                {
                    _connectionsByServer.Remove(serverId);
                }

                updates.Add((serverId, connections.Count));
            }

            return updates;
        }
    }
}