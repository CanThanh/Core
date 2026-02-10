using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Database;
using Microsoft.EntityFrameworkCore;
using Users.Entities;

namespace Users.Features.UpdateGroup;

public class UpdateGroupCommandHandler : ICommandHandler<UpdateGroupCommand, UpdateGroupResponse>
{
    private readonly ApplicationDbContext _context;

    public UpdateGroupCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<UpdateGroupResponse>> Handle(
        UpdateGroupCommand request,
        CancellationToken cancellationToken)
    {
        var group = await _context.Set<Group>()
            .FirstOrDefaultAsync(g => g.Id == request.GroupId, cancellationToken);

        if (group == null)
        {
            return Result.Failure<UpdateGroupResponse>($"Group with ID {request.GroupId} not found.");
        }

        // Check if name is already used by another group
        var nameExists = await _context.Set<Group>()
            .AnyAsync(g => g.Name == request.Name && g.Id != request.GroupId, cancellationToken);

        if (nameExists)
        {
            return Result.Failure<UpdateGroupResponse>(
                $"Group name '{request.Name}' is already in use by another group.");
        }

        group.Name = request.Name;
        group.Description = request.Description;
        group.IsActive = request.IsActive;
        group.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var response = new UpdateGroupResponse(
            group.Id,
            group.Name,
            group.Description,
            group.IsActive
        );

        return Result.Success(response);
    }
}
