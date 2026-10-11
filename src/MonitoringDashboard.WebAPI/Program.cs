using Hangfire;
using MonitoringDashboard.Application;
using MonitoringDashboard.Infrastructure;
using MonitoringDashboard.Infrastructure.BackgroundJobs;
using MonitoringDashboard.WebAPI;
using MonitoringDashboard.WebAPI.Hubs;
using MonitoringDashboard.WebAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.AddDashboardLogging();

var metricCollectionInterval = builder.Configuration.GetValue("Jobs:MetricsCollectionIntervalMinutes", 1);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddDashboardBackgroundJobs(builder.Configuration);
builder.Services.AddOpenApi();
builder.Services.AddDashboardControllers();
builder.Services.AddHealthChecks();
builder.Services.AddDashboardAuthentication(builder.Configuration);
builder.Services.AddDashboardAuthorization();
builder.Services.AddDashboardRateLimiting();
builder.Services.AddDashboardRealtime();
builder.Services.AddDashboardOptions(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseRateLimiter();


app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "swagger";
    options.SwaggerEndpoint("/openapi/v1.json", "Monitoring Dashboard API v1");
});
app.UseHangfireDashboard("/hangfire");


RecurringJob.AddOrUpdate<MetricsCollectionJob>(
    "metrics-collection",
    job => job.ExecuteAsync(CancellationToken.None),
    Cron.MinuteInterval(metricCollectionInterval));

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<MonitoringHub>("/hubs/monitoring").RequireAuthorization("servers.read");

app.Run();

public partial class Program;
