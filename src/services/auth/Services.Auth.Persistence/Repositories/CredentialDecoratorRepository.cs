using System.Linq.Expressions;
using Services.Auth.Domain.Entities;
using Services.Auth.Domain.StrongIds;
using Services.Auth.Domain.Abstractions;
using Shared.Common.Helper.ErrorsHandler;
using Value.Objects.Helper.Values.Domain;
using Shared.Domain.Abstractions.Services;

namespace Services.Auth.Persistence.Repositories;

internal sealed class CredentialDecoratorRepository : ICredentialRepository
{
    private readonly ICachingService _cachingService;
    private readonly CredentialRepository _credentialRepository;

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
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task<Result<Credential>> ByEmailAsync(EmailAddress email, bool tracking = true, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task<bool> ExistAsync(Expression<Func<Credential, bool>> filter, CancellationToken cancellationToken = default)
        => _credentialRepository.ExistAsync(filter, cancellationToken);

    /// <inheritdoc/>
    public Task CreateAsync(Credential model, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task<Result> DeleteAsync(CredentialId id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task CommitAsync(CancellationToken cancellationToken)
        => _credentialRepository.CommitAsync(cancellationToken);

    /// <inheritdoc/>
    public void Dispose()
        => _credentialRepository.Dispose();
}
