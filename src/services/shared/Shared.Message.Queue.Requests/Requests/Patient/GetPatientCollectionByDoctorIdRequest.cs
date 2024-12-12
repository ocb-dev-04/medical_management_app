using MassTransit;

namespace Shared.Message.Queue.Requests.Requests;

[EntityName("get-patient-collection-by-doctor-id-bind")]
public sealed class GetPatientCollectionByDoctorIdRequest
{
    public Guid Id { get; set; }
    public int PageNumber { get; set; }

    public GetPatientCollectionByDoctorIdRequest()
    {

    }

    public static GetPatientCollectionByDoctorIdRequest Create(Guid id, int pageNumber)
        => new GetPatientCollectionByDoctorIdRequest
        {
            Id = id,
            PageNumber = pageNumber
        };
}