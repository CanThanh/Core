using Authorization.Features.CreateRole;
using Authorization.Features.DeleteRole;
using Authorization.Features.GetRoleById;
using Authorization.Features.GetRoles;
using Authorization.Features.UpdateRole;
using Menus.Features.AssignMenuPermissionsToRole;
using Menus.Features.GetRoleMenuPermissions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IMediator _mediator;

    public RolesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetRoles()
    {
        var query = new GetRolesQuery();
        var result = await _mediator.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetRoleById(Guid id)
    {
        var query = new GetRoleByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result.IsFailure)
        {
            return NotFound(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
    {
        var command = new CreateRoleCommand(request.Name, request.Description);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return CreatedAtAction(nameof(GetRoleById), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleRequest request)
    {
        var command = new UpdateRoleCommand(id, request.Name, request.Description, request.IsActive);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteRole(Guid id)
    {
        var command = new DeleteRoleCommand(id);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return NoContent();
    }

    [HttpPost("{roleId}/menu-permissions")]
    public async Task<IActionResult> AssignMenuPermissionsToRole(
        Guid roleId,
        [FromBody] List<MenuPermissionAssignment> assignments)
    {
        var command = new AssignMenuPermissionsToRoleCommand(roleId, assignments);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpGet("{roleId}/menu-permissions")]
    public async Task<IActionResult> GetRoleMenuPermissions(Guid roleId)
    {
        var query = new GetRoleMenuPermissionsQuery(roleId);
        var result = await _mediator.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }
}

public record CreateRoleRequest(string Name, string? Description);
public record UpdateRoleRequest(string Name, string? Description, bool IsActive);
