using MonitoringDashboard.Domain.Enums;

namespace MonitoringDashboard.Application.Features.Servers.DTOs;

public sealed record ServerResponse(
    int ServerId,
    string Name,
    string? IPAddress,
    ServerStatus Status,
    string? Description,
    DateTimeOffset CreatedAt);