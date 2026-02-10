using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Database;
using Menus.Entities;
using Microsoft.EntityFrameworkCore;

namespace Menus.Features.GetMenuPermissions;

public class GetMenuPermissionsQueryHandler
    : IQueryHandler<GetMenuPermissionsQuery, List<MenuPermissionDto>>
{
    private readonly ApplicationDbContext _context;

    public GetMenuPermissionsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<MenuPermissionDto>>> Handle(
        GetMenuPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        var menuPermissions = await _context.Set<MenuPermission>()
            .Include(mp => mp.Permission)
            .Where(mp => mp.MenuId == request.MenuId)
            .Select(mp => new MenuPermissionDto(
                mp.Id,
                mp.PermissionId,
                mp.Permission.Name,
                mp.PermissionType
            ))
            .ToListAsync(cancellationToken);

        return Result.Success(menuPermissions);
    }
}
