using Microsoft.AspNetCore.Mvc;
using MonitoringDashboard.Application.Common.Resources;

namespace MonitoringDashboard.WebAPI.Common;

internal static class ApiPagination
{
    public static bool IsValid(int page, int pageSize) => page > 0 && pageSize is > 0 and <= 100;

    public static ProblemDetails InvalidParameters() => new()
    {
        Status = StatusCodes.Status400BadRequest,
        Title = ValidationMessages.InvalidPaginationTitle,
        Detail = ValidationMessages.InvalidPaginationDetails
    };
}