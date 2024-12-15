using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Services.Patients.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;
using System.Data;
using System.Reflection;

namespace Services.Patients.Persistence;

public static class PersistenceBuilders
{
    /// <summary>
    /// Check and apply pending migrations
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static void CheckMigrations(this WebApplication app)
    {
        IServiceScope scope = app.Services.CreateScope();
        using AppDbContext? context = scope.ServiceProvider.GetService<AppDbContext>();
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        bool canConnect = context.Database.CanConnect();
        if (!canConnect) 
            throw new Exception("Can't connect to database");

        IEnumerable<string> pendingMigrations = context.Database.GetPendingMigrations();
        if (pendingMigrations.Any())
            context.Database.Migrate();
    }

    public static void Warnup(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        using AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        DbConnection connection = dbContext.Database.GetDbConnection();
        if (connection.State.Equals(ConnectionState.Closed))
            connection.Open();

        ExecuteFirstQuery(dbContext);
    }

    private static void ExecuteFirstQuery(AppDbContext dbContext)
    {
        IEnumerable<Type> entityTypes = dbContext.Model.GetEntityTypes()
            .Where(e => !e.IsOwned())
            .Select(e => e.ClrType);

        foreach (Type entityType in entityTypes)
        {
            MethodInfo setMethod = typeof(DbContext)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Single(m =>
                    m.Name == nameof(DbContext.Set) &&
                    m.IsGenericMethodDefinition &&
                    m.GetParameters().Length == 0);

            object? dbSet = setMethod.MakeGenericMethod(entityType).Invoke(dbContext, null);
            if (dbSet is null)
                continue;

            MethodInfo firstOrDefaultMethod = typeof(Queryable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(m => m.Name == nameof(Queryable.FirstOrDefault) && m.GetParameters().Length == 1)
                .MakeGenericMethod(entityType);

            object? queryable = typeof(EntityFrameworkQueryableExtensions)
                .GetMethod(nameof(EntityFrameworkQueryableExtensions.AsNoTracking))?
                .MakeGenericMethod(entityType)
                .Invoke(null, new[] { dbSet });
            if (queryable is null)
                continue;

            firstOrDefaultMethod.Invoke(null, new object[] { queryable });
        }
    }
}
