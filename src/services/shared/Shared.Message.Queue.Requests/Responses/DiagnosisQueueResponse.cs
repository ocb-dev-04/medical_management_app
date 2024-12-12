namespace Shared.Message.Queue.Requests.Responses;

public sealed record DiagnosisQueueResponse(
    Guid Id,
    string Disease,
    string Medicine,
    string Indications,
    TimeSpan DosageInterval,
    DateTimeOffset CreatedOnUtc)
{
    public static DiagnosisQueueResponse Map(
        Guid id,
        string disease,
        string medicine,
        string indications,
        TimeSpan dosageInterval,
        DateTimeOffset createdOnUtc)
        => new(
            id,
            disease,
            medicine, 
            indications,
            dosageInterval, 
            createdOnUtc);
}

public sealed record DiagnosisCollectionQueueResponse(IEnumerable<DiagnosisQueueResponse> Collection);