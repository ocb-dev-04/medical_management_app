using MassTransit;

namespace Shared.Message.Queue.Requests.Requests;

[EntityName("get-doctor-by-id-bind")]
public sealed class GetDoctorByIdRequest
{
    public Guid Id { get; set; }

    public GetDoctorByIdRequest()
    {

    }

    public static GetDoctorByIdRequest Create(Guid id)
        => new GetDoctorByIdRequest { Id = id };
}