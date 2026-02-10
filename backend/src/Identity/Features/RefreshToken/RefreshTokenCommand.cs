using BuildingBlocks.Common.Abstractions;

namespace Identity.Features.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : ICommand<RefreshTokenResponse>;

public record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);
