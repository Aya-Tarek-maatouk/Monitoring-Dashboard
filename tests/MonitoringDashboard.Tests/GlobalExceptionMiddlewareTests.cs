using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using MonitoringDashboard.Application.Common.Exceptions;
using MonitoringDashboard.WebAPI.Middleware;

namespace MonitoringDashboard.Tests;

public sealed class GlobalExceptionMiddlewareTests
{
    [Theory]
    [InlineData(400)]
    [InlineData(401)]
    [InlineData(404)]
    [InlineData(409)]
    [InlineData(500)]
    public async Task InvokeAsync_MapsExpectedErrorsAndHidesUnexpectedDetails(int status)
    {
        const string message = "Sensitive internal detail";
        Exception exception = status switch
        {
            400 => new BadRequestException(message),
            401 => new UnauthorizedException(message),
            404 => new NotFoundException(message),
            409 => new ConflictException(message),
            _ => new InvalidOperationException(message)
        };
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.TraceIdentifier = "test-trace";
        var middleware = new GlobalExceptionMiddleware(
            _ => Task.FromException(exception), NullLogger<GlobalExceptionMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal(status, context.Response.StatusCode);
        Assert.StartsWith("application/problem+json", context.Response.ContentType);
        context.Response.Body.Position = 0;
        using var problem = await JsonDocument.ParseAsync(context.Response.Body);
        Assert.Equal(status, problem.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("test-trace", problem.RootElement.GetProperty("traceId").GetString());
        Assert.Equal(status < 500 ? message : "Please try again later.",
            problem.RootElement.GetProperty("detail").GetString());
    }
}