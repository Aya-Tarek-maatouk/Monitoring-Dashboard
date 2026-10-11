using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitoringDashboard.Application.Features.Servers;
using MonitoringDashboard.Application.Features.Servers.DTOs;

using MonitoringDashboard.WebAPI.Common;

namespace MonitoringDashboard.WebAPI.Controllers;

[ApiController]
[Route("api/v1/servers")]
[Authorize(Policy = "servers.read")]
[Produces("application/json")]
public sealed class ServersController(IServerService servers) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<ServerPageResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ServerPageResponse>> GetPage(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        if (!ApiPagination.IsValid(page, pageSize))
        {
            return BadRequest(ApiPagination.InvalidParameters());
        }

        return Ok(await servers.GetPageAsync(page, pageSize, cancellationToken));
    }

    [HttpGet("{serverId:int}")]
    [ProducesResponseType<ServerResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServerResponse>> GetById(int serverId, CancellationToken cancellationToken)
    {
        var server = await servers.GetByIdAsync(serverId, cancellationToken);
        return Ok(server);
    }

    [HttpPost]
    [Authorize(Policy = "servers.manage")]
    [ProducesResponseType<ServerResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<ServerResponse>> Create(
        CreateServerRequest request,
        CancellationToken cancellationToken)
    {
        var server = await servers.CreateAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, server);
    }

    [HttpPut("{serverId:int}")]
    [Authorize(Policy = "servers.manage")]
    [ProducesResponseType<ServerResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ServerResponse>> Update(
        int serverId,
        UpdateServerRequest request,
        CancellationToken cancellationToken)
    {
        var server = await servers.UpdateAsync(serverId, request, cancellationToken);
        return Ok(server);
    }

    [HttpDelete("{serverId:int}")]
    [Authorize(Policy = "servers.manage")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int serverId, CancellationToken cancellationToken)
    {
        await servers.DeleteAsync(serverId, cancellationToken);
        return NoContent();
    }
}