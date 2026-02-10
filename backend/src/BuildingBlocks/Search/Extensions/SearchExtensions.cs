using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Search.Extensions;

public static class SearchExtensions
{
    public static IServiceCollection AddElasticsearch(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var uri = configuration["Elasticsearch:Uri"];

        if (string.IsNullOrEmpty(uri))
        {
            // Elasticsearch is optional
            return services;
        }

        var settings = new ElasticsearchClientSettings(new Uri(uri))
            .DefaultIndex("assetmanagement");

        var client = new ElasticsearchClient(settings);

        services.AddSingleton(client);

        return services;
    }
}
