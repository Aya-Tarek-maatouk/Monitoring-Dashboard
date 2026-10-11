namespace MonitoringDashboard.Application.Features.Reports.DTOs;

public sealed record ReportPageResponse(
    IReadOnlyList<ReportResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);