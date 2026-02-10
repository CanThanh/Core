using BuildingBlocks.Common.Abstractions;

namespace Menus.Features.UpdateMenu;

public record UpdateMenuCommand(
    Guid Id,
    string Name,
    string? Icon,
    string? Route,
    int DisplayOrder,
    bool IsActive,
    Guid? ParentId
) : ICommand<UpdateMenuResponse>;

public record UpdateMenuResponse(
    Guid Id,
    string Name,
    string? Route
);
