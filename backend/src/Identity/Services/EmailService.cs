using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Identity.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendPasswordResetEmailAsync(
        string toEmail,
        string userName,
        string resetLink,
        CancellationToken cancellationToken = default)
    {
        var subject = "Password Reset Request";
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #4CAF50; color: white; text-decoration: none; border-radius: 4px; margin: 20px 0; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Password Reset Request</h1>
        </div>
        <div class='content'>
            <p>Hello {userName},</p>
            <p>We received a request to reset your password. Click the button below to reset it:</p>
            <p style='text-align: center;'>
                <a href='{resetLink}' class='button'>Reset Password</a>
            </p>
            <p>Or copy and paste this link into your browser:</p>
            <p><a href='{resetLink}'>{resetLink}</a></p>
            <p><strong>This link will expire in 60 minutes.</strong></p>
            <p>If you didn't request a password reset, please ignore this email.</p>
        </div>
        <div class='footer'>
            <p>© 2025 Asset Management System. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, body, cancellationToken);
    }

    public async Task SendWelcomeEmailAsync(
        string toEmail,
        string userName,
        CancellationToken cancellationToken = default)
    {
        var subject = "Welcome to Asset Management System";
        var body = $@"
<!DOCTYPE html>
<html>
<body>
    <h2>Welcome {userName}!</h2>
    <p>Thank you for joining our Asset Management System.</p>
    <p>You can now log in and start managing your assets.</p>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, body, cancellationToken);
    }

    public async Task SendEmailVerificationAsync(
        string toEmail,
        string userName,
        string verificationLink,
        CancellationToken cancellationToken = default)
    {
        var subject = "Email Verification";
        var body = $@"
<!DOCTYPE html>
<html>
<body>
    <h2>Verify Your Email</h2>
    <p>Hello {userName},</p>
    <p>Please click the link below to verify your email address:</p>
    <p><a href='{verificationLink}'>Verify Email</a></p>
</body>
</html>";

        await SendEmailAsync(toEmail, subject, body, cancellationToken);
    }

    private async Task SendEmailAsync(
        string toEmail,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
            var smtpPort = Convert.ToInt32(_configuration["Email:SmtpPort"] ?? "587");
            var fromEmail = _configuration["Email:FromEmail"];
            var fromName = _configuration["Email:FromName"] ?? "Asset Management System";
            var username = _configuration["Email:Username"];
            var password = _configuration["Email:Password"];

            if (string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                _logger.LogWarning("Email configuration is missing. Email not sent to {Email}", toEmail);
                Console.WriteLine($"[EMAIL] Would send to {toEmail}: {subject}");
                return;
            }

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(username, password)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage, cancellationToken);

            _logger.LogInformation("Email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            throw;
        }
    }
}
