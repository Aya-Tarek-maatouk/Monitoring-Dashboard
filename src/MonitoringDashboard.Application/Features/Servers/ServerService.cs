using MonitoringDashboard.Application.Common.Exceptions;
using MonitoringDashboard.Application.Common.Interfaces;
using MonitoringDashboard.Application.Common.Resources;
using MonitoringDashboard.Application.Features.Servers.DTOs;
using MonitoringDashboard.Domain.Entities;
using MonitoringDashboard.Domain.Interfaces;

namespace MonitoringDashboard.Application.Features.Servers;

public sealed class ServerService(
    IServerRepository servers,
    IMetricRepository metrics,
    IAlertRepository alerts,
    IReportRepository reports,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider) : IServerService
{
    public async Task<ServerPageResponse> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await servers.GetPageAsync(page, pageSize, cancellationToken);

        return new ServerPageResponse(items.Select(ToResponse).ToArray(), page, pageSize, totalCount);
    }

    public async Task<ServerResponse> GetByIdAsync(int serverId, CancellationToken cancellationToken)
    {
        var server = await servers.GetReadOnlyByIdAsync(serverId, cancellationToken);

        return server is null ? throw new NotFoundException(ValidationMessages.ServerNotFound) : ToResponse(server);
    }

    public async Task<ServerResponse> CreateAsync(
        CreateServerRequest request,
        CancellationToken cancellationToken)
    {
        var server = new Server
        {
            Name = request.Name.Trim(),
            IPAddress = request.IPAddress?.Trim(),
            Description = request.Description?.Trim(),
            Status = request.Status,
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime
        };

        servers.Add(server);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ToResponse(server);
    }

    public async Task<ServerResponse> UpdateAsync(
        int serverId,
        UpdateServerRequest request,
        CancellationToken cancellationToken)
    {
        var server = await servers.GetByIdAsync(serverId, cancellationToken);
        if (server is null)
        {
            throw new NotFoundException(ValidationMessages.ServerNotFound);
        }

        server.Name = request.Name.Trim();
        server.IPAddress = request.IPAddress?.Trim();
        server.Description = request.Description?.Trim();
        server.Status = request.Status;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ToResponse(server);
    }

    public async Task DeleteAsync(int serverId, CancellationToken cancellationToken)
    {
        var server = await servers.GetByIdAsync(serverId, cancellationToken);
        if (server is null)
        {
            throw new NotFoundException(ValidationMessages.ServerNotFound);
        }

        var hasHistory = await metrics.HasForServerAsync(serverId, cancellationToken)
            || await alerts.HasForServerAsync(serverId, cancellationToken)
            || await reports.HasForServerAsync(serverId, cancellationToken);

        if (hasHistory)
        {
            throw new ConflictException(ValidationMessages.ServerHasHistory);
        }

        servers.Remove(server);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static ServerResponse ToResponse(Server server) => new(
        server.ServerId,
        server.Name,
        server.IPAddress,
        server.Status,
        server.Description,
        new DateTimeOffset(DateTime.SpecifyKind(server.CreatedAt, DateTimeKind.Utc)));
}