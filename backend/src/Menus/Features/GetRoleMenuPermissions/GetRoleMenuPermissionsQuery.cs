using BuildingBlocks.Common.Abstractions;

namespace Menus.Features.GetRoleMenuPermissions;

public record GetRoleMenuPermissionsQuery(Guid RoleId)
    : IQuery<List<RoleMenuPermissionDto>>;

public record RoleMenuPermissionDto(
    Guid MenuId,
    string MenuName,
    string PermissionType,
    Guid PermissionId
);
