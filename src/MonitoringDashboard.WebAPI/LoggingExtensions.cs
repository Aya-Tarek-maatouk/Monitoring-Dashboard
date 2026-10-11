using Serilog;
using Serilog.Formatting.Json;

namespace MonitoringDashboard.WebAPI;

public static class LoggingExtensions
{
    public static WebApplicationBuilder AddDashboardLogging(this WebApplicationBuilder builder)
    {
        builder.Services.AddSerilog((services, logger) =>
        {
            logger.ReadFrom.Configuration(builder.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext();

            if (builder.Configuration.GetValue<bool>("FileLogging:Enabled"))
            {
                var path = builder.Configuration["FileLogging:Path"] ?? "logs/monitoring-.json";
                logger.WriteTo.File(new JsonFormatter(),
                    Path.GetFullPath(path, builder.Environment.ContentRootPath),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: builder.Configuration.GetValue("FileLogging:RetainedFileCountLimit", 14),
                    fileSizeLimitBytes: builder.Configuration.GetValue<long>("FileLogging:FileSizeLimitBytes", 10485760),
                    rollOnFileSizeLimit: true);
            }
        });
        return builder;
    }
}