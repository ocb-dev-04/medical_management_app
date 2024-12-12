using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;

namespace Doctor.Management.Gateway.ProxyConfig;

public class InMemoryProxyConfig 
    : IProxyConfig
{
    private readonly List<RouteConfig> _routes;
    private readonly List<ClusterConfig> _clusters;

    public InMemoryProxyConfig()
    {
        _routes = Enumerable.Empty<RouteConfig>().ToList();
        _clusters = Enumerable.Empty<ClusterConfig>().ToList();
    }

    public IReadOnlyList<RouteConfig> Routes => _routes;

    public IReadOnlyList<ClusterConfig> Clusters => _clusters;

    public IChangeToken ChangeToken { get; private set; } = new CancellationChangeToken(new CancellationTokenSource().Token);

    public void Update(IEnumerable<RouteConfig> routes, IEnumerable<ClusterConfig> clusters)
    {
        _routes.Clear();
        _routes.AddRange(routes);

        _clusters.Clear();
        _clusters.AddRange(clusters);

        IChangeToken previousToken = ChangeToken;
        ChangeToken = new CancellationChangeToken(new CancellationTokenSource().Token);

        (previousToken as IDisposable)?.Dispose();
    }
}

