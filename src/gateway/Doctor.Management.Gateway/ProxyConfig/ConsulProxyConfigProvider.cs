using Consul;
using Yarp.ReverseProxy.Configuration;

using IConsulClient = Consul.IConsulClient;
using RouteConfig = Yarp.ReverseProxy.Configuration.RouteConfig;
using DestinationConfig = Yarp.ReverseProxy.Configuration.DestinationConfig;

namespace Doctor.Management.Gateway.ProxyConfig;

public sealed class ConsulProxyConfigProvider : IProxyConfigProvider
{
    private readonly IConsulClient _consulClient;
    private readonly InMemoryProxyConfig _config;
    private readonly ILogger<ConsulProxyConfigProvider> _logger;

    public ConsulProxyConfigProvider(
        IConsulClient consulClient,
        InMemoryProxyConfig config,
        ILogger<ConsulProxyConfigProvider> logger)
    {
        ArgumentNullException.ThrowIfNull(consulClient, nameof(consulClient));
        ArgumentNullException.ThrowIfNull(config, nameof(config));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _consulClient = consulClient;
        _config = config;
        _logger = logger;
    }

    public IProxyConfig GetConfig()
        => _config;

    public async Task UpdateRoutesAsync(CancellationToken cancellationToken = default)
    {
        QueryResult<Dictionary<string, string[]>> catalog
            = await _consulClient.Catalog.Services(cancellationToken);

        Task<QueryResult<ServiceEntry[]>>[] healthTasks = catalog.Response.Keys
            .Select(name => _consulClient.Health.Service(name, null, passingOnly: true, cancellationToken))
            .ToArray();
        await Task.WhenAll(healthTasks);

        IEnumerable<IGrouping<string, ServiceEntry>> grouped = healthTasks
            .SelectMany(t => t.Result.Response)
            .GroupBy(e => e.Service.Service);

        List<RouteConfig> routes = BuildRoutes(grouped);
        List<ClusterConfig> clusters = BuildClusters(grouped);

        _logger.LogInformation(
            "Routes updated: {RouteCount} routes, {ClusterCount} clusters.",
            routes.Count,
            clusters.Count);
        _config.Update(routes, clusters);
    }

    private static List<RouteConfig> BuildRoutes(IEnumerable<IGrouping<string, ServiceEntry>> grouped)
        => grouped
            .Select(g => new RouteConfig
            {
                RouteId = $"{g.Key}-route",
                ClusterId = $"{g.Key}-cluster",
                Match = new RouteMatch { Path = $"/{g.Key}/{{**catch-all}}" }
            })
            .ToList();

    private static List<ClusterConfig> BuildClusters(IEnumerable<IGrouping<string, ServiceEntry>> grouped)
        => grouped
            .Select(g => new ClusterConfig
            {
                ClusterId = $"{g.Key}-cluster",
                Destinations = g.ToDictionary(
                    e => $"{e.Service.Service}-{e.Service.ID}",
                    e => new DestinationConfig { Address = $"http://{e.Service.Address}:{e.Service.Port}" }
                )
            })
            .ToList();
}
