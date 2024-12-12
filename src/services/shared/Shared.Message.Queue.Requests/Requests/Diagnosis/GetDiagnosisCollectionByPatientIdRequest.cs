using MassTransit;

namespace Shared.Message.Queue.Requests.Requests;

[EntityName("get-diagnosis-collection-by-patient-id-bind")]
public sealed class GetDiagnosisCollectionByPatientIdRequest
{
    public Guid Id { get; set; }

    public int PageNumber { get; set; }
    public GetDiagnosisCollectionByPatientIdRequest()
    {

    }

    public static GetDiagnosisCollectionByPatientIdRequest Create(Guid id, int pageNumber)
        => new GetDiagnosisCollectionByPatientIdRequest
        {
            Id = id,
            PageNumber = pageNumber
        };
}