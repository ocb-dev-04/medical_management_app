using Microsoft.Extensions.DependencyInjection;
using Shared.Domain.Abstractions.Services;
using Shared.Global.Sources.Services;

namespace Shared.Global.Sources;

public static class SharedSourcesServices
{
    public static IServiceCollection AddElasticSearchService(this IServiceCollection services)
    {
        services.AddSingleton(typeof(IElasticSearchService<>), typeof(ElasticSearchService<>));

        return services;
    }

    public static IServiceCollection AddCachingService(this IServiceCollection services)
    {
        services.AddSingleton<ICachingService, CachingService>();

        return services;
    }
}
