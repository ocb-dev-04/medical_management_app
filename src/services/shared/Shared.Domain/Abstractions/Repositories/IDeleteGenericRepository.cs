using Shared.Common.Helper.ErrorsHandler;

namespace Shared.Domain.Abstractions.Repositories;

public interface IDeleteAsyncGenericRepository<TEntity, TId>
        where TEntity : class
        where TId : notnull
{
    /// <summary>
    /// Delete some <see cref="{TEntity}"/> by <see cref="{TId}"/>
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    Task<Result> DeleteAsync(TId id, CancellationToken cancellationToken = default);
}