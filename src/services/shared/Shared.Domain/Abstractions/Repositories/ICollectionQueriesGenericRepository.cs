namespace Shared.Domain.Abstractions.Repositories;

public interface ICollectionQueriesGenericRepository<TEntity>
        where TEntity : class
{
    /// <summary>
    /// Get a <see cref="TEntity"/> collection
    /// </summary>
    /// <param name="pageNumber"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IReadOnlyCollection<TEntity>> CollectionAsync(int pageNumber, CancellationToken cancellationToken = default);
}