using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Database;
using Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Users.Entities;

namespace Users.Features.AddUserToGroup;

public class AddUserToGroupCommandHandler : ICommandHandler<AddUserToGroupCommand, bool>
{
    private readonly ApplicationDbContext _context;

    public AddUserToGroupCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(
        AddUserToGroupCommand request,
        CancellationToken cancellationToken)
    {
        // Validate user exists
        var userExists = await _context.Set<User>()
            .AnyAsync(u => u.Id == request.UserId, cancellationToken);

        if (!userExists)
        {
            return Result.Failure<bool>($"User with ID {request.UserId} not found.");
        }

        // Validate group exists
        var groupExists = await _context.Set<Group>()
            .AnyAsync(g => g.Id == request.GroupId, cancellationToken);

        if (!groupExists)
        {
            return Result.Failure<bool>($"Group with ID {request.GroupId} not found.");
        }

        // Check if user is already in the group
        var alreadyInGroup = await _context.Set<UserGroup>()
            .AnyAsync(ug => ug.UserId == request.UserId && ug.GroupId == request.GroupId, cancellationToken);

        if (alreadyInGroup)
        {
            return Result.Failure<bool>(
                "User is already a member of this group.");
        }

        var userGroup = new UserGroup
        {
            UserId = request.UserId,
            GroupId = request.GroupId,
            JoinedAt = DateTime.UtcNow
        };

        await _context.Set<UserGroup>().AddAsync(userGroup, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}
