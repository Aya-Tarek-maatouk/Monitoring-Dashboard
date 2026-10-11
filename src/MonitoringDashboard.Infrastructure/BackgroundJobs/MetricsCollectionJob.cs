using Microsoft.Extensions.Logging;
using MonitoringDashboard.Application.Features.Metrics;
using MonitoringDashboard.Application.Features.Metrics.DTOs;
using MonitoringDashboard.Domain.Enums;
using MonitoringDashboard.Domain.Interfaces;

namespace MonitoringDashboard.Infrastructure.BackgroundJobs;

public sealed class MetricsCollectionJob(
    IServerRepository servers,
    IMetricService metricService,
    ILogger<MetricsCollectionJob> logger)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var serverIds = await servers.GetServerIdsAsync(cancellationToken);

        foreach (var serverId in serverIds)
        {
            var sample = new RecordMetricRequest
            {
                CpuUsage = Random.Shared.NextDouble() * 100,
                MemoryUsage = Random.Shared.NextDouble() * 100,
                DiskUsage = Random.Shared.NextDouble() * 100,
                ResponseTime = Random.Shared.NextDouble() * 500,
                Status = Random.Shared.Next(100) == 0 ? ServerStatus.Down : ServerStatus.Up
            };

            await metricService.RecordAsync(serverId, sample, cancellationToken);
        }

        logger.LogInformation("Collected simulated metrics for {ServerCount} servers", serverIds.Count);
    }
}