using MonitoringDashboard.Domain.Enums;

namespace MonitoringDashboard.Application.Features.Reports.DTOs;

public sealed record ReportResponse(
    int ReportId,
    int ServerId,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    ReportStatus Status,
    bool HasDownload,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt);