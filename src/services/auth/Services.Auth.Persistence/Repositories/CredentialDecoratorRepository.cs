using System.Linq.Expressions;
using Services.Auth.Domain.Entities;
using Services.Auth.Domain.StrongIds;
using Shared.Common.Helper.Extensions;
using Services.Auth.Domain.Abstractions;
using Shared.Common.Helper.ErrorsHandler;
using Value.Objects.Helper.Values.Domain;
using Shared.Domain.Abstractions.Services;

namespace Services.Auth.Persistence.Repositories;

internal sealed class CredentialDecoratorRepository : ICredentialRepository
{
    private readonly ICachingService _cachingService;
    private readonly CredentialRepository _credentialRepository;
    private static readonly TimeSpan _defaultTimeSpan =
                TimeSpan.FromHours(1);
    public CredentialDecoratorRepository(
        ICachingService cachingService,
        CredentialRepository credentialRepository)
    {
        ArgumentNullException.ThrowIfNull(cachingService, nameof(cachingService));
        ArgumentNullException.ThrowIfNull(credentialRepository, nameof(credentialRepository));

        _cachingService = cachingService;
        _credentialRepository = credentialRepository;
    }

    /// <inheritdoc/>
    public async Task<Result<Credential>> ByIdAsync(CredentialId id, CancellationToken cancellationToken = default)
    {
        Result<Credential> found = await _cachingService.GetOrCreateAsync<Credential>(
                id.Value.ToString(),
                async () =>
                {
                    Result<Credential> found = await _credentialRepository.ByIdAsync(id, cancellationToken);
                    if (found.IsFailure)
                        return Result.Failure<Credential>(found.Error);

                    return found;
                },
                _defaultTimeSpan,
                cancellationToken);

        _credentialRepository.Attach(found.Value);
        return found;
    }

    /// <inheritdoc/>
    public async Task<Result<Credential>> ByEmailAsync(EmailAddress email, bool tracking = true, CancellationToken cancellationToken = default)
    {
        Result<Credential> found = await _cachingService.GetOrCreateAsync<Credential>(
                email.Value.ToString(),
                async () =>
                {
                    Result<Credential> found = await _credentialRepository.ByEmailAsync(email, tracking, cancellationToken);
                    if (found.IsFailure)
                        return Result.Failure<Credential>(found.Error);

                    return found;
                },
                _defaultTimeSpan,
                cancellationToken);
        
        if(tracking)
            _credentialRepository.Attach(found.Value);

        return found;
    }

    /// <inheritdoc/>
    public Task<bool> ExistAsync(Expression<Func<Credential, bool>> filter, CancellationToken cancellationToken = default)
        => _credentialRepository.ExistAsync(filter, cancellationToken);

    /// <inheritdoc/>
    public async Task CreateAsync(Credential model, CancellationToken cancellationToken)
    {
        await _credentialRepository.CreateAsync(model, cancellationToken);
        await _cachingService.CreateAsync(model.Id.Value.ToString(), model.Serialize(), _defaultTimeSpan, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Result> DeleteAsync(CredentialId id, CancellationToken cancellationToken = default)
    {
        await _credentialRepository.DeleteAsync(id, cancellationToken);
        await _cachingService.RemoveByKeyAsync(id.Value.ToString(), cancellationToken);

        return Result.Success();
    }

    /// <inheritdoc/>
    public Task CommitAsync(CancellationToken cancellationToken)
        => _credentialRepository.CommitAsync(cancellationToken);

    /// <inheritdoc/>
    public void Dispose()
        => _credentialRepository.Dispose();
}
