using Microsoft.EntityFrameworkCore;
using MonitoringDashboard.Domain.Interfaces;
using MonitoringDashboard.Domain.Entities;

namespace MonitoringDashboard.Infrastructure.Persistence.Repositories;

public sealed class ReportRepository(ApplicationDbContext context) : IReportRepository
{
    public async Task<(IReadOnlyList<Report> Items, int TotalCount)> GetPageAsync(
        int? serverId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = context.Reports.AsNoTracking();
        if (serverId.HasValue)
        {
            query = query.Where(report => report.ServerId == serverId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(report => report.CreatedAt)
            .ThenByDescending(report => report.ReportId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<Report?> GetByIdAsync(int reportId, CancellationToken cancellationToken) =>
        context.Reports.SingleOrDefaultAsync(report => report.ReportId == reportId, cancellationToken);

    public Task<Report?> GetReadOnlyByIdAsync(int reportId, CancellationToken cancellationToken) =>
        context.Reports.AsNoTracking().SingleOrDefaultAsync(report => report.ReportId == reportId, cancellationToken);

    public Task<bool> HasForServerAsync(int serverId, CancellationToken cancellationToken) =>
        context.Reports.AnyAsync(report => report.ServerId == serverId, cancellationToken);

    public void Add(Report report) => context.Reports.Add(report);
}