using Authorization.Entities;
using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Database;
using Menus.Entities;
using Microsoft.EntityFrameworkCore;

namespace Menus.Features.AssignMenuPermissionsToRole;

public class AssignMenuPermissionsToRoleCommandHandler
    : ICommandHandler<AssignMenuPermissionsToRoleCommand, bool>
{
    private readonly ApplicationDbContext _context;

    public AssignMenuPermissionsToRoleCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(
        AssignMenuPermissionsToRoleCommand request,
        CancellationToken cancellationToken)
    {
        // Validate role exists
        var roleExists = await _context.Set<Role>()
            .AnyAsync(r => r.Id == request.RoleId, cancellationToken);

        if (!roleExists)
        {
            return Result.Failure<bool>($"Role with ID {request.RoleId} not found");
        }

        // Get permission IDs from MenuPermissions
        var menuIds = request.MenuPermissions.Select(mp => mp.MenuId).Distinct().ToList();
        var permissionTypes = request.MenuPermissions.Select(mp => mp.PermissionType).Distinct().ToList();

        var menuPermissions = await _context.Set<MenuPermission>()
            .Where(mp => menuIds.Contains(mp.MenuId) &&
                        permissionTypes.Contains(mp.PermissionType))
            .ToListAsync(cancellationToken);

        // Build permission IDs to assign
        var permissionIdsToAssign = new HashSet<Guid>();

        foreach (var assignment in request.MenuPermissions)
        {
            var matchingPermissions = menuPermissions
                .Where(mp => mp.MenuId == assignment.MenuId &&
                            mp.PermissionType == assignment.PermissionType)
                .Select(mp => mp.PermissionId);

            foreach (var permId in matchingPermissions)
            {
                permissionIdsToAssign.Add(permId);
            }
        }

        // Get ALL menu-related permission IDs (any permission linked to a menu)
        var allMenuRelatedPermissionIds = await _context.Set<MenuPermission>()
            .Select(mp => mp.PermissionId)
            .Distinct()
            .ToListAsync(cancellationToken);

        // Clear ONLY existing menu-related permissions for this role
        // This preserves non-menu permissions that were assigned directly
        var existingMenuRolePermissions = await _context.Set<RolePermission>()
            .Where(rp => rp.RoleId == request.RoleId &&
                        allMenuRelatedPermissionIds.Contains(rp.PermissionId))
            .ToListAsync(cancellationToken);

        _context.Set<RolePermission>().RemoveRange(existingMenuRolePermissions);

        // Add new menu-based role permissions
        foreach (var permissionId in permissionIdsToAssign)
        {
            _context.Set<RolePermission>().Add(new RolePermission
            {
                RoleId = request.RoleId,
                PermissionId = permissionId
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}
