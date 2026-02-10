using Authorization.Entities;
using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Exceptions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Database;
using Identity.Entities;
using Microsoft.EntityFrameworkCore;

namespace Users.Features.AssignRolesToUser;

public class AssignRolesToUserCommandHandler : ICommandHandler<AssignRolesToUserCommand, bool>
{
    private readonly ApplicationDbContext _context;

    public AssignRolesToUserCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(
        AssignRolesToUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Set<User>()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            return Result.Failure<bool>($"User with ID {request.UserId} not found.");
        }

        // Validate that all roles exist
        var existingRoles = await _context.Set<Role>()
            .Where(r => request.RoleIds.Contains(r.Id))
            .ToListAsync(cancellationToken);

        if (existingRoles.Count != request.RoleIds.Count)
        {
            var missingRoleIds = request.RoleIds.Except(existingRoles.Select(r => r.Id)).ToList();
            return Result.Failure<bool>(
                $"Roles not found: {string.Join(", ", missingRoleIds)}");
        }

        // Remove existing role assignments
        var existingUserRoles = await _context.Set<UserRole>()
            .Where(ur => ur.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        _context.Set<UserRole>().RemoveRange(existingUserRoles);

        // Add new role assignments
        var newUserRoles = request.RoleIds.Select(roleId => new UserRole
        {
            UserId = request.UserId,
            RoleId = roleId,
            AssignedAt = DateTime.UtcNow
        }).ToList();

        await _context.Set<UserRole>().AddRangeAsync(newUserRoles, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}
