using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Database;
using Microsoft.EntityFrameworkCore;
using Users.Entities;

namespace Users.Features.CreateGroup;

public class CreateGroupCommandHandler : ICommandHandler<CreateGroupCommand, CreateGroupResponse>
{
    private readonly ApplicationDbContext _context;

    public CreateGroupCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CreateGroupResponse>> Handle(
        CreateGroupCommand request,
        CancellationToken cancellationToken)
    {
        // Check if group name already exists
        var groupExists = await _context.Set<Group>()
            .AnyAsync(g => g.Name == request.Name, cancellationToken);

        if (groupExists)
        {
            return Result.Failure<CreateGroupResponse>(
                $"Group with name '{request.Name}' already exists.");
        }

        var group = new Group
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.Set<Group>().AddAsync(group, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new CreateGroupResponse(
            group.Id,
            group.Name,
            group.Description
        );

        return Result.Success(response);
    }
}
