using BuildingBlocks.Common.Abstractions;

namespace Authorization.Features.GetPermissions;

public record GetPermissionsQuery() : IQuery<List<PermissionDto>>;

public record PermissionDto(
    Guid Id,
    string Name,
    string Description,
    string Module
);
