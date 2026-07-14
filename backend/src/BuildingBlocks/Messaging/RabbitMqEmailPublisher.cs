using System.Text;
using System.Text.Json;
using BuildingBlocks.Messaging.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace BuildingBlocks.Messaging;

/// <summary>
/// RabbitMQ implementation of IEmailPublisher.
/// Publishes email messages to the email queue for async processing.
/// Reads config directly from IConfiguration (appsettings.json section "RabbitMq").
/// </summary>
public class RabbitMqEmailPublisher : IEmailPublisher, IAsyncDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMqEmailPublisher> _logger;
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private bool _initialized;

    public RabbitMqEmailPublisher(
        IConfiguration configuration,
        ILogger<RabbitMqEmailPublisher> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    private string HostName => _configuration["RabbitMq:HostName"] ?? "localhost";
    private int Port => int.TryParse(_configuration["RabbitMq:Port"], out var p) ? p : 5672;
    private string UserName => _configuration["RabbitMq:UserName"] ?? "admin";
    private string Password => _configuration["RabbitMq:Password"] ?? "123456";
    private string EmailQueueName => _configuration["RabbitMq:EmailQueueName"] ?? "email_queue";
    private string DeadLetterQueueName => _configuration["RabbitMq:DeadLetterQueueName"] ?? "email_dead_letter_queue";
    private string DeadLetterExchangeName => _configuration["RabbitMq:DeadLetterExchangeName"] ?? "email_dlx";

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized && _connection is { IsOpen: true } && _channel is { IsOpen: true })
            return;

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            if (_initialized && _connection is { IsOpen: true } && _channel is { IsOpen: true })
                return;

            var factory = new ConnectionFactory
            {
                HostName = HostName,
                Port = Port,
                UserName = UserName,
                Password = Password
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            // Declare Dead Letter Exchange and Queue
            await _channel.ExchangeDeclareAsync(
                exchange: DeadLetterExchangeName,
                type: ExchangeType.Direct,
                durable: true,
                cancellationToken: cancellationToken);

            await _channel.QueueDeclareAsync(
                queue: DeadLetterQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: cancellationToken);

            await _channel.QueueBindAsync(
                queue: DeadLetterQueueName,
                exchange: DeadLetterExchangeName,
                routingKey: EmailQueueName,
                cancellationToken: cancellationToken);

            // Declare main email queue with DLX
            var queueArgs = new Dictionary<string, object?>
            {
                { "x-dead-letter-exchange", DeadLetterExchangeName },
                { "x-dead-letter-routing-key", EmailQueueName }
            };

            await _channel.QueueDeclareAsync(
                queue: EmailQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: queueArgs,
                cancellationToken: cancellationToken);

            _initialized = true;
            _logger.LogInformation("RabbitMQ email publisher initialized. Queue: {Queue}", EmailQueueName);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task PublishPasswordResetEmailAsync(string toEmail, string userName, string resetLink, CancellationToken cancellationToken = default)
    {
        var message = new PasswordResetEmailMessage
        {
            ToEmail = toEmail,
            UserName = userName,
            ResetLink = resetLink
        };

        await PublishAsync(message, cancellationToken);
    }

    public async Task PublishWelcomeEmailAsync(string toEmail, string userName, CancellationToken cancellationToken = default)
    {
        var message = new WelcomeEmailMessage
        {
            ToEmail = toEmail,
            UserName = userName
        };

        await PublishAsync(message, cancellationToken);
    }

    public async Task PublishEmailVerificationAsync(string toEmail, string userName, string verificationLink, CancellationToken cancellationToken = default)
    {
        var message = new EmailVerificationMessage
        {
            ToEmail = toEmail,
            UserName = userName,
            VerificationLink = verificationLink
        };

        await PublishAsync(message, cancellationToken);
    }

    public async Task PublishAsync(SendEmailMessage message, CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);

        var json = JsonSerializer.Serialize(message, message.GetType(), new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var body = Encoding.UTF8.GetBytes(json);

        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            Type = message.EmailType,
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        };

        await _channel!.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: EmailQueueName,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Published {EmailType} to queue {Queue} for {Email}",
            message.EmailType, EmailQueueName, message.ToEmail);
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
        {
            await _channel.CloseAsync();
            await _channel.DisposeAsync();
        }

        if (_connection is not null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }

        _semaphore.Dispose();
        GC.SuppressFinalize(this);
    }
}
