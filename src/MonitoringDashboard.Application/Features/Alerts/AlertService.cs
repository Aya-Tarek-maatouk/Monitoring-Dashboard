using MonitoringDashboard.Application.Features.Alerts.DTOs;
using MonitoringDashboard.Domain.Enums;
using MonitoringDashboard.Domain.Interfaces;

namespace MonitoringDashboard.Application.Features.Alerts;

public sealed class AlertService(IAlertRepository alerts) : IAlertService
{
    public async Task<AlertPageResponse> GetPageAsync(
        int? serverId,
        AlertStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await alerts.GetPageAsync(serverId, status, page, pageSize, cancellationToken);

        var responses = items.Select(alert => new AlertResponse(
            alert.AlertId,
            alert.ServerId,
            alert.MetricType,
            alert.MetricValue,
            alert.Threshold,
            alert.Status,
            new DateTimeOffset(DateTime.SpecifyKind(alert.CreatedAt, DateTimeKind.Utc)),
            alert.ResolvedAt.HasValue
                ? new DateTimeOffset(DateTime.SpecifyKind(alert.ResolvedAt.Value, DateTimeKind.Utc))
                : null)).ToArray();

        return new AlertPageResponse(responses, page, pageSize, totalCount);
    }
}