using MonitoringDashboard.Domain.Enums;

namespace MonitoringDashboard.Application.Features.Servers.DTOs;

public sealed record UpdateServerRequest
{
    public required string Name { get; init; }

    public string? IPAddress { get; init; }

    public string? Description { get; init; }

    public ServerStatus Status { get; init; }
}