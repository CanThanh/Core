using BuildingBlocks.Common.Abstractions;

namespace Identity.Features.VerifyEmail;

public record VerifyEmailCommand(
    string Email,
    string Token
) : ICommand<VerifyEmailResponse>;

public record VerifyEmailResponse(
    string Message,
    bool IsVerified
);
