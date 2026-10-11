using MonitoringDashboard.Application.Features.Servers.DTOs;

namespace MonitoringDashboard.Application.Features.Servers;

public interface IServerService
{
    Task<ServerPageResponse> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<ServerResponse> GetByIdAsync(int serverId, CancellationToken cancellationToken);
    Task<ServerResponse> CreateAsync(CreateServerRequest request, CancellationToken cancellationToken);
    Task<ServerResponse> UpdateAsync(int serverId, UpdateServerRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(int serverId, CancellationToken cancellationToken);
}