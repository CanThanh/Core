using BuildingBlocks.Common.Abstractions;

namespace Authorization.Features.GetRoleById;

public record GetRoleByIdQuery(Guid Id) : IQuery<RoleDetailDto>;

public record RoleDetailDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
