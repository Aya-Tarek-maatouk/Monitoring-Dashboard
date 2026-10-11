namespace MonitoringDashboard.Application.Features.Users.DTOs;

public sealed record UserSummaryResponse(
    int UserId,
    string UserName,
    string Email,
    DateTimeOffset CreatedAt,
    int RoleId,
    string Role,
    IReadOnlyList<string> Permissions);