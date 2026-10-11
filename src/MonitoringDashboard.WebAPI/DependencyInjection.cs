using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MonitoringDashboard.Application.Common.Interfaces;
using MonitoringDashboard.Application.Common.Resources;
using MonitoringDashboard.Application.Features.Metrics;
using MonitoringDashboard.Infrastructure.Services;
using MonitoringDashboard.WebAPI.Filters;
using MonitoringDashboard.WebAPI.Hubs;
using MonitoringDashboard.WebAPI.Services;

namespace MonitoringDashboard.WebAPI;

public static class DependencyInjection
{
    public static IServiceCollection AddDashboardControllers(this IServiceCollection services)
    {
        services.AddControllers(options => options.Filters.Add<RequestValidationFilter>())
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        return services;
    }

    public static IServiceCollection AddDashboardRealtime(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddSingleton<MonitoringPresenceTracker>();
        services.AddScoped<IMonitoringUpdatePublisher, SignalRMonitoringUpdatePublisher>();
        return services;
    }

    public static IServiceCollection AddDashboardOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MonitoringThresholds>(configuration.GetSection("MonitoringThresholds"));
        services.Configure<ReportStorageOptions>(configuration.GetSection("Reports"));
        services.Configure<JwtTokenOptions>(configuration.GetSection("Jwt"));
        return services;
    }

    public static IServiceCollection AddDashboardAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection("Jwt").Get<JwtTokenOptions>() ?? new JwtTokenOptions();
        services.AddScoped<TokenSessionValidator>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
                ValidateLifetime = true,
                NameClaimType = ClaimTypes.Name,
                RoleClaimType = ClaimTypes.Role
            };
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    var validator = context.HttpContext.RequestServices.GetRequiredService<TokenSessionValidator>();
                    if (context.Principal is null || !await validator.IsCurrentAsync(
                        context.Principal, context.HttpContext.RequestAborted))
                    {
                        context.Fail(ValidationMessages.SessionNoLongerValid);
                    }
                },
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"].ToString();
                    if (!string.IsNullOrEmpty(accessToken) && context.Request.Path.StartsWithSegments("/hubs/monitoring"))
                    {
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        });
        return services;
    }

    public static IServiceCollection AddDashboardAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            foreach (var permission in new[]
            {
                "servers.read", "servers.manage", "metrics.read", "metrics.write",
                "alerts.read", "reports.read", "reports.create", "users.manage", "jobs.read", "jobs.retry"
            })
            {
                options.AddPolicy(permission, policy => policy.RequireClaim("permission", permission));
            }
        });
        return services;
    }

    public static IServiceCollection AddDashboardRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));
        });
        return services;
    }
}