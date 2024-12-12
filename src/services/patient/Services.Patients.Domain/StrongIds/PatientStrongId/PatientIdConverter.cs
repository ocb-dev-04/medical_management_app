using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Services.Patients.Domain.StrongIds;

public sealed class PatientIdConverter
    : ValueConverter<PatientId, Guid>
{
    public PatientIdConverter()
        : base(
            (instance) => instance.Value,
            (id) => PatientId.Create(id).Value)
    {
    }
}
