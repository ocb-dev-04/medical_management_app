using Service.Diagnoses.Domain.Entities;
using Shared.Message.Queue.Requests.Responses;

namespace Service.Diagnoses.Application.UseCases;

public sealed record DiagnosisResponse(
    Guid Id,
    string Disease,
    string Medicine,
    string Indications,
    TimeSpan DosageInterval,
    DateTimeOffset CreatedOnUtc)
{
    public static DiagnosisResponse Map(Diagnosis entity)
        => new(
            entity.Id.Value,
            entity.Disease.Value,
            entity.Medicine.Value,
            entity.Indications.Value,
            entity.DosageInterval,
            entity.CreatedOnUtc);

    public DiagnosisQueueResponse MapToQueueResponse()
        => new DiagnosisQueueResponse(
            this.Id, 
            this.Disease, 
            this.Medicine, 
            this.Indications, 
            this.DosageInterval, 
            this.CreatedOnUtc);
}