using System.Security.Cryptography;
using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Database;
using Identity.Entities;
using Identity.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Identity.Features.ForgotPassword;

public class ForgotPasswordCommandHandler : ICommandHandler<ForgotPasswordCommand, ForgotPasswordResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(
        ApplicationDbContext context,
        IConfiguration configuration,
        IEmailService emailService)
    {
        _context = context;
        _configuration = configuration;
        _emailService = emailService;
    }

    public async Task<Result<ForgotPasswordResponse>> Handle(
        ForgotPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Set<User>()
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        // Always return success to prevent email enumeration
        if (user == null)
        {
            return Result.Success(new ForgotPasswordResponse(
                "If an account with that email exists, a password reset link has been sent."));
        }

        // Generate reset token
        var resetToken = GenerateResetToken();
        var hashedToken = BCrypt.Net.BCrypt.HashPassword(resetToken);

        var expirationMinutes = Convert.ToInt32(_configuration["PasswordReset:ExpirationMinutes"] ?? "60");
        var resetTokenEntity = new PasswordResetToken
        {
            UserId = user.Id,
            Token = hashedToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes)
        };

        _context.Set<PasswordResetToken>().Add(resetTokenEntity);
        await _context.SaveChangesAsync(cancellationToken);

        // Send email with reset link
        var resetLink = $"{_configuration["App:FrontendUrl"]}/reset-password?token={resetToken}&email={user.Email}";

        try
        {
            await _emailService.SendPasswordResetEmailAsync(user.Email, user.FullName, resetLink, cancellationToken);
        }
        catch (Exception ex)
        {
            // Log error but don't fail the request (security: don't reveal if email failed)
            Console.WriteLine($"Failed to send email to {user.Email}: {ex.Message}");
            // In development, log the token
            Console.WriteLine($"Password Reset Token for {user.Email}: {resetToken}");
            Console.WriteLine($"Reset Link: {resetLink}");
        }

        return Result.Success(new ForgotPasswordResponse(
            "If an account with that email exists, a password reset link has been sent."));
    }

    private static string GenerateResetToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }
}
