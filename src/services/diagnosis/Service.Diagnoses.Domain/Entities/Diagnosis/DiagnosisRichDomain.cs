using Value.Objects.Helper.Values.Primitives;

namespace Service.Diagnoses.Domain.Entities;

public sealed partial class Diagnosis
{
    /// <summary>
    /// Create a new <see cref="Diagnosis"/>
    /// </summary>
    /// <param name="doctorId"></param>
    /// <param name="patientId"></param>
    /// <param name="disease"></param>
    /// <param name="medicine"></param>
    /// <param name="indications"></param>
    /// <param name="dosageInterval"></param>
    /// <returns></returns>
    public static Diagnosis Create(
        GuidObject doctorId,
        GuidObject patientId,
        StringObject disease,
        StringObject medicine,
        StringObject indications,
        TimeSpan dosageInterval)
    {
        Diagnosis created = new(
            doctorId,
            patientId,
            disease,
            medicine,
            indications,
            dosageInterval);

        return created;
    }
}
