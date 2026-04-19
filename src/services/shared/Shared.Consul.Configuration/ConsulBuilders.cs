using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Shared.Consul.Configuration.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Consul.Configuration;

public static class ConsulBuilders
{
    public static void UseConsultServiceRegistry(this WebApplication app)
    {
        IOptions<ServiceRegistrationSettings> settings = app.Services.GetRequiredService<IOptions<ServiceRegistrationSettings>>();
        ArgumentNullException.ThrowIfNull(settings.Value, nameof(settings));

        app.AddServiceWhenStart(settings.Value);
        app.RemoveServiceWhenDown(settings.Value.Id);
    }

    private static void AddServiceWhenStart(this WebApplication app, ServiceRegistrationSettings settings)
    {
        app.Lifetime.ApplicationStarted.Register(() =>
        {
            CancellationToken stoppingToken = app.Lifetime.ApplicationStopping;

            _ = Task.Run(async () =>
            {
                IConsulClient consulClient = app.Services.GetRequiredService<IConsulClient>();
                int[] delaysInSeconds = [1, 2, 4, 8, 16, 30];
                int attempt = 0;

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        await consulClient.Agent.ServiceRegister(
                            settings.MapToAgentRegistration(),
                            CancellationToken.None);
                        app.Logger.LogInformation(
                            "Service '{Name}' registered in Consul successfully.",
                            settings.Name);
                        return;
                    }
                    catch (Exception ex)
                    {
                        int delay = attempt < delaysInSeconds.Length
                            ? delaysInSeconds[attempt]
                            : delaysInSeconds[^1];
                        app.Logger.LogWarning(
                            "Consul registration attempt {Attempt} failed: {Message}. Retrying in {Delay}s...",
                            attempt + 1,
                            ex.Message,
                            delay);
                        await Task.Delay(TimeSpan.FromSeconds(delay), stoppingToken);
                        attempt++;
                    }
                }
            }, stoppingToken);
        });
    }

    private static void RemoveServiceWhenDown(this WebApplication app, string id)
    {
        app.Lifetime.ApplicationStopping.Register(() =>
        {
            CancellationToken cancellationToken = app.Lifetime.ApplicationStopping;

            _ = Task.Run(async () =>
            {
                try
                {
                    IConsulClient consulClient = app.Services.GetRequiredService<IConsulClient>();
                    await consulClient.Agent.ServiceDeregister(id, cancellationToken);
                }
                catch (Exception)
                {
                    app.Logger.LogError("--> Error removing service registration from Consul...");
                }
            }, cancellationToken);
        });
    }
}
