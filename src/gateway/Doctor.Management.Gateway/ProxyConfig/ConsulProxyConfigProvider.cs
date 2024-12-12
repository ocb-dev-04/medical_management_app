using Yarp.ReverseProxy.Configuration;

using AgentService = Consul.AgentService;
using IConsulClient = Consul.IConsulClient;

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
        Consul.QueryResult<Dictionary<string, AgentService>>? services = await _consulClient.Agent.Services(cancellationToken);
        List<RouteConfig> routes = services.Response.Select(s =>
        {
            AgentService value = s.Value;
            return new RouteConfig
            {
                RouteId = $"{value.Service}-route",
                ClusterId = $"{value.Service}-cluster",
                Match = new RouteMatch { Path = $"/{value.Service}/{{**catch-all}}" }
            };
        }).ToList();

        List<ClusterConfig> clusters = services.Response.Select(s =>
        {
            AgentService value = s.Value;
            return new ClusterConfig
            {

                ClusterId = $"{value.Service}-cluster",
                Destinations = new Dictionary<string, DestinationConfig>
            {
                {
                    $"{value.Service}-destination",
                    new DestinationConfig
                    {
                        Address = string.Format("{0}:{1}", value.Address, value.Port),
                        Host = "http"
                    }
                }
            }
            };
        }).ToList();

        _logger.LogInformation("Routes added: {0} - Clusters added: {1}", routes.Count, clusters.Count);
        _config.Update(routes, clusters);
    }
}
