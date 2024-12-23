using Shared.Domain.Settings;
using Shared.Global.Sources.Services;
using Microsoft.Extensions.Configuration;
using Shared.Domain.Abstractions.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Global.Sources;

public static class SharedSourcesServices
{
    public static IServiceCollection AddElasticSearchService(this IServiceCollection services)
    {
        services.AddSingleton(typeof(IElasticSearchService<>), typeof(ElasticSearchService<>));

        return services;
    }

    public static IServiceCollection AddCachingService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache((options) =>
        {
            CacheDatabaseSettings? inMemoryDatabaseSettings = configuration.GetSection(nameof(CacheDatabaseSettings))
                                                                .Get<CacheDatabaseSettings>();
            ArgumentNullException.ThrowIfNull(inMemoryDatabaseSettings, nameof(inMemoryDatabaseSettings));

            options.ConfigurationOptions = new()
            {
                EndPoints = { inMemoryDatabaseSettings.ConnectionString },
                Password = inMemoryDatabaseSettings.Password,
                ConnectTimeout = inMemoryDatabaseSettings.Timeout,
                ConnectRetry = inMemoryDatabaseSettings.MaxRetryCount
            };
        });

        services.AddSingleton<ICachingService, CachingService>();

        return services;
    }
}
