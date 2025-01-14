using Shared.Common.Helper.ErrorsHandler;
using Shared.Message.Queue.Requests.Responses;

namespace Service.Diagnoses.Domain.Abstractions;

public interface IMessageQeueServices
{
    /// <summary>
    /// Get <see cref="DoctorQueueResponse"/> by doctor id
    /// </summary>
    /// <param name="doctorId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Result<DoctorQueueResponse>> GetDoctorByIdAsync(Guid doctorId, CancellationToken cancellationToken);

    /// <summary>
    /// Get <see cref="PatientQueueResponse"/> by patient id
    /// </summary>
    /// <param name="patient"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Result<PatientQueueResponse>> GetPatientByIdAsync(Guid patient, CancellationToken cancellationToken);
}
