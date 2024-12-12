using Services.Doctors.Domain.Entities;

namespace Services.Doctors.Application.UseCases;

public sealed record DoctorResponse(
    Guid Id,
    string Name,
    string Specialty,
    int ExperienceInYears,
    DateTimeOffset CreatedOnUtc)
{
    public static DoctorResponse Map(Doctor entity)
        => new(
            entity.Id.Value,
            entity.Name.Value,
            entity.Specialty.Value,
            entity.ExperienceInYears.Value,
            entity.AuditDates.CreatedOnUtc);
}
