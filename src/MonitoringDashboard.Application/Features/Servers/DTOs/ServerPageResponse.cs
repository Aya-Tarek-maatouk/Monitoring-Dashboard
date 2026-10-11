namespace MonitoringDashboard.Application.Features.Servers.DTOs;

public sealed record ServerPageResponse(
    IReadOnlyList<ServerResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);