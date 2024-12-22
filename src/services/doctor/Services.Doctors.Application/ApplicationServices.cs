using Common.Services;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Services.Doctors.Application.Consumers;
using Services.Doctors.Application.Services;
using Shared.Domain.Abstractions.Services;
using Shared.Domain.Settings;
using Shared.Global.Sources.Behaviors;
using Shared.Message.Queue.Requests.Buses;
using Shared.Global.Sources.Services;
using Shared.Global.Sources;

namespace Services.Doctors.Application;

public static class ApplicationServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
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
            .AddElasticSearchService();

        services.AddTransient<MessageQeueServices>();

        return services;
    }

    private static IServiceCollection AddMessgeQueueServices(this IServiceCollection services)
    {
        services.AddMassTransit<IGeneralBus>(busRegConfig =>
        {
            busRegConfig.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter(prefix: string.Empty, includeNamespace: false));

            busRegConfig.AddConsumer<GetDoctorByIdConsumer, GetDoctorByIdConsumerDefinition>();

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
