using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MonitoringDashboard.Application.Common.Interfaces;
using MonitoringDashboard.Application.Features.Jobs;
using MonitoringDashboard.Domain.Entities;
using MonitoringDashboard.Domain.Interfaces;
using MonitoringDashboard.Infrastructure.Persistence;
using MonitoringDashboard.Infrastructure.Persistence.Repositories;
using MonitoringDashboard.Infrastructure.Services;

namespace MonitoringDashboard.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDashboardBackgroundJobs(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        services.AddHangfire(options => options.UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings().UseSqlServerStorage(connectionString));
        services.AddHangfireServer();
        services.AddScoped<IDelayedAlertScheduler, HangfireDelayedAlertScheduler>();
        services.AddScoped<IReportJobScheduler, HangfireReportJobScheduler>();
        services.AddSingleton<IHangfireJobMonitor, HangfireJobMonitor>();
        return services;
    }

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IServerRepository, ServerRepository>();
        services.AddScoped<IMetricRepository, MetricRepository>();
        services.AddScoped<IAlertRepository, AlertRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IReportFileStore, LocalReportFileStore>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IUserPasswordService, IdentityUserPasswordService>();
        services.AddScoped<IAccessTokenService, JwtAccessTokenService>();

        return services;
    }
}