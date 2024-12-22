using System.Linq.Expressions;
using Services.Auth.Domain.Entities;
using Services.Auth.Domain.StrongIds;
using Services.Auth.Domain.Abstractions;
using Shared.Common.Helper.ErrorsHandler;
using Value.Objects.Helper.Values.Domain;

namespace Services.Auth.Persistence.Repositories;

internal sealed class CredentialDecoratorRepository : ICredentialRepository
{
    private readonly CredentialRepository _credentialRepository;

    public CredentialDecoratorRepository(CredentialRepository credentialRepository)
    {
        _credentialRepository = credentialRepository;
    }

    public Task<Result<Credential>> ByIdAsync(CredentialId id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<Credential>> ByEmailAsync(EmailAddress email, bool tracking = true, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExistAsync(Expression<Func<Credential, bool>> filter, CancellationToken cancellationToken = default)
        => _credentialRepository.ExistAsync(filter, cancellationToken);

    public Task CreateAsync(Credential model, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Result> DeleteAsync(CredentialId id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task CommitAsync(CancellationToken cancellationToken)
        => _credentialRepository.CommitAsync(cancellationToken);

    public void Dispose()
        => _credentialRepository.Dispose();
}
