using Value.Objects.Helper.Values.Primitives;

namespace Service.Diagnoses.Domain.Entities;

public sealed partial class Diagnosis
{
    public GuidObject Id { get; init; }
    public GuidObject DoctorId { get; init; }
    public GuidObject PatientId { get; init; }

    public StringObject Disease { get; private set; }
    public StringObject Medicine { get; private set; }
    public StringObject Indications { get; private set; }
    public TimeSpan DosageInterval { get; set; }

    public DateTimeOffset CreatedOnUtc { get; init; } = DateTimeOffset.UtcNow;

    private Diagnosis()
    {

    }

    private Diagnosis(
        GuidObject doctorId,
        GuidObject patientId,
        StringObject disease,
        StringObject medicine,
        StringObject indications,
        TimeSpan dosageInterval)
    {
        Id = GuidObject.New();
        DoctorId = doctorId;
        PatientId = patientId;

        Disease = disease;
        Medicine = medicine;
        Indications = indications;
        DosageInterval = dosageInterval;
    }
}
