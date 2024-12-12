using Value.Objects.Helper.Values.Domain;
using Common.Services.Hashing.Abstractions;
using Value.Objects.Helper.Values.Primitives;

namespace Services.Auth.Domain.Entities;

public sealed partial class Credential
{
    /// <summary>
    /// Create a new <see cref="Credential"/>
    /// </summary>
    /// <param name="email"></param>
    /// <param name="passwordHash"></param>
    /// <param name="hashingService"></param>
    /// <returns></returns>
    public static Credential Create(
        EmailAddress email,
        StringObject passwordHash,
        in IHashingService hashingService)
    {
        Credential created = new(email, passwordHash);
        created.SetPrivateKey(hashingService);

        return created;
    }

    /// <summary>
    /// Update <see cref="Credential"/> password
    /// </summary>
    /// <param name="password"></param>
    /// <returns></returns>
    public void UpdatePassword(StringObject password)
    {
        Password = password;
        AuditDates.ChangesApplied();
    }

    /// <summary>
    /// Set <see cref="Credential"/> private key
    /// </summary>
    /// <param name="hashingService"></param>
    /// <returns></returns>
    private void SetPrivateKey(in IHashingService hashingService)
    {
        string privateKey = hashingService.Hash($"{Id.Value}_{AuditDates.CreatedOnUtc.ToString()}");
        PrivateKey = StringObject.Create(privateKey);
    }
}
