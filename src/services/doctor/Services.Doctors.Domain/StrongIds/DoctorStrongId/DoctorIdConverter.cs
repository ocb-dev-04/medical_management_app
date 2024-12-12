using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Services.Doctors.Domain.StrongIds;

public sealed class DoctorIdConverter
    : ValueConverter<DoctorId, Guid>
{
    public DoctorIdConverter()
        : base(
            (instance) => instance.Value,
            (id) => DoctorId.Create(id).Value)
    {
    }
}
