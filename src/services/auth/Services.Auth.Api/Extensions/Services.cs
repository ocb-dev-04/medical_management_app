using Shared.Domain.Settings;
using Services.Auth.Presentation;
using Services.Auth.Persistence;
using Services.Auth.Application;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Services.Auth.Application.Settings;
using Shared.Common.Helper;
using Common.Services;
using Shared.Consul.Configuration;

namespace Services.Auth.Api.Extensions;

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

        services.AddOptions<JwtSettings>()
            .BindConfiguration(nameof(JwtSettings))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<RelationalDatabaseSettings>()
            .BindConfiguration(nameof(RelationalDatabaseSettings))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<CacheDatabaseSettings>()
            .BindConfiguration(nameof(CacheDatabaseSettings))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<MessageQueueSettings>()
            .BindConfiguration(nameof(MessageQueueSettings))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddPersistenceServices()
            .AddApplicationServices(configuration)
            .AddPresentationServices();

        services.AddSharedCommonProviders()
            .AddHashingServices();

        services.AddJWt()
            .AddCustomSwagger()
            .AddConsulServices()
            .AddHealthCheck()
            .AddTelemetries(configuration);
    }

    private static IServiceCollection AddJWt(this IServiceCollection services)
    {
        services
            .AddAuthorization()
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                IServiceScope serviceScope = services.BuildServiceProvider().CreateScope();

                IOptions<JwtSettings>? jwtSettingSetup = serviceScope.ServiceProvider.GetService<IOptions<JwtSettings>>();
                ArgumentNullException.ThrowIfNull(jwtSettingSetup, nameof(jwtSettingSetup));
                JwtSettings jwtSetting = jwtSettingSetup.Value;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = jwtSetting.ValidateIssuerSigningKey,
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSetting.IssuerSigningKey)),
                    RequireExpirationTime = jwtSetting.RequireExpirationTime,
                    ValidateLifetime = jwtSetting.ValidateLifetime,
                    ValidateIssuer = jwtSetting.ValidateIssuer,
                    ValidIssuer = jwtSetting.ValidIssuer,
                    ValidAudience = jwtSetting.ValidAudience,
                    ValidateAudience = jwtSetting.ValidateAudience,
                    ClockSkew = TimeSpan.Zero,
                };
            });

        return services;
    }

    private static IServiceCollection AddCustomSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme."
            });

            options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                                {
                                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                        },
                        Array.Empty<string>()
                    }
                });
        });

        return services;
    }

    private static IServiceCollection AddHealthCheck(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddNpgSql(
                sp => sp.GetRequiredService<IOptions<RelationalDatabaseSettings>>().Value.ConnectionString,
                name: "PostgreSQL",
                tags: new[] { "database", "relational" }
            );

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
                opt.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("ApplicationServices.Auth.Api"))
                   .AddMeter("Service_Auth_OpenRemoteManage")
                   .AddAspNetCoreInstrumentation()
                   .AddRuntimeInstrumentation()
                   .AddProcessInstrumentation()
                   .AddPrometheusExporter()
                   .AddOtlpExporter(otlpAction)
            )
            .WithTracing(opt =>
                opt.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("ApplicationServices.Auth.Api"))
                   .AddAspNetCoreInstrumentation()
                   .AddEntityFrameworkCoreInstrumentation()
                   .AddHttpClientInstrumentation()
                   .AddOtlpExporter(otlpAction)
            );

        services.AddMetrics();

        return services;
    }
}
