using BuildingBlocks.Common.Abstractions;

namespace Identity.Features.Register;

public record RegisterCommand(
    string Username,
    string Email,
    string Password,
    string FullName,
    string? PhoneNumber
) : ICommand<RegisterResponse>;

public record RegisterResponse(
    Guid UserId,
    string Username,
    string Email
);
