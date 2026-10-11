namespace MonitoringDashboard.Application.Features.Users.DTOs;

public sealed record RegisterUserRequest
{
    public required string UserName { get; init; }

    public required string Email { get; init; }

    public required string Password { get; init; }
}