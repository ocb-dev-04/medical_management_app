using Consul;
using Microsoft.Extensions.Options;
using Shared.Consul.Configuration.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Consul.Configuration;

public static class ConsulServices
{
    public static IServiceCollection AddConsulServices(this IServiceCollection services)
    {
        services.AddOptions<ConsulSettings>()
            .BindConfiguration(nameof(ConsulSettings))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<ServiceRegistrationSettings>()
            .BindConfiguration(nameof(ServiceRegistrationSettings))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IConsulClient, ConsulClient>(sp => new ConsulClient(config =>
        {
            IOptions<ConsulSettings> settings = sp.GetRequiredService<IOptions<ConsulSettings>> ();
            ArgumentNullException.ThrowIfNull(settings.Value, nameof(settings));

            config.Address = new Uri(settings.Value.Url);
            config.Token = settings.Value.Token;
        }));

        return services;
    }
}
