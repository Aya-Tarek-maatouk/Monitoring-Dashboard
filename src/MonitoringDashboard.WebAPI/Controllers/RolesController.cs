using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitoringDashboard.Application.Features.Roles;
using MonitoringDashboard.Application.Features.Roles.DTOs;

namespace MonitoringDashboard.WebAPI.Controllers;

[ApiController]
[Route("api/v1/roles")]
[Authorize(Policy = "users.manage")]
public sealed class RolesController(IRoleService roles) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RoleResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await roles.GetAllAsync(cancellationToken));

    [HttpGet("{roleId:int}")]
    public async Task<ActionResult<RoleResponse>> GetById(int roleId, CancellationToken cancellationToken) =>
        Ok(await roles.GetByIdAsync(roleId, cancellationToken));

    [HttpGet("permissions")]
    public async Task<ActionResult<IReadOnlyList<PermissionResponse>>> GetPermissions(CancellationToken cancellationToken) =>
        Ok(await roles.GetPermissionsAsync(cancellationToken));

    [HttpPost]
    [ProducesResponseType<RoleResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RoleResponse>> Create(CreateRoleRequest request, CancellationToken cancellationToken)
    {
        var role = await roles.CreateAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, role);
    }
}