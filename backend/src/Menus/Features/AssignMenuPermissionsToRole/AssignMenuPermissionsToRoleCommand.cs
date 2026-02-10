using BuildingBlocks.Common.Abstractions;

namespace Menus.Features.AssignMenuPermissionsToRole;

public record AssignMenuPermissionsToRoleCommand(
    Guid RoleId,
    List<MenuPermissionAssignment> MenuPermissions
) : ICommand<bool>;

public record MenuPermissionAssignment(
    Guid MenuId,
    string PermissionType // "View", "Create", "Edit", "Delete"
);
