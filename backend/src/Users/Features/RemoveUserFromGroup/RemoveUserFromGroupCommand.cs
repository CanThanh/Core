using BuildingBlocks.Common.Abstractions;

namespace Users.Features.RemoveUserFromGroup;

public record RemoveUserFromGroupCommand(
    Guid UserId,
    Guid GroupId
) : ICommand<bool>;
