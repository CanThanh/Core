using BuildingBlocks.Common.Abstractions;

namespace Users.Features.DeleteUser;

public record DeleteUserCommand(Guid UserId) : ICommand<bool>;
