using Microsoft.Extensions.Options;
using MonitoringDashboard.Application.Common.Interfaces;

namespace MonitoringDashboard.Infrastructure.Services;

public sealed class LocalReportFileStore(IOptions<ReportStorageOptions> options) : IReportFileStore
{
    public Task<Stream?> OpenReadAsync(string storedPath, CancellationToken cancellationToken)
    {
        var rootPath = ResolveRootPath();
        var fullPath = Path.GetFullPath(storedPath);
        var rootPrefix = rootPath.EndsWith(Path.DirectorySeparatorChar)
            ? rootPath
            : rootPath + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase) || !File.Exists(fullPath))
        {
            return Task.FromResult<Stream?>(null);
        }

        Stream stream = new FileStream(
            fullPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            useAsync: true);
        return Task.FromResult<Stream?>(stream);
    }

    internal string ResolveRootPath() => Path.GetFullPath(options.Value.Directory, AppContext.BaseDirectory);
}