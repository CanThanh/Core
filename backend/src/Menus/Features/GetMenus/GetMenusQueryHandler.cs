using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Database;
using Menus.Entities;
using Menus.Features.GetUserMenus;
using Microsoft.EntityFrameworkCore;

namespace Menus.Features.GetMenus;

public class GetMenusQueryHandler : IQueryHandler<GetMenusQuery, List<MenuDto>>
{
    private readonly ApplicationDbContext _context;

    public GetMenusQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<MenuDto>>> Handle(
        GetMenusQuery request,
        CancellationToken cancellationToken)
    {
        var allMenus = await _context.Set<Menu>()
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync(cancellationToken);

        var menuDtos = BuildMenuHierarchy(allMenus);

        return Result.Success(menuDtos);
    }

    private List<MenuDto> BuildMenuHierarchy(List<Menu> menus)
    {
        var menuDict = menus.ToDictionary(m => m.Id);
        var rootMenus = new List<MenuDto>();

        foreach (var menu in menus.Where(m => m.ParentId == null))
        {
            var menuDto = MapToDto(menu, menus, menuDict);
            rootMenus.Add(menuDto);
        }

        return rootMenus;
    }

    private MenuDto MapToDto(Menu menu, List<Menu> allMenus, Dictionary<Guid, Menu> menuDict)
    {
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
