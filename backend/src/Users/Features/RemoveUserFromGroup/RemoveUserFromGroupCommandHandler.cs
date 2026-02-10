using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Database;
using Microsoft.EntityFrameworkCore;
using Users.Entities;

namespace Users.Features.RemoveUserFromGroup;

public class RemoveUserFromGroupCommandHandler : ICommandHandler<RemoveUserFromGroupCommand, bool>
{
    private readonly ApplicationDbContext _context;

    public RemoveUserFromGroupCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(
        RemoveUserFromGroupCommand request,
        CancellationToken cancellationToken)
    {
        var userGroup = await _context.Set<UserGroup>()
            .FirstOrDefaultAsync(ug => ug.UserId == request.UserId && ug.GroupId == request.GroupId, cancellationToken);

        if (userGroup == null)
        {
            return Result.Failure<bool>(
                "User is not a member of this group.");
        }

        _context.Set<UserGroup>().Remove(userGroup);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}
