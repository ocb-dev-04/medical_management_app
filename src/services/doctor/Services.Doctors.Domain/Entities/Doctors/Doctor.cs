using Services.Doctors.Domain.StrongIds;
using Value.Objects.Helper.Values.Complex;
using Value.Objects.Helper.Values.Primitives;

namespace Services.Doctors.Domain.Entities;

public sealed partial class Doctor
{
    public DoctorId Id { get; init; }
    public GuidObject IdAsGuid { get; init; }
    public GuidObject CredentialId { get; init; }

    public StringObject Name { get; private set; }
    public string NormalizedName { get; private set; }
    public StringObject Specialty { get; private set; }
    public IntegerObject ExperienceInYears { get; private set; }

    public AuditDates AuditDates { get; init; } = AuditDates.Init();

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    /// <summary>
    /// Parameterless constructor for EF (migrations, deserialization, materialization, etc.)
    /// </summary>
    private Doctor()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {

    }

    private Doctor(
        GuidObject credentialId,
        StringObject name,
        string normalizedName,
        StringObject specialty,
        IntegerObject experienceInYears)
    {
        Id = DoctorId.New();
        IdAsGuid = GuidObject.Create(Id.Value.ToString());
        CredentialId = credentialId;

        Name = name;
        NormalizedName = normalizedName;
        Specialty = specialty;
        ExperienceInYears = experienceInYears;
    }
}
