using Value.Objects.Helper.Abstractions;
using Shared.Common.Helper.ErrorsHandler;
using Value.Objects.Helper.Values.Primitives;

namespace Services.Patients.Domain.StrongIds;

public sealed class PatientId
    : BaseId
{
    private static readonly Error _patientIdCantBeNullOrEmpty
        = new(500, "patientIdCantBeNullOrEmpty", "The patient id cant be null or empty");

    public Guid Value { get; init; }

    private PatientId()
    {

    }

    private PatientId(Guid id)
        => Value = GuidObject.Create(id.ToString()).Value;

    public static Result<PatientId> Create(Guid id)
    {
        if (string.IsNullOrEmpty(id.ToString()))
            return Result.Failure<PatientId>(_patientIdCantBeNullOrEmpty);

        return new PatientId(id);
    }

    public static PatientId New()
        => new(GuidObject.New().Value);

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}