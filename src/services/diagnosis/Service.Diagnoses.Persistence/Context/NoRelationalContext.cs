using MongoDB.Driver;
using Shared.Domain.Settings;
using Microsoft.Extensions.Options;

namespace Service.Diagnoses.Persistence.Context;

internal sealed class NoRelationalContext : IDisposable
{
    private readonly List<Func<Task>> _commands;
    private readonly IMongoDatabase _database;
    private int ChangeCount;

    public IMongoDatabase Database => _database;

    /// <summary>
    /// <see cref="NoRelationalContext"/> public constructor
    /// </summary>
    /// <param name="databaseOptions"></param>
    public NoRelationalContext(IOptions<NoRelationalDatabaseSettings> databaseOptions)
    {
        ArgumentNullException.ThrowIfNull(databaseOptions);

        _commands = new ();
        ChangeCount = 0;

        MongoClient mongoClient = new(databaseOptions.Value.ConnectionString);
        _database = mongoClient.GetDatabase(databaseOptions.Value.DatabaseName);
    }

    /// <summary>
    /// Save all change maked
    /// </summary>
    /// <returns></returns>
    public void SaveChanges()
    {
        if (ChangeCount.Equals(0))
            return;

        foreach (Func<Task> command in _commands)
            command.Invoke();

        _commands.Clear();
    }

    /// <summary>
    /// Check if change or commands list has any
    /// </summary>
    /// <returns></returns>
    public bool HasChanges()
        => _commands.Any();

    /// <summary>
    /// Add commnad waiting to call the <see cref="SaveChanges"/> method
    /// </summary>
    /// <param name="func"></param>
    /// <returns></returns>
    public async Task AddCommand(Func<Task> func)
    {
        _commands.Add(func);
        ChangeCount = _commands.Count;

        await Task.CompletedTask;
    }

    /// <summary>
    /// Get collection. Can add some filters and more
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public IMongoCollection<T> GetCollection<T>()
        => Database.GetCollection<T>(typeof(T).Name);

    /// <summary>
    /// Dispose method
    /// </summary>
    public void Dispose()
        => GC.SuppressFinalize(this);
}
