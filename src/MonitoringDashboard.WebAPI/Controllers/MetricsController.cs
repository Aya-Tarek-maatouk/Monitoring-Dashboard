using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitoringDashboard.Application.Features.Metrics;
using MonitoringDashboard.Application.Features.Metrics.DTOs;

using MonitoringDashboard.WebAPI.Common;

namespace MonitoringDashboard.WebAPI.Controllers;

[ApiController]
[Route("api/v1/servers/{serverId:int}/metrics")]
[Authorize(Policy = "metrics.read")]
[Produces("application/json")]
public sealed class MetricsController(IMetricService metrics) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<MetricHistoryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MetricHistoryResponse>> GetHistory(
        int serverId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100,
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null,
        CancellationToken cancellationToken = default)
    {
        if (!ApiPagination.IsValid(page, pageSize) || (from.HasValue && to.HasValue && from > to))
        {
            return BadRequest(ApiPagination.InvalidParameters());
        }

        var result = await metrics.GetHistoryAsync(serverId, page, pageSize, from, to, cancellationToken);
        return Ok(result);
    }

}