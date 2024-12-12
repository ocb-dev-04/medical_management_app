namespace Shared.Message.Queue.Requests.Responses;

public sealed record PatientQueueResponse(
     Guid Id,
    string Name,
    int Age,
    DateTimeOffset CreatedOnUtc)
{
    public static PatientQueueResponse Map(
        Guid id,
        string name,
        int age,
        DateTimeOffset createdOnUtc)
        => new(
            id,
            name,
            age,
            createdOnUtc);
}

public sealed record PatientCollectionQueueResponse(IEnumerable<PatientQueueResponse> Collection);