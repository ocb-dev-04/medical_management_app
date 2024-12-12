using Services.Patients.Domain.Entities;
using Services.Patients.Domain.StrongIds;
using Value.Objects.Helper.Values.Primitives;
using Shared.Domain.Abstractions.Repositories;

namespace Services.Patients.Domain.Abstractions;

public interface IPatientRepository
    : ISingleQueriesGenericRepository<Patient, PatientId>,
        IBooleanGenericRepository<Patient, PatientId>,
        ICreateGenericRepository<Patient>,
        IDisposable
{
    /// <summary>
    /// Get <see cref="Patient"/> collection by doctor <see cref="GuidObject"/>
    /// </summary>
    /// <param name="doctorId"></param>
    /// <param name="pageNumber"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IReadOnlyCollection<Patient>> CollectionByDoctorIdAsync(GuidObject doctorId, int pageNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Save changes
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CommitAsync(CancellationToken cancellationToken);
}