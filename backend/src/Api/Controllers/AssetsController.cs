using Assets.Features.CreateAsset;
using Assets.Features.DeleteAsset;
using Assets.Features.GetAssetById;
using Assets.Features.GetAssetCategories;
using Assets.Features.GetAssets;
using Assets.Features.UpdateAsset;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AssetsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AssetsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all assets with pagination and filtering
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAssets(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] string? status = null)
    {
        var query = new GetAssetsQuery(pageNumber, pageSize, searchTerm, categoryId, status);
        var result = await _mediator.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Get all asset categories
    /// </summary>
    [HttpGet("categories")]
    public async Task<IActionResult> GetAssetCategories()
    {
        var query = new GetAssetCategoriesQuery();
        var result = await _mediator.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Get asset by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAssetById(Guid id)
    {
        var query = new GetAssetByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Create a new asset
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateAsset([FromBody] CreateAssetCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return CreatedAtAction(nameof(GetAssetById), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Update an existing asset
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsset(Guid id, [FromBody] UpdateAssetRequest request)
    {
        var command = new UpdateAssetCommand(
            id,
            request.Name,
            request.CategoryId,
            request.Manufacturer,
            request.SerialNumber,
            request.PurchasePrice,
            request.PurchaseDate,
            request.DepreciationRate,
            request.Location,
            request.Status,
            request.IsActive
        );

        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Delete an asset
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsset(Guid id)
    {
        var command = new DeleteAssetCommand(id);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        return NoContent();
    }
}

public record UpdateAssetRequest(
    string Name,
    Guid CategoryId,
    string? Manufacturer,
    string? SerialNumber,
    decimal PurchasePrice,
    DateTime PurchaseDate,
    decimal DepreciationRate,
    string? Location,
    string Status,
    bool IsActive
);
