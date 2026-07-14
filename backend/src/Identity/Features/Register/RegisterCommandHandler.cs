using System.Security.Cryptography;
using BuildingBlocks.Common.Abstractions;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Database;
using BuildingBlocks.Messaging;
using Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Identity.Features.Register;

public class RegisterCommandHandler : ICommandHandler<RegisterCommand, RegisterResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailPublisher _emailPublisher;
    private readonly IConfiguration _configuration;

    public RegisterCommandHandler(
        ApplicationDbContext context,
        IEmailPublisher emailPublisher,
        IConfiguration configuration)
    {
        _context = context;
        _emailPublisher = emailPublisher;
        _configuration = configuration;
    }

    public async Task<Result<RegisterResponse>> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        // Check if username already exists
        var usernameExists = await _context.Set<User>()
            .AnyAsync(u => u.Username == request.Username, cancellationToken);

        if (usernameExists)
        {
            return Result.Failure<RegisterResponse>("Username already exists");
        }

        // Check if email already exists
        var emailExists = await _context.Set<User>()
            .AnyAsync(u => u.Email == request.Email, cancellationToken);

        if (emailExists)
        {
            return Result.Failure<RegisterResponse>("Email already exists");
        }

        // Generate email verification token
        var verificationToken = GenerateVerificationToken();
        var expirationHours = Convert.ToInt32(_configuration["EmailVerification:ExpirationHours"] ?? "24");

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            IsActive = true,
            IsEmailVerified = false,
            EmailVerificationToken = verificationToken,
            EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(expirationHours)
        };

        _context.Set<User>().Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        // Publish email verification message to RabbitMQ
        var verificationLink = $"{_configuration["App:FrontendUrl"]}/verify-email?token={verificationToken}&email={user.Email}";

        try
        {
            await _emailPublisher.PublishEmailVerificationAsync(
                user.Email, user.FullName, verificationLink, cancellationToken);
        }
        catch (Exception)
        {
            // Don't fail registration if RabbitMQ is temporarily unavailable
            // The user can request a new verification email later
        }

        var response = new RegisterResponse(user.Id, user.Username, user.Email);

        return Result.Success(response);
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
