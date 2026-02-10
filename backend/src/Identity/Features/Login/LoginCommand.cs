using BuildingBlocks.Common.Abstractions;

namespace Identity.Features.Login;

public record LoginCommand(string Username, string Password) : ICommand<LoginResponse>;

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserInfo User
);

public record UserInfo(
    Guid Id,
    string Username,
    string Email,
    string FullName
);
