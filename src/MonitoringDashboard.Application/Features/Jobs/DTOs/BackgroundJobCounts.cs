namespace MonitoringDashboard.Application.Features.Jobs.DTOs;

public sealed record BackgroundJobCounts(
    long Enqueued,
    long Scheduled,
    long Processing,
    long Succeeded,
    long Failed);