namespace BuildingBlocks.Messaging;

/// <summary>
/// Email service interface for sending transactional emails.
/// Placed in BuildingBlocks.Messaging so the EmailConsumerService can reference it directly
/// without reflection. Implementation lives in Identity module.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send password reset email with reset link
    /// </summary>
    Task SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send welcome email to new users
    /// </summary>
    Task SendWelcomeEmailAsync(string toEmail, string userName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send email verification email
    /// </summary>
    Task SendEmailVerificationAsync(string toEmail, string userName, string verificationLink, CancellationToken cancellationToken = default);
}
