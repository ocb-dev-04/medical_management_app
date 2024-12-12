using System.Linq.Expressions;
using Services.Auth.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Services.Auth.Domain.Entities;
using Services.Auth.Domain.StrongIds;
using Services.Auth.Domain.Abstractions;
using Services.Auth.Persistence.Context;
using Shared.Common.Helper.ErrorsHandler;
using Value.Objects.Helper.Values.Domain;

namespace Services.Auth.Persistence.Repositories;

internal sealed class CredentialRepository
    : CredentialCompiledQueries,
        ICredentialRepository
{
    private readonly AppDbContext _dbContext;
    private readonly DbSet<Credential> _table;

    public CredentialRepository(AppDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));

        _dbContext = dbContext;
        _table = dbContext.Set<Credential>();
    }

    /// <inheritdoc/>
    public async Task<Result<Credential>> ByIdAsync(CredentialId id, CancellationToken cancellationToken = default)
    {
        Credential? found = await _table.FindAsync(id, cancellationToken);
        if (found is null)
            return Result.Failure<Credential>(CredentialErrors.NotFound);

        return found;
    }

    /// <inheritdoc/>
    public async Task<bool> ExistAsync(Expression<Func<Credential, bool>> filter, CancellationToken cancellationToken = default)
        => await AnyFilter(_dbContext, filter);

    /// <inheritdoc/>
    public async Task<Result<Credential>> ByEmailAsync(EmailAddress email, bool tracking = true, CancellationToken cancellationToken = default)
    {
        Credential? found = await GetCredentialByEmail(_dbContext, email);
        if (found is null)
            return Result.Failure<Credential>(CredentialErrors.NotFound);

        return found;
    }

    /// <inheritdoc/>
    public async Task CreateAsync(Credential model, CancellationToken cancellationToken)
        => await _table.AddAsync(model, cancellationToken);

    /// <inheritdoc/>
    public async Task<Result> DeleteAsync(CredentialId id, CancellationToken cancellationToken = default)
    {
        Credential? found = await _table.FindAsync(id, cancellationToken);
        if (found is null)
            return Result.Failure(CredentialErrors.NotFound);

        _table.Remove(found);

        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        if (_dbContext.ChangeTracker.HasChanges())
            await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
        => _dbContext.Dispose();
}
