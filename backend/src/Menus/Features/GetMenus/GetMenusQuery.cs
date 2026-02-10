using BuildingBlocks.Common.Abstractions;
using Menus.Features.GetUserMenus;

namespace Menus.Features.GetMenus;

public record GetMenusQuery() : IQuery<List<MenuDto>>;
