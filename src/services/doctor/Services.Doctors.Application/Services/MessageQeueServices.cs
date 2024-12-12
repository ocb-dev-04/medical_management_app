using Shared.Common.Helper.Enums;
using Shared.Message.Queue.Requests;
using Shared.Common.Helper.Extensions;
using Common.Services.Bus.Abstractions;
using Shared.Common.Helper.ErrorsHandler;
using Shared.Message.Queue.Requests.Buses;
using Shared.Message.Queue.Requests.Requests;
using Shared.Common.Helper.Models.QueueResponses;

namespace Services.Doctors.Application.Services;

internal sealed class MessageQeueServices
{
    private readonly IMultiBusService<IGeneralBus, GetCredentialByIdRequest> _credentialByIdRequest;

    private readonly static Error _credentialNotFound 
        = Error.NotFound("credentialNotFound", "The credential waas no found");

    public MessageQeueServices(IMultiBusService<IGeneralBus, GetCredentialByIdRequest> credentialByIdRequest)
    {
        ArgumentNullException.ThrowIfNull(credentialByIdRequest, nameof(credentialByIdRequest));

        _credentialByIdRequest = credentialByIdRequest;
    }

    public async Task<Result<CredentialQueueResponse>> GetCredentialByIdAsync(Guid credentialId, CancellationToken cancellationToken)
    {
        BusMessageResponse response = await _credentialByIdRequest.Request(GetCredentialByIdRequest.Create(credentialId), cancellationToken: cancellationToken);
        if (response.SomeErrorOcurred || string.IsNullOrEmpty(response.SerializedData))
            return Result.Failure<CredentialQueueResponse>(
                response.RequestStatus.Equals(RequestResultStatus.NotFound)
                    ? _credentialNotFound
                    : Error.NullValue);

        return response.SerializedData.Deserialize<CredentialQueueResponse>();
    }
}
