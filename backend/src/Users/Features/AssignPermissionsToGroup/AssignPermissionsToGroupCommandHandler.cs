using Authorization.Entities;
using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Database;
using Microsoft.EntityFrameworkCore;
using Users.Entities;

namespace Users.Features.AssignPermissionsToGroup;

public class AssignPermissionsToGroupCommandHandler : ICommandHandler<AssignPermissionsToGroupCommand, bool>
{
    private readonly ApplicationDbContext _context;

    public AssignPermissionsToGroupCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(
        AssignPermissionsToGroupCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Validate group exists
        var groupExists = await _context.Set<Users.Entities.Group>()
            .AnyAsync(g => g.Id == request.GroupId, cancellationToken);

        if (!groupExists)
        {
            return Result.Failure<bool>($"Group with ID {request.GroupId} not found");
        }

        // 2. Validate all permissions exist
        var existingPermissions = await _context.Set<Permission>()
            .Where(p => request.PermissionIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        if (existingPermissions.Count != request.PermissionIds.Count)
        {
            var missingIds = request.PermissionIds.Except(existingPermissions.Select(p => p.Id)).ToList();
            return Result.Failure<bool>(
                $"Permissions not found: {string.Join(", ", missingIds)}");
        }

        // 3. Remove existing group permissions
        var existingGroupPermissions = await _context.Set<GroupPermission>()
            .Where(gp => gp.GroupId == request.GroupId)
            .ToListAsync(cancellationToken);

        _context.Set<GroupPermission>().RemoveRange(existingGroupPermissions);

        // 4. Add new group permissions
        var newGroupPermissions = request.PermissionIds.Select(permissionId => new GroupPermission
        {
            Id = Guid.NewGuid(),
            GroupId = request.GroupId,
            PermissionId = permissionId,
            AssignedAt = DateTime.UtcNow
        }).ToList();

        await _context.Set<GroupPermission>().AddRangeAsync(newGroupPermissions, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}
