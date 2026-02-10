using BuildingBlocks.Common.Abstractions;

namespace Users.Features.CreateGroup;

public record CreateGroupCommand(
    string Name,
    string? Description
) : ICommand<CreateGroupResponse>;

public record CreateGroupResponse(
    Guid Id,
    string Name,
    string? Description
);
