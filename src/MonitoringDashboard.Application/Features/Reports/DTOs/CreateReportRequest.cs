namespace MonitoringDashboard.Application.Features.Reports.DTOs;

public sealed record CreateReportRequest
{
    public required DateTimeOffset StartTime { get; init; }
    public required DateTimeOffset EndTime { get; init; }
}