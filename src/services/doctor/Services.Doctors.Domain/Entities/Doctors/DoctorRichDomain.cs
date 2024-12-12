using Shared.Common.Helper.Extensions;
using Value.Objects.Helper.Values.Primitives;

namespace Services.Doctors.Domain.Entities;

public sealed partial class Doctor
{
    /// <summary>
    /// Create a new <see cref="Doctor"/>
    /// </summary>
    /// <param name="credentialId"></param>
    /// <param name="name"></param>
    /// <param name="specialty"></param>
    /// <param name="experienceInYears"></param>
    /// <returns></returns>
    public static Doctor Create(
        GuidObject credentialId,
        StringObject name,
        StringObject specialty,
        IntegerObject experienceInYears)
    {
        Doctor created = new(
            credentialId, 
            name, 
            name.Value.NormalizeToFTS(), 
            specialty, experienceInYears);

        return created;
    }

    /// <summary>
    /// Update <see cref="Doctor"/> general information
    /// </summary>
    /// <param name="name"></param>
    /// <param name="specialty"></param>
    /// <param name="experienceInYears"></param>
    public void UpdateGeneaalData(
        StringObject name,
        StringObject specialty,
        IntegerObject experienceInYears)
    {
        Name = name;
        Specialty = specialty;
        ExperienceInYears = experienceInYears;

        AuditDates.ChangesApplied();
    }
}
