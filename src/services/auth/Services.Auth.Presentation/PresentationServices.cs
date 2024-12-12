using Microsoft.Extensions.DependencyInjection;
using Shared.Common.Helper.Filters;

namespace Services.Auth.Presentation;

public static class PresentationServices
{
    public static IServiceCollection AddPresentationServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor()
            .AddControllers()
            .AddApplicationPart(typeof(PresentationServices).Assembly);

        return services;
    }
}
