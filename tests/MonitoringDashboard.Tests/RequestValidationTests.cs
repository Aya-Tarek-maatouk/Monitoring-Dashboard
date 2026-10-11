using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using MonitoringDashboard.Application;
using MonitoringDashboard.Application.Common.Resources;
using MonitoringDashboard.Application.Features.Metrics.DTOs;
using MonitoringDashboard.Application.Features.Reports.DTOs;
using MonitoringDashboard.Application.Features.Roles.DTOs;
using MonitoringDashboard.Application.Features.Servers.DTOs;
using MonitoringDashboard.Application.Features.Users.DTOs;
using MonitoringDashboard.Domain.Enums;
using MonitoringDashboard.WebAPI.Filters;
using MonitoringDashboard.WebAPI.Middleware;

namespace MonitoringDashboard.Tests;

public sealed class RequestValidationTests
{
    public static IEnumerable<object[]> InvalidRequests()
    {
        yield return [new CreateRoleRequest { Name = " " }, "Name", ValidationMessages.RoleNameInvalid];
        yield return [new CreateRoleRequest { Name = "Observer", PermissionIds = [-1] }, "PermissionIds[0]", ValidationMessages.PermissionIdsInvalid];
        yield return [new RegisterUserRequest { UserName = "ab", Email = "invalid", Password = "short" }, "UserName", ValidationMessages.UserNameInvalid];
        yield return [new LoginRequest { UserNameOrEmail = "", Password = "" }, "Password", ValidationMessages.LoginPasswordInvalid];
        yield return [new ChangeUserRoleRequest { RoleId = 0 }, "RoleId", ValidationMessages.RoleIdInvalid];
        yield return [new CreateServerRequest { Name = " " }, "Name", ValidationMessages.ServerNameInvalid];
        yield return [new UpdateServerRequest { Name = "server", Status = (ServerStatus)99 }, "Status", ValidationMessages.ServerStatusInvalid];
        yield return [new RecordMetricRequest { CpuUsage = 101, MemoryUsage = 0, DiskUsage = 0, ResponseTime = 0, Status = ServerStatus.Up }, "CpuUsage", ValidationMessages.MetricUsageInvalid];
        yield return [new RecordMetricRequest { CpuUsage = 0, MemoryUsage = 0, DiskUsage = 0, ResponseTime = double.NaN, Status = ServerStatus.Up }, "ResponseTime", ValidationMessages.ResponseTimeInvalid];
        yield return [new CreateReportRequest { StartTime = DateTimeOffset.UnixEpoch, EndTime = DateTimeOffset.UnixEpoch }, "StartTime", ValidationMessages.ReportTimeRangeInvalid];
    }

    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public async Task Validators_RejectInvalidInputWithResourceMessages(object request, string property, string message)
    {
        using var provider = new ServiceCollection().AddApplication().BuildServiceProvider();
        using var scope = provider.CreateScope();
        var validator = (IValidator)scope.ServiceProvider.GetRequiredService(typeof(IValidator<>).MakeGenericType(request.GetType()));

        var result = await validator.ValidateAsync(new ValidationContext<object>(request));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, failure => failure.PropertyName == property && failure.ErrorMessage == message);
    }

    [Fact]
    public async Task FilterAndMiddleware_Return400FieldErrorsWithoutExecutingAction()
    {
        using var provider = new ServiceCollection().AddApplication().BuildServiceProvider();
        using var scope = provider.CreateScope();
        var context = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
        context.Response.Body = new MemoryStream();
        var actionContext = new ActionContext(context, new RouteData(), new ActionDescriptor());
        var executing = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(),
            new Dictionary<string, object?> { ["request"] = new CreateRoleRequest { Name = " " } }, new object());
        var invoked = false;
        ActionExecutionDelegate executeAction = () =>
        {
            invoked = true;
            return Task.FromResult(new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), new object()));
        };
        var middleware = new GlobalExceptionMiddleware(async _ =>
        {
            await new RequestValidationFilter().OnActionExecutionAsync(executing, executeAction);
        }, NullLogger<GlobalExceptionMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.False(invoked);
        Assert.Equal(400, context.Response.StatusCode);
        context.Response.Body.Position = 0;
        using var problem = await JsonDocument.ParseAsync(context.Response.Body);
        Assert.Equal(ValidationMessages.InvalidRequest, problem.RootElement.GetProperty("title").GetString());
        Assert.Equal(ValidationMessages.RoleNameInvalid, problem.RootElement.GetProperty("errors").GetProperty("Name")[0].GetString());
    }

    [Fact]
    public async Task Filter_AllowsValidInputAndUnvalidatedRouteArguments()
    {
        using var provider = new ServiceCollection().AddApplication().BuildServiceProvider();
        using var scope = provider.CreateScope();
        var context = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
        var actionContext = new ActionContext(context, new RouteData(), new ActionDescriptor());
        var executing = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(),
            new Dictionary<string, object?> { ["request"] = new CreateRoleRequest { Name = "Observer" }, ["roleId"] = 17 }, new object());
        var invoked = false;
        await new RequestValidationFilter().OnActionExecutionAsync(executing, () =>
        {
            invoked = true;
            return Task.FromResult(new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), new object()));
        });
        Assert.True(invoked);
    }
}