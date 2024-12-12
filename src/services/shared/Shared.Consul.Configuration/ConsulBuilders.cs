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
            CancellationToken cancellationToken = app.Lifetime.ApplicationStopping;

            _ = Task.Run(async () =>
            {
                try
                {
                    IConsulClient consulClient = app.Services.GetRequiredService<IConsulClient>();
                    await consulClient.Agent.ServiceRegister(settings.MapToAgentRegistration(), cancellationToken);

                }
                catch (Exception)
                {
                    app.Logger.LogError("--> Error adding service registration to Consul...");
                }
            }, cancellationToken);
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
