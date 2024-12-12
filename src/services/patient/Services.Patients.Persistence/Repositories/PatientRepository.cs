using Services.Patients.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Services.Patients.Domain.Entities;
using Services.Patients.Domain.StrongIds;
using Services.Patients.Domain.Abstractions;
using Services.Patients.Persistence.Context;
using Shared.Common.Helper.ErrorsHandler;
using Value.Objects.Helper.Values.Primitives;
using System.Linq.Expressions;

namespace Services.Patients.Persistence.Repositories;

internal sealed class PatientRepository
    : IPatientRepository
{
    private readonly AppDbContext _dbContext;
    private readonly DbSet<Patient> _table;
    private readonly static int _pageSize = 10;

    public PatientRepository(AppDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));

        _dbContext = dbContext;
        _table = dbContext.Set<Patient>();
    }

    /// <inheritdoc/>
    public async Task<Result<Patient>> ByIdAsync(PatientId id, CancellationToken cancellationToken = default)
    {
        Patient? found = await _table.FindAsync(id, cancellationToken);
        if (found is null)
            return Result.Failure<Patient>(PatientErrors.NotFound);

        return found;
    }

    /// <inheritdoc/>
    public async Task<Result<Patient>> ByCredentialId(GuidObject credentialId, CancellationToken cancellationToken)
    {
        Patient? found = await _table.FirstOrDefaultAsync(f => f.DoctorId.Equals(credentialId), cancellationToken);
        if (found is null)
            return Result.Failure<Patient>(PatientErrors.NotFound);

        return found;
    }

    /// <inheritdoc/>
    public async Task<bool> ExistAsync(Expression<Func<Patient, bool>> filter, CancellationToken cancellationToken = default)
        => await _table.AsNoTracking()
                    .AnyAsync(filter, cancellationToken);

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<Patient>> CollectionByDoctorIdAsync(GuidObject doctorId, int pageNumber, CancellationToken cancellationToken = default)
        => await _table.AsNoTracking()
                    .Where(w => w.DoctorId.Equals(doctorId))
                    .OrderBy(w => w.Name)
                    .Skip((pageNumber - 1) * _pageSize).Take(_pageSize)
                    .ToArrayAsync(cancellationToken);

    /// <inheritdoc/>
    public async Task CreateAsync(Patient model, CancellationToken cancellationToken)
        => await _table.AddAsync(model, cancellationToken);

    /// <inheritdoc/>
    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        if (_dbContext.ChangeTracker.HasChanges())
            await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
        => _dbContext.Dispose();

}
