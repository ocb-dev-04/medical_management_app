using Shared.Common.Helper.ErrorsHandler;

namespace Shared.Domain.Abstractions.Services;

public interface IElasticSearchService<T> where T : class
{
    /// <summary>
    /// Create a new index
    /// </summary>
    /// <param name="indexName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CreateIndexIfNotExistAsync(string indexName, CancellationToken cancellationToken);

    /// <summary>
    /// Add or update model
    /// </summary>
    /// <param name="model"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> AddOrUpdateAsync(T model, CancellationToken cancellationToken);

    /// <summary>
    /// Add or updaate model bulk
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="indexName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> AddOrUpdateBulkAsync(IEnumerable<T> collection, string indexName, CancellationToken cancellationToken);

    /// <summary>
    /// Get model
    /// </summary>
    /// <param name="key"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Result<T>> GetAsync(string key, CancellationToken cancellationToken);

    /// <summary>
    /// Get model collection
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IReadOnlyCollection<T>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Remmove
    /// </summary>
    /// <param name="key"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> RemoveAsync(string key, CancellationToken cancellationToken);

    /// <summary>
    /// remove all
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<long?> RemoveAllAsync(CancellationToken cancellationToken);
}
