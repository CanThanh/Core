using BuildingBlocks.Common.Abstractions;

namespace Users.Features.AssignRolesToUser;

public record AssignRolesToUserCommand(
    Guid UserId,
    List<Guid> RoleIds
) : ICommand<bool>;
