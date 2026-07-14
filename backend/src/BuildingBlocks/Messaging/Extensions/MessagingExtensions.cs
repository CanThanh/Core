using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Messaging.Extensions;

/// <summary>
/// Extension methods for registering RabbitMQ messaging services
/// </summary>
public static class MessagingExtensions
{
    /// <summary>
    /// Add RabbitMQ messaging services to the DI container
    /// </summary>
    public static IServiceCollection AddRabbitMq(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind RabbitMQ settings from configuration
        services.Configure<RabbitMqSettings>(
            configuration.GetSection(RabbitMqSettings.SectionName));

        // Register publisher as singleton (maintains persistent connection)
        services.AddSingleton<IEmailPublisher, RabbitMqEmailPublisher>();

        // Register consumer as hosted background service
        services.AddHostedService<EmailConsumerService>();

        return services;
    }
}
