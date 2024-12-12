using Service.Diagnoses.Domain.Abstractions;
using Service.Diagnoses.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;
using Service.Diagnoses.Persistence.Serializers;
using Service.Diagnoses.Persistence.Repositories;

namespace Service.Diagnoses.Persistence;

public static class PersistenceServices
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services)
    {
        DiagnosisSerializer.RegisterMappings();
        services.AddScoped<NoRelationalContext>();

        services.AddScoped<IDiagnosisRepository, DiagnosisRepository>();

        return services;
    }

    public static void EnsureCollectionsExist(this IServiceProvider serviceProvider)
    {
        
    }
}
