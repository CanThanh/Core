using Authorization.Entities;
using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Database;
using Microsoft.EntityFrameworkCore;

namespace Menus.Features.GetRoleMenuPermissions;

public class GetRoleMenuPermissionsQueryHandler
    : IQueryHandler<GetRoleMenuPermissionsQuery, List<RoleMenuPermissionDto>>
{
    private readonly ApplicationDbContext _context;

    public GetRoleMenuPermissionsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<RoleMenuPermissionDto>>> Handle(
        GetRoleMenuPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        // Get all permission IDs for this role
        var rolePermissionIds = await _context.Set<RolePermission>()
            .Where(rp => rp.RoleId == request.RoleId)
            .Select(rp => rp.PermissionId)
            .ToListAsync(cancellationToken);

        // Get all menu permissions that match these permission IDs
        // Return all records (one per menu-permission combination)
        var menuPermissions = await _context.Set<Menus.Entities.MenuPermission>()
            .Include(mp => mp.Menu)
            .Where(mp => rolePermissionIds.Contains(mp.PermissionId))
            .Where(mp => mp.Menu != null) // Filter out orphaned records
            .Select(mp => new RoleMenuPermissionDto(
                mp.MenuId,
                mp.Menu.Name,
                mp.PermissionType ?? string.Empty,
                mp.PermissionId
            ))
            .ToListAsync(cancellationToken);

        return Result.Success(menuPermissions);
    }
}
