using Common.Services.Bus.Abstractions;
using Shared.Message.Queue.Requests.Buses;
using Shared.Message.Queue.Requests.Requests;
using Shared.Common.Helper.Models.QueueResponses;
using Shared.Common.Helper.Enums;
using Shared.Common.Helper.Extensions;
using Shared.Common.Helper.ErrorsHandler;
using Shared.Message.Queue.Requests.Responses;

namespace Services.Patients.Application.Services;

internal sealed class MessageQeueServices
{
    private readonly IMultiBusService<IGeneralBus, GetDoctorByIdRequest> _doctorByIdRequest;

    private readonly static Error _doctorNotFound 
        = Error.NotFound("doctorNotFound", "The doctor waas no found");

    public MessageQeueServices(IMultiBusService<IGeneralBus, GetDoctorByIdRequest> doctorByIdRequest)
    {
        ArgumentNullException.ThrowIfNull(doctorByIdRequest, nameof(doctorByIdRequest));

        _doctorByIdRequest = doctorByIdRequest;
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
}
