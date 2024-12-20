using Shared.Domain.Settings;
using Microsoft.Extensions.Options;
using Elastic.Clients.Elasticsearch;
using Shared.Domain.Abstractions.Services;
using Shared.Common.Helper.ErrorsHandler;
using Result = Shared.Common.Helper.ErrorsHandler.Result;

namespace Shared.Global.Sources.Services;

public sealed class ElasticSearchService<T>
    : IElasticSearchService<T>
        where T : class
{
    private readonly ElasticsearchClient _client;
    private readonly ElasticSettings _elasticSettings;

    private static readonly Error _nullValue = Error.NullValue;

    public ElasticSearchService(IOptions<ElasticSettings> options)
    {
        ArgumentNullException.ThrowIfNull(options.Value, nameof(options));

        _elasticSettings  = options.Value;
        ElasticsearchClientSettings settings = new ElasticsearchClientSettings(new Uri(_elasticSettings.Url))
            //.Authentication()
            .DefaultIndex(_elasticSettings.DefaultIndex);
        _client = new ElasticsearchClient(settings);
    }

    /// <inheritdoc/>
    public async Task CreateIndexIfNotExistAsync(string indexName, CancellationToken cancellationToken)
    {
        if (!(await _client.Indices.ExistsAsync(indexName, cancellationToken)).Exists)
            await _client.Indices.CreateAsync(indexName, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Result<T>> GetAsync(string key, CancellationToken cancellationToken)
    {
        GetResponse<T> response = await _client.GetAsync<T>(
            key,
            g => g.Index(_elasticSettings.DefaultIndex),
            cancellationToken);
        if (response.Source is null)
            return Result.Failure<T>(_nullValue);

        return response.Source;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<T>> GetAllAsync(CancellationToken cancellationToken)
    {
        SearchResponse<T> response = await _client.SearchAsync<T>(
            s => s.Index(_elasticSettings.DefaultIndex),
            cancellationToken: cancellationToken);

        return response.IsValidResponse 
            ? response.Documents.ToArray() 
            : Enumerable.Empty<T>().ToArray();
    }

    /// <inheritdoc/>
    public async Task<bool> AddOrUpdateAsync(T model, CancellationToken cancellationToken)
    {
        IndexResponse response = await _client.IndexAsync(
            model, 
            idx 
                => idx.Index(_elasticSettings.DefaultIndex).OpType(OpType.Index),
            cancellationToken);

        return response.IsValidResponse;
    }

    /// <inheritdoc/>
    public async Task<bool> AddOrUpdateBulkAsync(IEnumerable<T> collection, string indexName, CancellationToken cancellationToken)
    {
        BulkResponse response = await _client.BulkAsync(
            b 
                => b.Index(_elasticSettings.DefaultIndex)
                    .UpdateMany(collection, (descriptor, model) 
                        => descriptor.Doc(model).DocAsUpsert()),
            cancellationToken);

        return response.IsValidResponse;
    }

    /// <inheritdoc/>
    public async Task<bool> RemoveAsync(string key, CancellationToken cancellationToken)
    {
        DeleteResponse response = await _client.DeleteAsync<T>(
            key, 
            d => d.Index(_elasticSettings.DefaultIndex),
            cancellationToken);

        return response.IsValidResponse;
    }

    /// <inheritdoc/>
    public async Task<long?> RemoveAllAsync(CancellationToken cancellationToken)
    {
        DeleteByQueryResponse response = await _client.DeleteByQueryAsync<T>(
            d => d.Indices(_elasticSettings.DefaultIndex),
            cancellationToken);

        return response.IsValidResponse ? response.Deleted : default;
    }
}