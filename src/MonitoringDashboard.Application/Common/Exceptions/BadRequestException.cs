namespace MonitoringDashboard.Application.Common.Exceptions;

public sealed class BadRequestException(string message) : BusinessException(message);