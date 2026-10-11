namespace MonitoringDashboard.Application.Features.Users.DTOs;

public sealed record AuthenticationResponse(
    int UserId,
    string UserName,
    string Role,
    IReadOnlyList<string> Permissions,
    string AccessToken,
    DateTimeOffset ExpiresAt);