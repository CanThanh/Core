using BuildingBlocks.Common.Abstractions;

namespace Menus.Features.AssignPermissionsToMenu;

public record AssignPermissionsToMenuCommand(
    Guid MenuId,
    List<PermissionTypeAssignment> Assignments
) : ICommand<bool>;

public record PermissionTypeAssignment(
    Guid PermissionId,
    string PermissionType // "View", "Create", "Edit", "Delete"
);
