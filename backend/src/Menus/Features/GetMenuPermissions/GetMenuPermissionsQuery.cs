using BuildingBlocks.Common.Abstractions;

namespace Menus.Features.GetMenuPermissions;

public record GetMenuPermissionsQuery(Guid MenuId)
    : IQuery<List<MenuPermissionDto>>;

public record MenuPermissionDto(
    Guid Id,
    Guid PermissionId,
    string PermissionName,
    string PermissionType
);
