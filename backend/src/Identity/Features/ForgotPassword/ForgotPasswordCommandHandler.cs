using System.Security.Cryptography;
using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Database;
using BuildingBlocks.Messaging;
using Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Identity.Features.ForgotPassword;

public class ForgotPasswordCommandHandler : ICommandHandler<ForgotPasswordCommand, ForgotPasswordResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IEmailPublisher _emailPublisher;

    public ForgotPasswordCommandHandler(
        ApplicationDbContext context,
        IConfiguration configuration,
        IEmailPublisher emailPublisher)
    {
        _context = context;
        _configuration = configuration;
        _emailPublisher = emailPublisher;
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

        // Publish email to RabbitMQ for async sending
        var resetLink = $"{_configuration["App:FrontendUrl"]}/reset-password?token={resetToken}&email={user.Email}";

        try
        {
            await _emailPublisher.PublishPasswordResetEmailAsync(user.Email, user.FullName, resetLink, cancellationToken);
        }
        catch (Exception ex)
        {
            // Log error but don't fail the request (security: don't reveal if email failed)
            Console.WriteLine($"Failed to publish email to queue for {user.Email}: {ex.Message}");
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
