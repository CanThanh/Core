using BuildingBlocks.Common.Abstractions;

namespace Authorization.Features.DeleteRole;

public record DeleteRoleCommand(Guid Id) : ICommand;
