using Value.Objects.Helper.Abstractions;
using Shared.Common.Helper.ErrorsHandler;
using Value.Objects.Helper.Values.Primitives;

namespace Services.Doctors.Domain.StrongIds;

public sealed class DoctorId
    : BaseId
{
    private static readonly Error _doctorIdCantBeNullOrEmpty
        = new(500, "doctorIdCantBeNullOrEmpty", "The doctor id cant be null or empty");

    public Guid Value { get; init; }

    private DoctorId()
    {

    }

    private DoctorId(Guid id)
        => Value = GuidObject.Create(id.ToString()).Value;

    public static Result<DoctorId> Create(Guid id)
    {
        if (string.IsNullOrEmpty(id.ToString()))
            return Result.Failure<DoctorId>(_doctorIdCantBeNullOrEmpty);

        return new DoctorId(id);
    }

    public static DoctorId New()
        => new(GuidObject.New().Value);

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}