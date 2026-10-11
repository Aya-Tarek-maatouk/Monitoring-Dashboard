using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitoringDashboard.Application.Features.Users;
using MonitoringDashboard.Application.Features.Users.DTOs;

using MonitoringDashboard.WebAPI.Common;

namespace MonitoringDashboard.WebAPI.Controllers;

[ApiController]
[Route("api/v1/users")]
[Produces("application/json")]
public sealed class UsersController(IUserService users) : ControllerBase
{
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType<UserProfileResponse>(StatusCodes.Status200OK)]
    public ActionResult<UserProfileResponse> GetCurrentUser()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var userName = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
        var email = User.FindFirstValue(ClaimTypes.Email);
        var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        var permissions = User.FindAll("permission").Select(claim => claim.Value).ToArray();

        return Ok(new UserProfileResponse(userId, userName, email, role, permissions));
    }

    [HttpPut("{userId:int}/role")]
    [Authorize(Policy = "users.manage")]
    [ProducesResponseType<UserSummaryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserSummaryResponse>> ChangeRole(
        int userId, ChangeUserRoleRequest request, CancellationToken cancellationToken)
    {
        return Ok(await users.ChangeRoleAsync(userId, request, cancellationToken));
    }

    [HttpGet]
    [Authorize(Policy = "users.manage")]
    [ProducesResponseType<UserPageResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserPageResponse>> GetPage(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        if (!ApiPagination.IsValid(page, pageSize))
        {
            return BadRequest(ApiPagination.InvalidParameters());
        }

        return Ok(await users.GetPageAsync(page, pageSize, cancellationToken));
    }
}