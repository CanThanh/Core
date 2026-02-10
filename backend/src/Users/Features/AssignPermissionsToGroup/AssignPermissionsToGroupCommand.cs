using BuildingBlocks.Common.Abstractions;

namespace Users.Features.AssignPermissionsToGroup;

public record AssignPermissionsToGroupCommand(
    Guid GroupId,
    List<Guid> PermissionIds) : ICommand<bool>;
