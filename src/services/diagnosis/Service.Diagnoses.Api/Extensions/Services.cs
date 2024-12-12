using Common.Services;
using OpenTelemetry.Trace;
using Shared.Common.Helper;
using OpenTelemetry.Metrics;
using Shared.Domain.Settings;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using Shared.Consul.Configuration;
using Microsoft.Extensions.Options;
using Service.Diagnoses.Persistence;
using Service.Diagnoses.Application;
using Service.Diagnoses.Presentation;
using Microsoft.AspNetCore.ResponseCompression;

namespace Service.Diagnoses.Api.Extensions;

public static class Services
{
    public static void AddServices(this WebApplicationBuilder builder)
    {
        IServiceCollection services = builder.Services;
        IConfiguration configuration = builder.Configuration;

        builder.Configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

        services.AddResponseCompression(options =>
        {
            options.Providers.Add<GzipCompressionProvider>();
            options.EnableForHttps = true;
        });

        services.AddOptions<NoRelationalDatabaseSettings>()
            .BindConfiguration(nameof(NoRelationalDatabaseSettings))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<MessageQueueSettings>()
            .BindConfiguration(nameof(MessageQueueSettings))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddPersistenceServices()
            .AddApplicationServices()
            .AddPresentationServices();

        services.AddSharedCommonProviders()
            .AddHashingServices();

        services.AddSwaggerGen()
            .AddConsulServices()
            .AddHealthCheck()
            .AddTelemetries(configuration);
    }

    private static IServiceCollection AddHealthCheck(this IServiceCollection services)
    {
        services.AddHealthChecks();

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
                opt.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("ApplicationServices.Diagnoses.Api"))
                   .AddMeter("Service_Diagnoses_OpenRemoteManage")
                   .AddAspNetCoreInstrumentation()
                   .AddRuntimeInstrumentation()
                   .AddProcessInstrumentation()
                   .AddPrometheusExporter()
                   .AddOtlpExporter(otlpAction)
            )
            .WithTracing(opt =>
                opt.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("ApplicationServices.Diagnoses.Api"))
                   .AddAspNetCoreInstrumentation()
                   .AddEntityFrameworkCoreInstrumentation()
                   .AddHttpClientInstrumentation()
                   .AddOtlpExporter(otlpAction)
            );

        services.AddMetrics();

        return services;
    }
}
