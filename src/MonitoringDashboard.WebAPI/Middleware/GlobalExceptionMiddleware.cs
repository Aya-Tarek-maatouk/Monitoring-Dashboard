using Microsoft.AspNetCore.Mvc;
using MonitoringDashboard.Application.Common.Exceptions;
using MonitoringDashboard.Application.Common.Resources;

namespace MonitoringDashboard.WebAPI.Middleware;

public sealed class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
            {
                throw;
            }

            var status = exception switch
            {
                FluentValidation.ValidationException => StatusCodes.Status400BadRequest,
                BadRequestException => StatusCodes.Status400BadRequest,
                UnauthorizedException => StatusCodes.Status401Unauthorized,
                NotFoundException => StatusCodes.Status404NotFound,
                ConflictException => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };
            if (status == StatusCodes.Status500InternalServerError)
            {
                logger.LogError(exception, "Unhandled exception while processing {Method} {Path}", context.Request.Method, context.Request.Path);
            }

            context.Response.Clear();
            context.Response.StatusCode = status;
            var problem = new ProblemDetails
            {
                Status = status,
                Title = status switch
                {
                    400 => ValidationMessages.InvalidRequest,
                    401 => ValidationMessages.AuthenticationFailed,
                    404 => ValidationMessages.ResourceNotFound,
                    409 => ValidationMessages.Conflict,
                    _ => ValidationMessages.UnexpectedError
                },
                Detail = exception is FluentValidation.ValidationException ? ValidationMessages.InvalidRequest
                    : status < 500 ? exception.Message : ValidationMessages.TryAgainLater,
                Instance = context.Request.Path,
                Extensions = { ["traceId"] = context.TraceIdentifier }
            };
            if (exception is FluentValidation.ValidationException validationException)
            {
                problem.Extensions["errors"] = validationException.Errors.GroupBy(failure => failure.PropertyName)
                    .ToDictionary(group => group.Key, group => group.Select(failure => failure.ErrorMessage).Distinct().ToArray());
            }
            await context.Response.WriteAsJsonAsync(problem, options: (System.Text.Json.JsonSerializerOptions?)null,
                contentType: "application/problem+json", cancellationToken: context.RequestAborted);
        }
    }
}