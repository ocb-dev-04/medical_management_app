using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;

namespace Doctor.Management.Gateway.ProxyConfig;

public sealed class InMemoryProxyConfig
    : IProxyConfig
{
    private readonly object _syncLock = new();
    private List<RouteConfig> _routes = [];
    private List<ClusterConfig> _clusters = [];

    public IReadOnlyList<RouteConfig> Routes
    {
        get { lock (_syncLock) return _routes.AsReadOnly(); }
    }

    public IReadOnlyList<ClusterConfig> Clusters
    {
        get { lock (_syncLock) return _clusters.AsReadOnly(); }
    }

    public IChangeToken ChangeToken { get; private set; } = new CancellationChangeToken(new CancellationTokenSource().Token);

    public void Update(IEnumerable<RouteConfig> routes, IEnumerable<ClusterConfig> clusters)
    {
        lock (_syncLock)
        {
            _routes = routes.ToList();
            _clusters = clusters.ToList();
        }

        IChangeToken previousToken = ChangeToken;
        ChangeToken = new CancellationChangeToken(new CancellationTokenSource().Token);

        (previousToken as IDisposable)?.Dispose();
    }
}

