using Microsoft.AspNetCore.SignalR;
using MonitoringDashboard.Application.Common.Interfaces;
using MonitoringDashboard.Application.Features.Alerts.DTOs;
using MonitoringDashboard.Application.Features.Metrics.DTOs;
using MonitoringDashboard.WebAPI.Hubs;

namespace MonitoringDashboard.WebAPI.Services;

public sealed class SignalRMonitoringUpdatePublisher(
    IHubContext<MonitoringHub> hubContext) : IMonitoringUpdatePublisher
{
    public Task PublishMetricAsync(int serverId, MetricResponse metric, CancellationToken cancellationToken) =>
        hubContext.Clients.Group(MonitoringHub.GetServerGroupName(serverId))
            .SendAsync("MetricUpdated", metric, cancellationToken);

    public Task PublishAlertRaisedAsync(int serverId, AlertResponse alert, CancellationToken cancellationToken) =>
        hubContext.Clients.Group(MonitoringHub.GetServerGroupName(serverId))
            .SendAsync("AlertRaised", alert, cancellationToken);

    public Task PublishAlertResolvedAsync(int serverId, AlertResponse alert, CancellationToken cancellationToken) =>
        hubContext.Clients.Group(MonitoringHub.GetServerGroupName(serverId))
            .SendAsync("AlertResolved", alert, cancellationToken);
}