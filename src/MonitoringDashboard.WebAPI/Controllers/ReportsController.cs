using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitoringDashboard.Application.Features.Reports;
using MonitoringDashboard.Application.Features.Reports.DTOs;

using MonitoringDashboard.WebAPI.Common;

namespace MonitoringDashboard.WebAPI.Controllers;

[ApiController]
[Route("api/v1")]
[Authorize(Policy = "reports.read")]
public sealed class ReportsController(IReportService reports) : ControllerBase
{
    [HttpPost("servers/{serverId:int}/reports")]
    [Authorize(Policy = "reports.create")]
    [ProducesResponseType<ReportResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReportResponse>> Create(
        int serverId,
        CreateReportRequest request,
        CancellationToken cancellationToken)
    {
        var report = await reports.RequestAsync(serverId, request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, report);
    }

    [HttpGet("reports")]
    [ProducesResponseType<ReportPageResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ReportPageResponse>> GetPage(
        [FromQuery] int? serverId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        if (!ApiPagination.IsValid(page, pageSize) || serverId <= 0)
        {
            return BadRequest(ApiPagination.InvalidParameters());
        }

        return Ok(await reports.GetPageAsync(serverId, page, pageSize, cancellationToken));
    }

    [HttpGet("reports/{reportId:int}")]
    [ProducesResponseType<ReportResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReportResponse>> GetById(int reportId, CancellationToken cancellationToken)
    {
        var report = await reports.GetByIdAsync(reportId, cancellationToken);
        return Ok(report);
    }

    [HttpGet("reports/{reportId:int}/download")]
    [Produces("text/csv")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Download(int reportId, CancellationToken cancellationToken)
    {
        var download = await reports.OpenDownloadAsync(reportId, cancellationToken);
        return File(download.Content, "text/csv", download.FileName);
    }
}