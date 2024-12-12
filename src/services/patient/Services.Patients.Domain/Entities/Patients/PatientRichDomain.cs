using Value.Objects.Helper.Values.Primitives;

namespace Services.Patients.Domain.Entities;

public sealed partial class Patient
{
    /// <summary>
    /// Create a new <see cref="Patient"/>
    /// </summary>
    /// <param name="doctorId"></param>
    /// <param name="name"></param>
    /// <param name="age"></param>
    /// <returns></returns>
    public static Patient Create(
        GuidObject doctorId,
        StringObject name,
        IntegerObject age)
    {
        Patient created = new(
            doctorId, 
            name, 
            age);

        return created;
    }

    /// <summary>
    /// Update <see cref="Patient"/> general information
    /// </summary>
    /// <param name="name"></param>
    /// <param name="age"></param>
    public void UpdateGeneralData(
        StringObject name,
        IntegerObject age)
    {
        Name = name;
        Age = age;

        AuditDates.ChangesApplied();
    }

    /// <summary>
    /// Set <see cref="Patient"/> as deleted
    /// </summary>
    public void SetAsDeleted()
    {
        Deleted = BooleanObject.CreateAsTrue();

        AuditDates.ChangesApplied();
    }
}
