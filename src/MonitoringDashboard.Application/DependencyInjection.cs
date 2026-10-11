using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MonitoringDashboard.Application.Features.Alerts;
using MonitoringDashboard.Application.Features.Metrics;
using MonitoringDashboard.Application.Features.Reports;
using MonitoringDashboard.Application.Features.Servers;
using MonitoringDashboard.Application.Features.Users;
using MonitoringDashboard.Application.Features.Roles;

namespace MonitoringDashboard.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddValidatorsFromAssemblyContaining<IRoleService>();
        services.AddScoped<IServerService, ServerService>();
        services.AddScoped<IMetricService, MetricService>();
        services.AddScoped<IAlertService, AlertService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        return services;
    }
}