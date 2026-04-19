using Doctor.Management.Gateway.ProxyConfig;

namespace Doctor.Management.Gateway.HostedServices;

internal sealed class ConsulDiscoveryHostedService : BackgroundService
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(15);

    private readonly ConsulProxyConfigProvider _provider;
    private readonly ILogger<ConsulDiscoveryHostedService> _logger;

    public ConsulDiscoveryHostedService(
        ConsulProxyConfigProvider provider,
        ILogger<ConsulDiscoveryHostedService> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await UpdateAsync(stoppingToken);

        using PeriodicTimer timer = new(PollingInterval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
            await UpdateAsync(stoppingToken);
    }

    private async Task UpdateAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _provider.UpdateRoutesAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError("Failed to update routes from Consul: {Message}", ex.Message);
        }
    }
}
