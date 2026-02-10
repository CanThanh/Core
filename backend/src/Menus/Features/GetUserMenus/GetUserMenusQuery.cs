using BuildingBlocks.Common.Abstractions;

namespace Menus.Features.GetUserMenus;

public record GetUserMenusQuery(Guid UserId) : IQuery<List<MenuDto>>;

public record MenuDto(
    Guid Id,
    string Name,
    string? Icon,
    string? Route,
    int DisplayOrder,
    List<MenuDto> Children
);
