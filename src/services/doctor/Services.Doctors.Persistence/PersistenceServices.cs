using Shared.Domain.Settings;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Services.Doctors.Domain.Abstractions;
using Services.Doctors.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;
using Services.Doctors.Persistence.Repositories;

namespace Services.Doctors.Persistence;

public static class PersistenceServices
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>((servicesProvider, optionsBuilder) => {
            IOptions<RelationalDatabaseSettings>? databaseOptionsSetup = servicesProvider.GetService<IOptions<RelationalDatabaseSettings>>();
            ArgumentNullException.ThrowIfNull(databaseOptionsSetup, nameof(databaseOptionsSetup));

            RelationalDatabaseSettings databaseOptions = databaseOptionsSetup.Value;
            ArgumentNullException.ThrowIfNullOrEmpty(databaseOptions.ConnectionString, nameof(databaseOptions.ConnectionString));

            optionsBuilder.UseNpgsql(databaseOptions.ConnectionString, serverOptions =>
            {
                serverOptions.EnableRetryOnFailure(databaseOptions.MaxRetryCount);
                serverOptions.CommandTimeout(databaseOptions.CommandTimeout);
                serverOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                serverOptions.MigrationsHistoryTable("_Doctors_MigrationsHistory", schema: "migrations");
            });
#if DEBUG
            optionsBuilder.EnableDetailedErrors(true);
            optionsBuilder.EnableSensitiveDataLogging(true);
#endif
        });

        services.AddScoped<IDoctorRepository, DoctorRepository>();

        return services;
    }
}
