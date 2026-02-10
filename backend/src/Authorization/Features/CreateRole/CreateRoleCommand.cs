using BuildingBlocks.Common.Abstractions;

namespace Authorization.Features.CreateRole;

public record CreateRoleCommand(
    string Name,
    string? Description
) : ICommand<CreateRoleResponse>;

public record CreateRoleResponse(
    Guid Id,
    string Name
);
