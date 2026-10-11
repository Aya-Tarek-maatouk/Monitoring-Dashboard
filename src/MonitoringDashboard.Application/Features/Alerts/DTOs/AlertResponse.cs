using MonitoringDashboard.Domain.Enums;

namespace MonitoringDashboard.Application.Features.Alerts.DTOs;

public sealed record AlertResponse(
    int AlertId,
    int ServerId,
    string MetricType,
    double MetricValue,
    double Threshold,
    AlertStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ResolvedAt);