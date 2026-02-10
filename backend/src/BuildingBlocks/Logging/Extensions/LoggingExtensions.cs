using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace BuildingBlocks.Logging.Extensions;

public static class LoggingExtensions
{
    public static IHostBuilder AddLogging(this IHostBuilder hostBuilder)
    {
        return hostBuilder.UseSerilog((context, configuration) =>
        {
            var environment = context.HostingEnvironment;
            var config = context.Configuration;

            configuration
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Environment", environment.EnvironmentName)
                .Enrich.WithProperty("Application", "AssetManagement")
                .WriteTo.Console();

            // File logging
            configuration.WriteTo.File(
                path: "logs/log-.txt",
                rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: LogEventLevel.Information,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}");

            // Elasticsearch (optional - configure if needed)
            var elasticsearchUri = config["Elasticsearch:Uri"];
            if (!string.IsNullOrEmpty(elasticsearchUri))
            {
                configuration.WriteTo.Elasticsearch(
                    new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(new Uri(elasticsearchUri))
                    {
                        AutoRegisterTemplate = true,
                        IndexFormat = "assetmanagement-logs-{0:yyyy.MM}",
                        MinimumLogEventLevel = LogEventLevel.Information
                    });
            }
        });
    }
}
