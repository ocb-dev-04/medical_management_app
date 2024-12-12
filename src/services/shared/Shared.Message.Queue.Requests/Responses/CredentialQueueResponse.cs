namespace Shared.Message.Queue.Requests;

public sealed record CredentialQueueResponse(
    Guid Id,
    string Email,
    DateTimeOffset CreatedOnUtc,
    DateTimeOffset ModifiedOnUtc)
{
    public static CredentialQueueResponse Map(
        Guid id, 
        string email, 
        DateTimeOffset createdOnUtc,
        DateTimeOffset modifiedOnUtc)
        => new(
            id,
            email,
            createdOnUtc,
            modifiedOnUtc);
}
