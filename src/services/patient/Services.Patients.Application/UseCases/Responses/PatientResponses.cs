using Services.Patients.Domain.Entities;
using Shared.Message.Queue.Requests.Responses;

namespace Services.Patients.Application.UseCases;

public sealed record PatientResponse(
    Guid Id,
    string Name,
    int Age,
    DateTimeOffset CreatedOnUtc)
{
    public static PatientResponse Map(Patient entity)
        => new(
            entity.Id.Value,
            entity.Name.Value,
            entity.Age.Value,
            entity.AuditDates.CreatedOnUtc);

    public PatientQueueResponse MapToQueueResponse()
        => new PatientQueueResponse(
            this.Id, 
            this.Name, 
            this.Age, 
            this.CreatedOnUtc);
}
