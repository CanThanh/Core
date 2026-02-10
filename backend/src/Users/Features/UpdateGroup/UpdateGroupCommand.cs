using BuildingBlocks.Common.Abstractions;

namespace Users.Features.UpdateGroup;

public record UpdateGroupCommand(
    Guid GroupId,
    string Name,
    string? Description,
    bool IsActive
) : ICommand<UpdateGroupResponse>;

public record UpdateGroupResponse(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive
);
