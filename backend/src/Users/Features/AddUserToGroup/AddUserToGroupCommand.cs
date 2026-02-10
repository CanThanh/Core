using BuildingBlocks.Common.Abstractions;

namespace Users.Features.AddUserToGroup;

public record AddUserToGroupCommand(
    Guid UserId,
    Guid GroupId
) : ICommand<bool>;
