using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitoringDashboard.Application.Features.Alerts;
using MonitoringDashboard.Application.Features.Alerts.DTOs;
using MonitoringDashboard.Domain.Enums;

using MonitoringDashboard.WebAPI.Common;

namespace MonitoringDashboard.WebAPI.Controllers;

[ApiController]
[Route("api/v1/alerts")]
[Authorize(Policy = "alerts.read")]
[Produces("application/json")]
public sealed class AlertsController(IAlertService alerts) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<AlertPageResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AlertPageResponse>> GetPage(
        [FromQuery] int? serverId = null,
        [FromQuery] AlertStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        if (!ApiPagination.IsValid(page, pageSize) || serverId <= 0)
        {
            return BadRequest(ApiPagination.InvalidParameters());
        }

        return Ok(await alerts.GetPageAsync(serverId, status, page, pageSize, cancellationToken));
    }
}