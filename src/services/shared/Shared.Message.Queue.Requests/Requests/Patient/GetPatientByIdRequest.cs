using MassTransit;

namespace Shared.Message.Queue.Requests.Requests;

[EntityName("get-patient-by-id-bind")]
public sealed class GetPatientByIdRequest
{
    public Guid Id { get; set; }

    public GetPatientByIdRequest()
    {

    }

    public static GetPatientByIdRequest Create(Guid id)
        => new GetPatientByIdRequest { Id = id };
}