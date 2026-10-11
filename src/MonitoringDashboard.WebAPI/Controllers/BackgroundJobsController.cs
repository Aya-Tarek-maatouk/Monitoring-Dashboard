using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitoringDashboard.Application.Common.Exceptions;
using MonitoringDashboard.Application.Common.Resources;
using MonitoringDashboard.Application.Features.Jobs.DTOs;
using MonitoringDashboard.Application.Features.Jobs;

using MonitoringDashboard.WebAPI.Common;

namespace MonitoringDashboard.WebAPI.Controllers;

[ApiController]
[Route("api/v1/jobs")]
public sealed class BackgroundJobsController(IHangfireJobMonitor monitor) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "jobs.read")]
    [ProducesResponseType<BackgroundJobPageResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public ActionResult<BackgroundJobPageResponse> GetPage(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        if (!ApiPagination.IsValid(page, pageSize))
        {
            return BadRequest(ApiPagination.InvalidParameters());
        }

        return Ok(monitor.GetPage(page, pageSize));
    }

    [HttpPost("{jobId}/retry")]
    [Authorize(Policy = "jobs.retry")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public IActionResult RetryFailedJob(string jobId)
    {
        return monitor.RetryFailedJob(jobId) switch
        {
            RetryJobOutcome.Requeued => NoContent(),
            RetryJobOutcome.NotFound => throw new NotFoundException(ValidationMessages.JobNotFound),
            _ => throw new ConflictException(ValidationMessages.JobRetryConflict)
        };
    }
}