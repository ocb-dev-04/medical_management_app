using MongoDB.Driver;
using System.Linq.Expressions;
using Service.Diagnoses.Domain.Errors;
using Service.Diagnoses.Domain.Entities;
using Shared.Common.Helper.ErrorsHandler;
using Service.Diagnoses.Persistence.Context;
using Service.Diagnoses.Domain.Abstractions;
using Value.Objects.Helper.Values.Primitives;

namespace Service.Diagnoses.Persistence.Repositories;

internal sealed class DiagnosisRepository : IDiagnosisRepository
{
    private readonly NoRelationalContext _context;
    private IMongoCollection<Diagnosis> _table;

    private static readonly int _pageSize = 10;

    public DiagnosisRepository(NoRelationalContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        _context = context;
        _table = _context.GetCollection<Diagnosis>();
    }

    /// <inheritdoc/>
    public async Task<Result<Diagnosis>> ByIdAsync(GuidObject id, CancellationToken cancellationToken = default)
    {
        Diagnosis? found = await _table.Find(f => f.Id.Equals(id)).FirstOrDefaultAsync(cancellationToken);
        if (found is null)
            return Result.Failure<Diagnosis>(DiagnosisErrors.NotFound);

        return found;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyCollection<Diagnosis>> ByPatientId(GuidObject patientId, int pageNumber, CancellationToken cancellationToken)
        => await _table
            .Find(w => w.PatientId.Equals(patientId))
            .SortByDescending(s => s.CreatedOnUtc)
            .Skip((pageNumber - 1) * _pageSize)
            .Limit(_pageSize)
            .ToListAsync(cancellationToken);

    /// <inheritdoc/>
    public async Task<bool> ExistAsync(Expression<Func<Diagnosis, bool>> filter, CancellationToken cancellationToken)
        => await _table.Find(filter).AnyAsync(cancellationToken);

    /// <inheritdoc/>
    public async Task CreateAsync(Diagnosis model, CancellationToken cancellationToken)
        => await _context.AddCommand((async () 
            => await _table.InsertOneAsync(model, default, cancellationToken)));

    /// <inheritdoc/>
    public async Task<Result> DeleteAsync(GuidObject id, CancellationToken cancellationToken)
    {
        await _context.AddCommand((async () 
            => await _table.DeleteOneAsync(d => d.Id.Equals(id), cancellationToken)));
        return Result.Success();
    }

    /// <inheritdoc/>
    public void Commit()
        => _context.SaveChanges();

    /// <inheritdoc/>
    public void Dispose()
        => _context.Dispose();
}
