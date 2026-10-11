namespace MonitoringDashboard.Application.Features.Jobs.DTOs;

public sealed record BackgroundJobPageResponse(
    IReadOnlyList<BackgroundJobResponse> Items,
    int Page,
    int PageSize,
    long TotalCount,
    BackgroundJobCounts Counts,
    DateTimeOffset SampledAt);