using MonitoringDashboard.Application.Features.Alerts.DTOs;
using MonitoringDashboard.Application.Features.Metrics.DTOs;

namespace MonitoringDashboard.Application.Common.Interfaces;

public interface IMonitoringUpdatePublisher
{
    Task PublishMetricAsync(int serverId, MetricResponse metric, CancellationToken cancellationToken);
    Task PublishAlertRaisedAsync(int serverId, AlertResponse alert, CancellationToken cancellationToken);
    Task PublishAlertResolvedAsync(int serverId, AlertResponse alert, CancellationToken cancellationToken);
}