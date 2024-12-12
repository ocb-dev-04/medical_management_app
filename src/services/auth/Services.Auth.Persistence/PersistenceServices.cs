using Shared.Domain.Settings;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Services.Auth.Domain.Abstractions;
using Services.Auth.Persistence.Context;
using Services.Auth.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Services.Auth.Persistence;

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
                serverOptions.MigrationsHistoryTable("_Auth_MigrationsHistory", schema: "migrations");
            });
#if DEBUG
            optionsBuilder.EnableDetailedErrors(true);
            optionsBuilder.EnableSensitiveDataLogging(true);
#endif
        });

        services.AddScoped<ICredentialRepository, CredentialRepository>();

        return services;
    }
}
