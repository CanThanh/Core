using BuildingBlocks.Common.Abstractions;

namespace Menus.Features.CreateMenu;

public record CreateMenuCommand(
    string Name,
    string? Icon,
    string? Route,
    int DisplayOrder,
    bool IsActive,
    Guid? ParentId
) : ICommand<CreateMenuResponse>;

public record CreateMenuResponse(
    Guid Id,
    string Name,
    string? Route,
    int DisplayOrder
);
