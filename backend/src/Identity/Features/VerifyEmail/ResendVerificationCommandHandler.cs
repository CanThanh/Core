using System.Security.Cryptography;
using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Database;
using BuildingBlocks.Messaging;
using Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Identity.Features.VerifyEmail;

public class ResendVerificationCommandHandler : ICommandHandler<ResendVerificationCommand, ResendVerificationResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailPublisher _emailPublisher;
    private readonly IConfiguration _configuration;

    public ResendVerificationCommandHandler(
        ApplicationDbContext context,
        IEmailPublisher emailPublisher,
        IConfiguration configuration)
    {
        _context = context;
        _emailPublisher = emailPublisher;
        _configuration = configuration;
    }

    public async Task<Result<ResendVerificationResponse>> Handle(
        ResendVerificationCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Set<User>()
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        // Always return success to prevent email enumeration
        if (user == null)
        {
            return Result.Success(new ResendVerificationResponse(
                "If an account with that email exists, a verification email has been sent."));
        }

        if (user.IsEmailVerified)
        {
            return Result.Success(new ResendVerificationResponse("Email is already verified."));
        }

        // Generate new verification token
        var verificationToken = GenerateVerificationToken();
        var expirationHours = Convert.ToInt32(_configuration["EmailVerification:ExpirationHours"] ?? "24");

        user.EmailVerificationToken = verificationToken;
        user.EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(expirationHours);

        await _context.SaveChangesAsync(cancellationToken);

        // Publish email verification message to RabbitMQ
        var verificationLink = $"{_configuration["App:FrontendUrl"]}/verify-email?token={verificationToken}&email={user.Email}";

        await _emailPublisher.PublishEmailVerificationAsync(
            user.Email, user.FullName, verificationLink, cancellationToken);

        return Result.Success(new ResendVerificationResponse(
            "If an account with that email exists, a verification email has been sent."));
    }

    private static string GenerateVerificationToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
}
