using Value.Objects.Helper.Abstractions;
using Shared.Common.Helper.ErrorsHandler;
using Value.Objects.Helper.Values.Primitives;

namespace Services.Auth.Domain.StrongIds;

public sealed class CredentialId
    : BaseId
{
    private static readonly Error _credentialIdCantBeNullOrEmpty
        = new(500, "credentialIdCantBeNullOrEmpty", "The credential id cant be null or empty");

    public Guid Value { get; init; }

    private CredentialId()
    {

    }

    private CredentialId(Guid id)
        => Value = GuidObject.Create(id.ToString()).Value;

    public static Result<CredentialId> Create(Guid id)
    {
        if (string.IsNullOrEmpty(id.ToString()))
            return Result.Failure<CredentialId>(_credentialIdCantBeNullOrEmpty);

        return new CredentialId(id);
    }

    public static CredentialId New()
        => new(GuidObject.New().Value);

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}