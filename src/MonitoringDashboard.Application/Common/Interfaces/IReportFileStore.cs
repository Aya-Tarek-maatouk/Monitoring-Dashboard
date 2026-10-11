namespace MonitoringDashboard.Application.Common.Interfaces;

public interface IReportFileStore
{
    Task<Stream?> OpenReadAsync(string storedPath, CancellationToken cancellationToken);
}