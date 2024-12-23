using Elastic.Transport;
using Shared.Domain.Settings;
using System.Linq.Expressions;
using Microsoft.Extensions.Options;
using Elastic.Clients.Elasticsearch;
using Shared.Search.Sources.Abstractions;
using Shared.Common.Helper.ErrorsHandler;
namespace Shared.Search.Sources.Implementations;

using Result = Shared.Common.Helper.ErrorsHandler.Result;

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

        _elasticSettings = options.Value;

        ElasticsearchClientSettings settings = new ElasticsearchClientSettings(new Uri(_elasticSettings.Url))
            .DefaultIndex(_elasticSettings.DefaultIndex)
            .Authentication(new BasicAuthentication(_elasticSettings.UserName, _elasticSettings.Passwword));

        _client = new ElasticsearchClient(settings);
    }

    public async Task<Result<IReadOnlyCollection<T>>> SearchAsync(Func<T, object> func, string value, string indexName, CancellationToken cancellationToken)
    {
        string field = GetFieldName(func);
        SearchResponse<T> response = await _client.SearchAsync<T>(s 
            => s.Index(indexName)
                .Query(q 
                    => q.Bool(b 
                            => b.Must(
                                m 
                                    => m.Match(
                                        match => match.Field(field)
                                        .Query(value)
                                        )
                                    )
                            )
                    )
        , cancellationToken);

        return response.IsValidResponse
            ? Result.Success(response.Documents)
            : Result.Failure<IReadOnlyCollection<T>>();
    }

    public async Task<bool> AddOrUpdateAsync(T model, string indexName, CancellationToken cancellationToken)
    {
        if (!(await _client.Indices.ExistsAsync(indexName, cancellationToken)).Exists)
            await _client.Indices.CreateAsync(indexName, cancellationToken);

        IndexResponse response = await _client.IndexAsync(
            model,
            idx
                => idx.Index(_elasticSettings.DefaultIndex).OpType(OpType.Index),
            cancellationToken);

        return response.IsValidResponse;
    }

    public async Task<bool> RemoveAsync(string id, string indexName, CancellationToken cancellationToken)
    {
        if (!(await _client.Indices.ExistsAsync(indexName, cancellationToken)).Exists)
            return false;

        DeleteResponse response = await _client.DeleteAsync<T>(
            id,
            d => d.Index(_elasticSettings.DefaultIndex),
            cancellationToken);

        return response.IsValidResponse;
    }

    private string GetFieldName(Func<T, object> func)
        => func.Target is MemberExpression memberExpression 
            ? memberExpression.Member.Name 
            : string.Empty;
}