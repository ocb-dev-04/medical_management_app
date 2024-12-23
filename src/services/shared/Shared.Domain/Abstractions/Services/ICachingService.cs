using Shared.Common.Helper.ErrorsHandler;

namespace Shared.Domain.Abstractions.Services;

public interface ICachingService
{
    /// <summary>
    /// Get or create data from memory database
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="factory"></param>
    /// <param name="expiration"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Result<T>> GetOrCreateAsync<T>(
        string key,
        Func<Task<Result<T>>> factory,
        TimeSpan expiration,
        CancellationToken cancellationToken) where T : class;

    /// <summary>
    /// Create a new object in memory database
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="data"></param>
    /// <param name="expiration"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CreateAsync<T>(
        string key,
        T data,
        TimeSpan expiration,
        CancellationToken cancellationToken) where T : class;

    /// <summary>
    /// Update data in memory database
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="data"></param>
    /// <param name="expiration"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task UpdateAsync<T>(
        string key,
        T data,
        TimeSpan expiration,
        CancellationToken cancellationToken) where T : class;

    /// <summary>
    /// Remove data from memory database by key
    /// </summary>
    /// <param name="key"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RemoveByKeyAsync(string key, CancellationToken cancellationToken);
}
