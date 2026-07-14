using System.Text;
using System.Text.Json;
using BuildingBlocks.Messaging.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace BuildingBlocks.Messaging;

/// <summary>
/// Background service that consumes email messages from RabbitMQ 
/// and sends them via the IEmailService.
/// Reads config directly from IConfiguration (appsettings.json section "RabbitMq").
/// Retry count tracked via RabbitMQ message headers (x-retry-count).
/// </summary>
public class EmailConsumerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailConsumerService> _logger;
    private IConnection? _connection;
    private IChannel? _channel;

    private const string RetryCountHeader = "x-retry-count";

    public EmailConsumerService(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<EmailConsumerService> logger)
    {
        _serviceProvider = serviceProvider;
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
    private int MaxRetryCount => int.TryParse(_configuration["RabbitMq:MaxRetryCount"], out var r) ? r : 3;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EmailConsumerService starting...");

        // Wait a bit for RabbitMQ to be ready
        await Task.Delay(3000, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConnectAndConsumeAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "EmailConsumerService connection lost. Reconnecting in 5 seconds...");
                await CleanupAsync();
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    private async Task ConnectAndConsumeAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = HostName,
            Port = Port,
            UserName = UserName,
            Password = Password
        };

        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        // Set prefetch to process one message at a time
        await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);

        // Declare Dead Letter Exchange and Queue
        await _channel.ExchangeDeclareAsync(
            exchange: DeadLetterExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            cancellationToken: stoppingToken);

        await _channel.QueueDeclareAsync(
            queue: DeadLetterQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await _channel.QueueBindAsync(
            queue: DeadLetterQueueName,
            exchange: DeadLetterExchangeName,
            routingKey: EmailQueueName,
            cancellationToken: stoppingToken);

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
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            await HandleMessageAsync(ea, stoppingToken);
        };

        await _channel.BasicConsumeAsync(
            queue: EmailQueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation("EmailConsumerService connected and consuming from {Queue}", EmailQueueName);

        // Keep alive until cancellation
        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Expected on shutdown
        }
    }

    private async Task HandleMessageAsync(BasicDeliverEventArgs ea, CancellationToken stoppingToken)
    {
        var messageType = ea.BasicProperties?.Type ?? "Unknown";
        var body = Encoding.UTF8.GetString(ea.Body.ToArray());

        _logger.LogInformation("Received message type {Type} from queue", messageType);

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            switch (messageType)
            {
                case nameof(PasswordResetEmailMessage):
                    var resetMsg = JsonSerializer.Deserialize<PasswordResetEmailMessage>(body, options)!;
                    await emailService.SendPasswordResetEmailAsync(
                        resetMsg.ToEmail, resetMsg.UserName, resetMsg.ResetLink, stoppingToken);
                    break;

                case nameof(WelcomeEmailMessage):
                    var welcomeMsg = JsonSerializer.Deserialize<WelcomeEmailMessage>(body, options)!;
                    await emailService.SendWelcomeEmailAsync(
                        welcomeMsg.ToEmail, welcomeMsg.UserName, stoppingToken);
                    break;

                case nameof(EmailVerificationMessage):
                    var verifyMsg = JsonSerializer.Deserialize<EmailVerificationMessage>(body, options)!;
                    await emailService.SendEmailVerificationAsync(
                        verifyMsg.ToEmail, verifyMsg.UserName, verifyMsg.VerificationLink, stoppingToken);
                    break;

                default:
                    _logger.LogWarning("Unknown email message type: {Type}", messageType);
                    await _channel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
                    return;
            }

            await _channel!.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
            _logger.LogInformation("Successfully processed {Type} message", messageType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message type {Type}", messageType);

            // Read retry count from message headers
            var retryCount = GetRetryCountFromHeaders(ea.BasicProperties);

            if (retryCount < MaxRetryCount)
            {
                // Republish with same body, incremented retry count in headers
                await RepublishWithRetryAsync(ea.Body.ToArray(), messageType, retryCount + 1, stoppingToken);
                await _channel!.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                _logger.LogWarning("Requeued message type {Type}, retry {Retry}/{Max}",
                    messageType, retryCount + 1, MaxRetryCount);
            }
            else
            {
                // Max retries exceeded — reject, routes to DLQ
                await _channel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
                _logger.LogError("Max retries exceeded for {Type}. Message moved to DLQ.", messageType);
            }
        }
    }

    /// <summary>
    /// Read retry count from RabbitMQ message headers (x-retry-count).
    /// Returns 0 if header is not present.
    /// </summary>
    private static int GetRetryCountFromHeaders(IReadOnlyBasicProperties? properties)
    {
        if (properties?.Headers == null)
            return 0;

        if (properties.Headers.TryGetValue(RetryCountHeader, out var value))
        {
            return value switch
            {
                int intVal => intVal,
                long longVal => (int)longVal,
                byte[] bytes => int.TryParse(Encoding.UTF8.GetString(bytes), out var parsed) ? parsed : 0,
                _ => 0
            };
        }

        return 0;
    }

    /// <summary>
    /// Republish the original message body unchanged, with retry count stored in headers.
    /// </summary>
    private async Task RepublishWithRetryAsync(byte[] originalBody, string messageType, int newRetryCount, CancellationToken cancellationToken)
    {
        try
        {
            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
                Type = messageType,
                Headers = new Dictionary<string, object?>
                {
                    { RetryCountHeader, newRetryCount }
                }
            };

            await _channel!.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: EmailQueueName,
                mandatory: false,
                basicProperties: properties,
                body: originalBody,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to republish message for retry");
        }
    }

    private async Task CleanupAsync()
    {
        try
        {
            if (_channel is { IsOpen: true })
            {
                await _channel.CloseAsync();
                await _channel.DisposeAsync();
            }
            if (_connection is { IsOpen: true })
            {
                await _connection.CloseAsync();
                await _connection.DisposeAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cleanup");
        }
        finally
        {
            _channel = null;
            _connection = null;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("EmailConsumerService stopping...");
        await CleanupAsync();
        await base.StopAsync(cancellationToken);
    }
}
