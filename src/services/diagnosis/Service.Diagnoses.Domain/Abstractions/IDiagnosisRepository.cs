using Service.Diagnoses.Domain.Entities;
using Value.Objects.Helper.Values.Primitives;
using Shared.Domain.Abstractions.Repositories;

namespace Service.Diagnoses.Domain.Abstractions;

public interface IDiagnosisRepository
    : ISingleQueriesGenericRepository<Diagnosis, GuidObject>,
        IBooleanGenericRepository<Diagnosis, GuidObject>,
        ICreateGenericRepository<Diagnosis>,
        IDeleteAsyncGenericRepository<Diagnosis, GuidObject>,
        IDisposable
{
    /// <summary>
    /// Get <see cref="Diagnosis"/> by patient id
    /// </summary>
    /// <param name="patientId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IReadOnlyCollection<Diagnosis>> ByPatientId(GuidObject patientId, int pageNumber, CancellationToken cancellationToken);

    /// <summary>
    /// Save all changes
    /// </summary>
    void Commit();
}