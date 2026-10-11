namespace MonitoringDashboard.Application.Common.Exceptions;

public sealed class UnauthorizedException(string message) : BusinessException(message);