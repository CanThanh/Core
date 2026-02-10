namespace Identity.Services;

/// <summary>
/// Email service interface for sending transactional emails
/// TODO: Implement using SendGrid, AWS SES, or SMTP
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
