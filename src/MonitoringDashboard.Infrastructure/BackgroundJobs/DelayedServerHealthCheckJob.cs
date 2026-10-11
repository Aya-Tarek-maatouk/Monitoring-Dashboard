using Microsoft.Extensions.Logging;
using MonitoringDashboard.Domain.Enums;
using MonitoringDashboard.Domain.Interfaces;

namespace MonitoringDashboard.Infrastructure.BackgroundJobs;

public sealed class DelayedServerHealthCheckJob(
    IServerRepository servers,
    ILogger<DelayedServerHealthCheckJob> logger)
{
    public async Task CheckAsync(int serverId, CancellationToken cancellationToken)
    {
        var server = await servers.GetReadOnlyByIdAsync(serverId, cancellationToken);
        if (server is null)
        {
            return;
        }

        if (server.Status == ServerStatus.Down)
        {
            logger.LogWarning("Server {ServerId} remains down after the delayed five-minute health check", serverId);
        }
        else
        {
            logger.LogInformation("Server {ServerId} recovered before the delayed health check", serverId);
        }
    }
}