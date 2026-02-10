using BuildingBlocks.Common.Abstractions;

namespace Menus.Features.DeleteMenu;

public record DeleteMenuCommand(Guid Id) : ICommand<bool>;
