using BuildingBlocks.Common.Abstractions;

namespace Authorization.Features.UpdateRole;

public record UpdateRoleCommand(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive
) : ICommand<UpdateRoleResponse>;

public record UpdateRoleResponse(
    Guid Id,
    string Name
);
