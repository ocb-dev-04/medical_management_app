using Doctor.Management.Gateway.ProxyConfig;

namespace Doctor.Management.Gateway.Extensions;

public static class Builders
{
    public static async Task AddDynamicRoutes(this WebApplication app)
    {
        app.Logger.LogWarning("--> Waiting a few seconds while services register with consul...");
        await Task.Delay(20 * 1000);

        try
        {

            ConsulProxyConfigProvider provider = app.Services.GetRequiredService<ConsulProxyConfigProvider>();
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(10));

            await provider.UpdateRoutesAsync(cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            app.Logger.LogError("--> Some error ocurred: {0}", ex.Message);
        }
    }
}
