using BuildingBlocks.Common.Abstractions;

namespace Users.Features.CreateUser;

public record CreateUserCommand(
    string Username,
    string Email,
    string Password,
    string FullName,
    string? PhoneNumber,
    bool IsActive = true
) : ICommand<CreateUserResponse>;

public record CreateUserResponse(
    Guid Id,
    string Username,
    string Email,
    string FullName
);
