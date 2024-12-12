using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Services.Auth.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;

namespace Services.Auth.Persistence;

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
}
