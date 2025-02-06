using Shared.Common.Helper.ErrorsHandler;

namespace Shared.Domain.Abstractions.Repositories;

public interface ISingleQueriesGenericRepository<TEntity, TId>
        where TEntity : class
        where TId : notnull
{
    /// <summary>
    /// Get a <see cref="TEntity"/> by <see cref="{TId}"/>
    /// </summary>
    /// <param name="id"></param>
    /// <param name="tracking"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Result<TEntity>> ByIdAsync(TId id, CancellationToken cancellationToken = default);
}