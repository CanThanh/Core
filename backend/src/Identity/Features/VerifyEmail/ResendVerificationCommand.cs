using BuildingBlocks.Common.Abstractions;

namespace Identity.Features.VerifyEmail;

public record ResendVerificationCommand(
    string Email
) : ICommand<ResendVerificationResponse>;

public record ResendVerificationResponse(
    string Message
);
