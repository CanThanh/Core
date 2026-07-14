using BuildingBlocks.Messaging.Messages;

namespace BuildingBlocks.Messaging;

/// <summary>
/// Interface for publishing email messages to RabbitMQ queue
/// </summary>
public interface IEmailPublisher
{
    /// <summary>
    /// Publish a password reset email message to the queue
    /// </summary>
    Task PublishPasswordResetEmailAsync(string toEmail, string userName, string resetLink, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publish a welcome email message to the queue
    /// </summary>
    Task PublishWelcomeEmailAsync(string toEmail, string userName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publish an email verification message to the queue
    /// </summary>
    Task PublishEmailVerificationAsync(string toEmail, string userName, string verificationLink, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publish a raw email message to the queue
    /// </summary>
    Task PublishAsync(SendEmailMessage message, CancellationToken cancellationToken = default);
}
