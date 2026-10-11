namespace MonitoringDashboard.Application.Features.Alerts.DTOs;

public sealed record AlertPageResponse(
    IReadOnlyList<AlertResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);