using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Shared.Global.Sources.Behaviors;
using System.IdentityModel.Tokens.Jwt;
using Services.Auth.Application.Providers;
using Microsoft.Extensions.DependencyInjection;
using MassTransit;
using Shared.Message.Queue.Requests.Buses;
using Services.Auth.Application.Consumers;
using Shared.Domain.Settings;
using Microsoft.Extensions.Options;
using Common.Services;
using Shared.Global.Sources;
using Microsoft.Extensions.Configuration;

namespace Services.Auth.Application;

public static class ApplicationServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<TokenProvider>();
        services.AddSingleton<JwtSecurityTokenHandler>();

        services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);
        
        services.AddValidatorsFromAssembly(
            typeof(ApplicationServices).Assembly, 
            includeInternalTypes: true);

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(ApplicationServices).Assembly);

            config.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
        });

        services.AddMultiBusServices()
            .AddMessgeQueueServices()
            .AddCachingService(configuration);

        return services;
    }

    public static IServiceCollection AddMessgeQueueServices(this IServiceCollection services)
    {
        services.AddMassTransit<IGeneralBus>(busRegConfig =>
            {
                busRegConfig.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter(prefix: string.Empty, includeNamespace: false));

                busRegConfig.AddConsumer<GetCredentialByIdConsumer, GetCredentialByIdConsumerDefinition>();

                busRegConfig.UsingRabbitMq((ctx, config) =>
                {
                    IOptions<MessageQueueSettings>? messageOptions = ctx.GetService<IOptions<MessageQueueSettings>>();
                    ArgumentNullException.ThrowIfNull(messageOptions, nameof(messageOptions));

                    MessageQueueSettings messageQueueOptions = messageOptions.Value; 
                    config.Host(new Uri(messageQueueOptions.Url), h =>
                    {
                        h.Username(messageQueueOptions.User);
                        h.Password(messageQueueOptions.Password);
#if !DEBUG
            h.UseSsl();
#endif
                    });

                    config.ConfigureEndpoints(ctx);
                });
            });

        return services;
    }
}
