using Authorization.Entities;
using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Database;
using Menus.Entities;
using Microsoft.EntityFrameworkCore;
using Users.Entities;

namespace Menus.Features.GetUserMenus;

public class GetUserMenusQueryHandler : IQueryHandler<GetUserMenusQuery, List<MenuDto>>
{
    private readonly ApplicationDbContext _context;

    public GetUserMenusQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<MenuDto>>> Handle(
        GetUserMenusQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Get user's role IDs
        var userRoleIds = await _context.Set<UserRole>()
            .Where(ur => ur.UserId == request.UserId)
            .Select(ur => ur.RoleId)
            .ToListAsync(cancellationToken);

        // 2. Get user's group IDs
        var userGroupIds = await _context.Set<UserGroup>()
            .Where(ug => ug.UserId == request.UserId)
            .Select(ug => ug.GroupId)
            .ToListAsync(cancellationToken);

        // 3. Get all permission IDs from user's roles
        var rolePermissionIds = await _context.Set<RolePermission>()
            .Where(rp => userRoleIds.Contains(rp.RoleId))
            .Select(rp => rp.PermissionId)
            .Distinct()
            .ToListAsync(cancellationToken);

        // 4. Get all permission IDs from user's groups
        var groupPermissionIds = await _context.Set<GroupPermission>()
            .Where(gp => userGroupIds.Contains(gp.GroupId))
            .Select(gp => gp.PermissionId)
            .Distinct()
            .ToListAsync(cancellationToken);

        // 5. Combine all permission IDs (from roles + groups)
        var allPermissionIds = rolePermissionIds.Union(groupPermissionIds).Distinct().ToList();

        if (!allPermissionIds.Any())
        {
            // User has no permissions from roles or groups
            return Result.Success(new List<MenuDto>());
        }

        // 6. Get menu IDs from role permissions (MenuPermissions table)
        var roleMenuIds = await _context.Set<MenuPermission>()
            .Where(mp => mp.PermissionType == "View" && rolePermissionIds.Contains(mp.PermissionId))
            .Select(mp => mp.MenuId)
            .Distinct()
            .ToListAsync(cancellationToken);

        // 7. Get menu IDs from group permissions (GroupMenuPermissions table)
        var groupMenuIds = await _context.Set<GroupMenuPermission>()
            .Where(gmp => gmp.PermissionType == "View" && userGroupIds.Contains(gmp.GroupId))
            .Select(gmp => gmp.MenuId)
            .Distinct()
            .ToListAsync(cancellationToken);

        // 8. Combine all accessible menu IDs (from roles + groups)
        var accessibleMenuIds = roleMenuIds.Union(groupMenuIds).Distinct().ToList();

        if (!accessibleMenuIds.Any())
        {
            // No menus assigned to user's permissions
            return Result.Success(new List<MenuDto>());
        }

        // 9. Get all accessible menus with their parent relationships
        var accessibleMenus = await _context.Set<Menu>()
            .Where(m => accessibleMenuIds.Contains(m.Id) && m.IsActive)
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync(cancellationToken);

        // 10. Build hierarchical menu structure
        var menuDtos = BuildMenuHierarchy(accessibleMenus);

        return Result.Success(menuDtos);
    }

    private List<MenuDto> BuildMenuHierarchy(List<Menu> menus)
    {
        // Create dictionary for quick lookup
        var menuDict = menus.ToDictionary(m => m.Id);
        var rootMenus = new List<MenuDto>();

        // Find root menus (no parent or parent not in accessible list)
        foreach (var menu in menus.Where(m => m.ParentId == null || !menuDict.ContainsKey(m.ParentId.Value)))
        {
            var menuDto = MapToDto(menu, menus, menuDict);
            rootMenus.Add(menuDto);
        }

        return rootMenus;
    }

    private MenuDto MapToDto(Menu menu, List<Menu> allMenus, Dictionary<Guid, Menu> menuDict)
    {
        // Find children of current menu
        var children = allMenus
            .Where(m => m.ParentId == menu.Id)
            .OrderBy(m => m.DisplayOrder)
            .Select(m => MapToDto(m, allMenus, menuDict))
            .ToList();

        return new MenuDto(
            menu.Id,
            menu.Name,
            menu.Icon,
            menu.Route,
            menu.DisplayOrder,
            children
        );
    }
}
