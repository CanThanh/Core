using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Users.Features.AddUserToGroup;
using Users.Features.AssignRolesToUser;
using Users.Features.CreateUser;
using Users.Features.DeleteUser;
using Users.Features.GetUserById;
using Users.Features.GetUsers;
using Users.Features.RemoveUserFromGroup;
using Users.Features.UpdateUser;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] Guid? groupId = null)
    {
        var query = new GetUsersQuery(pageNumber, pageSize, searchTerm, isActive, groupId);
        var result = await _mediator.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var query = new GetUserByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result.IsFailure)
        {
            return NotFound(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var command = new CreateUserCommand(
            request.Username,
            request.Email,
            request.Password,
            request.FullName,
            request.PhoneNumber,
            request.IsActive
        );

        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return CreatedAtAction(nameof(GetUserById), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        var command = new UpdateUserCommand(
            id,
            request.Email,
            request.FullName,
            request.PhoneNumber,
            request.IsActive
        );

        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var command = new DeleteUserCommand(id);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return NoContent();
    }

    [HttpPost("{id:guid}/roles")]
    public async Task<IActionResult> AssignRolesToUser(Guid id, [FromBody] AssignRolesRequest request)
    {
        var command = new AssignRolesToUserCommand(id, request.RoleIds);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }

    [HttpPost("{userId:guid}/groups/{groupId:guid}")]
    public async Task<IActionResult> AddUserToGroup(Guid userId, Guid groupId)
    {
        var command = new AddUserToGroupCommand(userId, groupId);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }

    [HttpDelete("{userId:guid}/groups/{groupId:guid}")]
    public async Task<IActionResult> RemoveUserFromGroup(Guid userId, Guid groupId)
    {
        var command = new RemoveUserFromGroupCommand(userId, groupId);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return NoContent();
    }
}

public record CreateUserRequest(
    string Username,
    string Email,
    string Password,
    string FullName,
    string? PhoneNumber,
    bool IsActive = true
);

public record UpdateUserRequest(
    string Email,
    string FullName,
    string? PhoneNumber,
    bool IsActive
);

public record AssignRolesRequest(List<Guid> RoleIds);
