using Authorization.Entities;
using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Database;
using Microsoft.EntityFrameworkCore;

namespace Authorization.Features.DeleteRole;

public class DeleteRoleCommandHandler : ICommandHandler<DeleteRoleCommand>
{
    private readonly ApplicationDbContext _context;

    public DeleteRoleCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(
        DeleteRoleCommand request,
        CancellationToken cancellationToken)
    {
        var role = await _context.Set<Role>()
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (role is null)
        {
            return Result.Failure("Role not found");
        }

        // Check if role is assigned to any users
        if (role.UserRoles.Any())
        {
            return Result.Failure("Cannot delete role that is assigned to users");
        }

        _context.Set<Role>().Remove(role);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
