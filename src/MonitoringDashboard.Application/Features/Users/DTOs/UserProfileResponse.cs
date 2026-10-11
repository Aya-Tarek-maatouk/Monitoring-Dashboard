namespace MonitoringDashboard.Application.Features.Users.DTOs;

public sealed record UserProfileResponse(
    int UserId,
    string UserName,
    string? Email,
    string Role,
    IReadOnlyList<string> Permissions);