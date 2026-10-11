namespace MonitoringDashboard.Application.Features.Users.DTOs;

public sealed record UserPageResponse(
    IReadOnlyList<UserSummaryResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);