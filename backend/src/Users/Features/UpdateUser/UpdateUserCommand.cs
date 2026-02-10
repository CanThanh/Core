using BuildingBlocks.Common.Abstractions;

namespace Users.Features.UpdateUser;

public record UpdateUserCommand(
    Guid UserId,
    string Email,
    string FullName,
    string? PhoneNumber,
    bool IsActive
) : ICommand<UpdateUserResponse>;

public record UpdateUserResponse(
    Guid Id,
    string Username,
    string Email,
    string FullName
);
