using Services.Doctors.Domain.StrongIds;
using Value.Objects.Helper.Values.Complex;
using Value.Objects.Helper.Values.Primitives;

namespace Services.Doctors.Domain.Dtos;

public sealed record DoctorDto(
    Guid Id,
    string Name,
    string Specialty,
    int ExperienceInYears,
    DateTimeOffset CreatedOnUtc)
{
    public static DoctorDto Create(
        DoctorId id,
        StringObject name,
        StringObject specialty,
        IntegerObject experienceInYears,
        AuditDates auditDates
        )
        => new(
            id.Value,
            name.Value,
            specialty.Value,
            experienceInYears.Value,
            auditDates.CreatedOnUtc);
}