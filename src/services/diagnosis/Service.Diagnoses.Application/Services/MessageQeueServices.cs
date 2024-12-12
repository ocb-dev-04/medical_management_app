using Shared.Common.Helper.Enums;
using Shared.Common.Helper.Extensions;
using Common.Services.Bus.Abstractions;
using Shared.Common.Helper.ErrorsHandler;
using Shared.Message.Queue.Requests.Buses;
using Shared.Message.Queue.Requests.Requests;
using Shared.Message.Queue.Requests.Responses;
using Shared.Common.Helper.Models.QueueResponses;

namespace Service.Diagnoses.Application.Services;

internal sealed class MessageQeueServices
{
    private readonly IMultiBusService<IGeneralBus, GetDoctorByIdRequest> _doctorByIdRequest;
    private readonly IMultiBusService<IGeneralBus, GetPatientByIdRequest> _patientByIdRequest;

    private readonly static Error _doctorNotFound 
        = Error.NotFound("doctorNotFound", "The doctor was no found");
    
    private readonly static Error _patientNotFound 
        = Error.NotFound("patientNotFound", "The patient was no found");

    public MessageQeueServices(
        IMultiBusService<IGeneralBus, GetDoctorByIdRequest> doctorByIdRequest,
        IMultiBusService<IGeneralBus, GetPatientByIdRequest> patientByIdRequest)
    {
        ArgumentNullException.ThrowIfNull(doctorByIdRequest, nameof(doctorByIdRequest));
        ArgumentNullException.ThrowIfNull(patientByIdRequest, nameof(patientByIdRequest));

        _doctorByIdRequest = doctorByIdRequest;
        _patientByIdRequest = patientByIdRequest;
    }

    public async Task<Result<DoctorQueueResponse>> GetDoctorByIdAsync(Guid doctorId, CancellationToken cancellationToken)
    {
        BusMessageResponse response = await _doctorByIdRequest.Request(GetDoctorByIdRequest.Create(doctorId), cancellationToken: cancellationToken);
        if (response.SomeErrorOcurred || string.IsNullOrEmpty(response.SerializedData))
            return Result.Failure<DoctorQueueResponse>(
                response.RequestStatus.Equals(RequestResultStatus.NotFound)
                    ? _doctorNotFound
                    : Error.NullValue);

        return response.SerializedData.Deserialize<DoctorQueueResponse>();
    }

    public async Task<Result<PatientQueueResponse>> GetPatientByIdAsync(Guid patient, CancellationToken cancellationToken)
    {
        BusMessageResponse response = await _patientByIdRequest.Request(GetPatientByIdRequest.Create(patient), cancellationToken: cancellationToken);
        if (response.SomeErrorOcurred || string.IsNullOrEmpty(response.SerializedData))
            return Result.Failure<PatientQueueResponse>(
                response.RequestStatus.Equals(RequestResultStatus.NotFound)
                    ? _patientNotFound
                    : Error.NullValue);

        return response.SerializedData.Deserialize<PatientQueueResponse>();
    }
}
