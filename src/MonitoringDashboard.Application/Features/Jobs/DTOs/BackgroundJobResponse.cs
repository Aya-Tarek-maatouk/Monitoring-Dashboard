namespace MonitoringDashboard.Application.Features.Jobs.DTOs;

public sealed record BackgroundJobResponse(
    string JobId,
    string JobName,
    string State,
    DateTimeOffset? StateChangedAt,
    string? Error,
    bool CanRetry);