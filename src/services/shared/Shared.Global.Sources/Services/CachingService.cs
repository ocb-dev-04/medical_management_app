using Newtonsoft.Json;
using Shared.Domain.JsonResolvers;
using Shared.Common.Helper.ErrorsHandler;
using Shared.Domain.Abstractions.Services;
using Microsoft.Extensions.Caching.Distributed;

namespace Shared.Global.Sources.Services;

internal sealed class CachingService : ICachingService
{
    private readonly IDistributedCache _distributedCache;
    private static readonly JsonSerializerSettings _jsonSerializerSettings =
        new()
        {
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            ContractResolver = new PrivatePropsResolver()
        };

    public CachingService(IDistributedCache distributedCache)
    {
        ArgumentNullException.ThrowIfNull(distributedCache, nameof(distributedCache));

        _distributedCache = distributedCache;
    }

    /// <inheritdoc/>
    public async Task<Result<T>> GetOrCreateAsync<T>(
        string key,
        Func<Task<Result<T>>> factory,
        TimeSpan expiration,
        CancellationToken cancellationToken) where T : class
    {
        string? data = await _distributedCache.GetStringAsync(key, cancellationToken);
        if (!string.IsNullOrEmpty(data))
            return JsonConvert.DeserializeObject<T>(data, _jsonSerializerSettings);

        Result<T> getData = await factory();
        if (getData.IsFailure)
            return Result.Failure<T>(getData.Error);

        await _distributedCache.SetStringAsync(
                    key,
                    JsonConvert.SerializeObject(getData, _jsonSerializerSettings),
                    DefaultCacheOptions(expiration),
                    cancellationToken);

        return getData;
    }

    /// <inheritdoc/>
    public Task CreateAsync<T>(
        string key,
        T data,
        TimeSpan expiration,
        CancellationToken cancellationToken) where T : class
        => _distributedCache.SetStringAsync(
                    key,
                    JsonConvert.SerializeObject(data, _jsonSerializerSettings),
                    DefaultCacheOptions(expiration),
                    cancellationToken);

    /// <inheritdoc/>
    public async Task UpdateAsync<T>(
        string key,
        T data,
        TimeSpan expiration,
        CancellationToken cancellationToken) where T : class
    {
        await _distributedCache.RemoveAsync(key, cancellationToken);

        await _distributedCache.SetStringAsync(
                    key,
                    JsonConvert.SerializeObject(data, _jsonSerializerSettings),
                    DefaultCacheOptions(expiration),
                    cancellationToken);
    }

    /// <inheritdoc/>
    public Task RemoveByKeyAsync(string key, CancellationToken cancellationToken)
        => _distributedCache.RemoveAsync(key, cancellationToken);

    private DistributedCacheEntryOptions DefaultCacheOptions(TimeSpan expiration)
        => new()
        {
            AbsoluteExpirationRelativeToNow = expiration,
            SlidingExpiration = expiration
        };
}
