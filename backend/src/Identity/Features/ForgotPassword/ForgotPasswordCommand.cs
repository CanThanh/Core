using BuildingBlocks.Common.Abstractions;

namespace Identity.Features.ForgotPassword;

public record ForgotPasswordCommand(string Email) : ICommand<ForgotPasswordResponse>;

public record ForgotPasswordResponse(string Message);
