using BuildingBlocks.Common.Abstractions;

namespace Identity.Features.ResetPassword;

public record ResetPasswordCommand(
    string Email,
    string Token,
    string NewPassword
) : ICommand<ResetPasswordResponse>;

public record ResetPasswordResponse(string Message);
