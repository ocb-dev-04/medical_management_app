using Refit;
using Consul;
using System.Net;
using System.Net.Sockets;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using Microsoft.Extensions.Options;
using System.Threading.RateLimiting;
using Yarp.ReverseProxy.Configuration;
using Doctor.Management.Gateway.Settings;
using Doctor.Management.Gateway.AuthClient;
using Doctor.Management.Gateway.ProxyConfig;

namespace Doctor.Management.Gateway.Extensions;

public static class Services
{
    public static void AddServices(this WebApplicationBuilder builder)
    {
        builder.Configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

        builder.Services
            .AddConsulAsService()
            .AddAuthService()
            .AddCustomRateLimiter()
            .AddTelemetries(builder.Configuration)
            .AddReverseProxy();
    }

    private static IServiceCollection AddConsulAsService(this IServiceCollection services)
    {
        services.AddOptions<ConsulSettings>()
            .BindConfiguration(nameof(ConsulSettings))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IConsulClient, ConsulClient>(sp => new ConsulClient(config =>
        {
            IOptions<ConsulSettings> settings = sp.GetRequiredService<IOptions<ConsulSettings>>();
            ArgumentNullException.ThrowIfNull(settings.Value, nameof(settings));

            config.Address = new Uri(settings.Value.Url);
            config.Token = settings.Value.Token;
        }));

        services.AddSingleton<InMemoryProxyConfig>();
        services.AddSingleton<ConsulProxyConfigProvider>();
        services.AddSingleton<IProxyConfigProvider, ConsulProxyConfigProvider>();

        return services;
    }

    private static IServiceCollection AddAuthService(this IServiceCollection services)
    {
        services.AddOptions<AuthSettings>()
            .BindConfiguration(nameof(AuthSettings))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddRefitClient<IAuthClient>()
            .ConfigureHttpClient((sp, c) =>
                {
                    AuthSettings authSettings = sp.GetRequiredService<IOptions<AuthSettings>>().Value;
                    c.BaseAddress = new Uri(authSettings.Endpoint);
                });

        return services;
    }

    private static IServiceCollection AddCustomRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddPolicy("PerIpPolicy", context =>
            {
                IPAddress? clientIp = context.Connection.RemoteIpAddress;
                string ipKey = clientIp != null && clientIp.AddressFamily.Equals(AddressFamily.InterNetworkV6)
                    ? clientIp.MapToIPv4().ToString()
                    : clientIp?.ToString() ?? "unknown";

                return RateLimitPartition.GetTokenBucketLimiter(
                                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                                    key => new TokenBucketRateLimiterOptions
                                    {
                                        TokenLimit = 4,
                                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                                        QueueLimit = 0,
                                        ReplenishmentPeriod = TimeSpan.FromSeconds(1),
                                        TokensPerPeriod = 4,
                                        AutoReplenishment = true
                                    }
                                );
            });

            options.OnRejected = async (context, token) =>
            {
                ProblemDetails problemsDetails = new ()
                {
                    Type = "https://httpstatuses.com/429",
                    Title = "Too Many Requests",
                    Status = StatusCodes.Status429TooManyRequests,
                    Detail = "You have sent too many requests in a given amount of time. Please try again later.",
                    Instance = context.HttpContext.Request.Path
                };
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/problem+json";
                await context.HttpContext.Response.WriteAsJsonAsync(problemsDetails);
            };
        });

        return services;
    }

    private static IServiceCollection AddTelemetries(this IServiceCollection services, IConfiguration configuration)
    {
        string? otlpEndpointEnv = configuration.GetValue<string>("OTEL_EXPORTER_OTLP_ENDPOINT");
        ArgumentNullException.ThrowIfNullOrEmpty(otlpEndpointEnv);

        Uri otlpEndpoint = new Uri(otlpEndpointEnv);

        Action<OtlpExporterOptions> otlpAction =
            options => options.Endpoint = otlpEndpoint;

        services.AddOpenTelemetry()
            .WithMetrics(opt =>
                opt.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("ApplicationServices.Gateway.Api"))
                   .AddMeter("Service_Gateway_OpenRemoteManage")
                   .AddAspNetCoreInstrumentation()
                   .AddRuntimeInstrumentation()
                   .AddProcessInstrumentation()
                   .AddPrometheusExporter()
                   .AddOtlpExporter(otlpAction)
            )
            .WithTracing(opt =>
                opt.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("ApplicationServices.Gateway.Api"))
                   .AddAspNetCoreInstrumentation()
                   .AddHttpClientInstrumentation()
                   .AddOtlpExporter(otlpAction)
            );

        services.AddMetrics();

        return services;
    }
}
