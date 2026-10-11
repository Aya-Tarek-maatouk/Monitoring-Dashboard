namespace MonitoringDashboard.Infrastructure.Services;

public sealed class JwtTokenOptions
{
    public string Secret { get; init; } = string.Empty;
    public string Issuer { get; init; } = "MonitoringDashboard";
    public string Audience { get; init; } = "MonitoringDashboard.Client";
    public int ExpirationMinutes { get; init; } = 60;
}