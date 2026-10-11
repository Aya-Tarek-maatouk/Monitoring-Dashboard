using MonitoringDashboard.Application.Features.Users.DTOs;

namespace MonitoringDashboard.Application.Features.Users;

public interface IUserService
{
    Task<AuthenticationResponse> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken);
    Task<AuthenticationResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<UserPageResponse> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<UserSummaryResponse> ChangeRoleAsync(int userId, ChangeUserRoleRequest request, CancellationToken cancellationToken);
}