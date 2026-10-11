using MonitoringDashboard.Domain.Enums;
using MonitoringDashboard.Application.Features.Alerts.DTOs;

namespace MonitoringDashboard.Application.Features.Alerts;

public interface IAlertService
{
    Task<AlertPageResponse> GetPageAsync(
        int? serverId,
        AlertStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}