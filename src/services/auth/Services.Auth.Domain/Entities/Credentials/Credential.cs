using Services.Auth.Domain.StrongIds;
using Value.Objects.Helper.Values.Domain;
using Value.Objects.Helper.Values.Complex;
using Value.Objects.Helper.Values.Primitives;

namespace Services.Auth.Domain.Entities;

public sealed partial class Credential
{
    public CredentialId Id { get; init; }
    public GuidObject IdAsGuid { get; init; }
    public EmailAddress Email { get; init; }

    public StringObject Password { get; private set; } = StringObject.CreateAsEmpty();
    public StringObject PrivateKey { get; private set; } = StringObject.CreateAsEmpty();

    public AuditDates AuditDates { get; init; } = AuditDates.Init();

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    /// <summary>
    /// Parameterless constructor for EF (migrations, deserialization, materialization, etc.)
    /// </summary>
    private Credential()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {

    }

    private Credential(
        EmailAddress email,
        StringObject passwordHash)
    {
        Id = CredentialId.New();
        IdAsGuid = GuidObject.Create(Id.Value.ToString());
        Email = email;

        Password = passwordHash;
    }
}
