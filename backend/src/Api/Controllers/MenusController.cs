using MediatR;
using Menus.Features.AssignPermissionsToMenu;
using Menus.Features.CreateMenu;
using Menus.Features.DeleteMenu;
using Menus.Features.GetMenuPermissions;
using Menus.Features.GetMenus;
using Menus.Features.GetUserMenus;
using Menus.Features.UpdateMenu;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MenusController : ControllerBase
{
    private readonly IMediator _mediator;

    public MenusController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all menus (admin)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMenus()
    {
        var query = new GetMenusQuery();
        var result = await _mediator.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Get current user's accessible menus based on their roles and permissions
    /// </summary>
    [HttpGet("user")]
    public async Task<IActionResult> GetUserMenus()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "User ID not found in token" });
        }

        var query = new GetUserMenusQuery(userId);
        var result = await _mediator.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Create new menu
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateMenu([FromBody] CreateMenuRequest request)
    {
        var command = new CreateMenuCommand(
            request.Name,
            request.Icon,
            request.Route,
            request.DisplayOrder,
            request.IsActive,
            request.ParentId
        );

        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return CreatedAtAction(nameof(GetMenus), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Update existing menu
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateMenu(Guid id, [FromBody] UpdateMenuRequest request)
    {
        var command = new UpdateMenuCommand(
            id,
            request.Name,
            request.Icon,
            request.Route,
            request.DisplayOrder,
            request.IsActive,
            request.ParentId
        );

        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Delete menu
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteMenu(Guid id)
    {
        var command = new DeleteMenuCommand(id);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return NoContent();
    }

    /// <summary>
    /// Get permissions assigned to a menu
    /// </summary>
    [HttpGet("{menuId:guid}/permissions")]
    public async Task<IActionResult> GetMenuPermissions(Guid menuId)
    {
        var query = new GetMenuPermissionsQuery(menuId);
        var result = await _mediator.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Assign permissions to a menu
    /// </summary>
    [HttpPost("{menuId:guid}/permissions")]
    public async Task<IActionResult> AssignPermissionsToMenu(
        Guid menuId,
        [FromBody] List<PermissionTypeAssignment> assignments)
    {
        var command = new AssignPermissionsToMenuCommand(menuId, assignments);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }
}

// Request DTOs
public record CreateMenuRequest(
    string Name,
    string? Icon,
    string? Route,
    int DisplayOrder,
    bool IsActive,
    Guid? ParentId
);

public record UpdateMenuRequest(
    string Name,
    string? Icon,
    string? Route,
    int DisplayOrder,
    bool IsActive,
    Guid? ParentId
);
