using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Database;
using Menus.Entities;
using Microsoft.EntityFrameworkCore;

namespace Menus.Features.AssignPermissionsToMenu;

public class AssignPermissionsToMenuCommandHandler
    : ICommandHandler<AssignPermissionsToMenuCommand, bool>
{
    private readonly ApplicationDbContext _context;

    public AssignPermissionsToMenuCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(
        AssignPermissionsToMenuCommand request,
        CancellationToken cancellationToken)
    {
        // Validate menu exists
        var menuExists = await _context.Set<Menu>()
            .AnyAsync(m => m.Id == request.MenuId, cancellationToken);

        if (!menuExists)
        {
            return Result.Failure<bool>($"Menu with ID {request.MenuId} not found");
        }

        // Validate all permissions exist
        var permissionIds = request.Assignments.Select(a => a.PermissionId).Distinct().ToList();
        var existingPermissions = await _context.Set<Authorization.Entities.Permission>()
            .Where(p => permissionIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        if (existingPermissions.Count != permissionIds.Count)
        {
            return Result.Failure<bool>("One or more permissions not found");
        }

        // Clear existing menu permissions
        var existingMenuPermissions = await _context.Set<MenuPermission>()
            .Where(mp => mp.MenuId == request.MenuId)
            .ToListAsync(cancellationToken);

        _context.Set<MenuPermission>().RemoveRange(existingMenuPermissions);

        // Add new menu permissions
        foreach (var assignment in request.Assignments)
        {
            _context.Set<MenuPermission>().Add(new MenuPermission
            {
                MenuId = request.MenuId,
                PermissionId = assignment.PermissionId,
                PermissionType = assignment.PermissionType
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}
