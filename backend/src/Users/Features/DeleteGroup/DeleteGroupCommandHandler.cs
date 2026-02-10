using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Database;
using Microsoft.EntityFrameworkCore;
using Users.Entities;

namespace Users.Features.DeleteGroup;

public class DeleteGroupCommandHandler : ICommandHandler<DeleteGroupCommand, bool>
{
    private readonly ApplicationDbContext _context;

    public DeleteGroupCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(
        DeleteGroupCommand request,
        CancellationToken cancellationToken)
    {
        var group = await _context.Set<Group>()
            .FirstOrDefaultAsync(g => g.Id == request.GroupId, cancellationToken);

        if (group == null)
        {
            return Result.Failure<bool>($"Group with ID {request.GroupId} not found.");
        }

        // Soft delete by setting IsActive to false
        group.IsActive = false;
        group.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}
