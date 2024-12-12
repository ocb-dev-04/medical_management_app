namespace Shared.Message.Queue.Requests.Responses;

public sealed record DoctorQueueResponse(
    Guid Id,
    string Name,
    string Specialty,
    int ExperienceInYears,
    DateTimeOffset CreatedOnUtc)
{
    public static DoctorQueueResponse Map(
        Guid id, 
        string name, 
        string specialty, 
        int experienceInYears, 
        DateTimeOffset createdOnUtc)
        => new(
            id, 
            name, 
            specialty, 
            experienceInYears, 
            createdOnUtc);
}
