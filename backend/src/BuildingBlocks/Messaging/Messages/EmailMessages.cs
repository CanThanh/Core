using System.Text.Json.Serialization;

namespace BuildingBlocks.Messaging.Messages;

/// <summary>
/// Base email message for RabbitMQ queue
/// </summary>
public class SendEmailMessage
{
    public string ToEmail { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string EmailType { get; set; } = string.Empty;
    public int RetryCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Message for password reset emails
/// </summary>
public class PasswordResetEmailMessage : SendEmailMessage
{
    public string ResetLink { get; set; } = string.Empty;

    public PasswordResetEmailMessage()
    {
        EmailType = nameof(PasswordResetEmailMessage);
    }
}

/// <summary>
/// Message for welcome emails
/// </summary>
public class 
    WelcomeEmailMessage : SendEmailMessage
{
    public WelcomeEmailMessage()
    {
        EmailType = nameof(WelcomeEmailMessage);
    }
}

/// <summary>
/// Message for email verification
/// </summary>
public class EmailVerificationMessage : SendEmailMessage
{
    public string VerificationLink { get; set; } = string.Empty;

    public EmailVerificationMessage()
    {
        EmailType = nameof(EmailVerificationMessage);
    }
}
