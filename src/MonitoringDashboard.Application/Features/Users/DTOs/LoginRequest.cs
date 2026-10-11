namespace MonitoringDashboard.Application.Features.Users.DTOs;

public sealed record LoginRequest
{
    public required string UserNameOrEmail { get; init; }

    public required string Password { get; init; }
}