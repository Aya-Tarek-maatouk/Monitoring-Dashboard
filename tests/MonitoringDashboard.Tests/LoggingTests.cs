using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MonitoringDashboard.WebAPI;

namespace MonitoringDashboard.Tests;

public sealed class LoggingTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DashboardLogging_WritesStructuredDailyFilesOnlyWhenEnabled(bool enabled)
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        try
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                EnvironmentName = "Testing"
            });
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["FileLogging:Enabled"] = enabled.ToString(),
                ["FileLogging:Path"] = Path.Combine(directory, "monitoring-.json"),
                ["FileLogging:RetainedFileCountLimit"] = "14",
                ["FileLogging:FileSizeLimitBytes"] = "10485760"
            });
            builder.AddDashboardLogging();
            using (var app = builder.Build())
            {
                app.Services.GetRequiredService<ILogger<LoggingTests>>()
                    .LogInformation("Collected metrics for server {ServerId}", 42);
            }

            if (!enabled)
            {
                Assert.False(Directory.Exists(directory));
                return;
            }

            var file = Assert.Single(Directory.GetFiles(directory, "monitoring-*.json"));
            Assert.Contains(DateTime.Now.ToString("yyyyMMdd"), Path.GetFileName(file));
            var events = File.ReadAllLines(file).Select(line => JsonDocument.Parse(line)).ToArray();
            try
            {
                Assert.Contains(events, entry => entry.RootElement.TryGetProperty("Properties", out var properties)
                    && properties.TryGetProperty("ServerId", out var serverId) && serverId.GetInt32() == 42);
            }
            finally
            {
                foreach (var entry in events) entry.Dispose();
            }
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }
}