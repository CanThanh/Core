using BuildingBlocks.Common.Abstractions;

namespace Authorization.Features.GetRoles;

public record GetRolesQuery : IQuery<List<RoleDto>>;

public record RoleDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive
);
