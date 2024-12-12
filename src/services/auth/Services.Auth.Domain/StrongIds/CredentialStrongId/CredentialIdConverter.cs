using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Services.Auth.Domain.StrongIds;

public sealed class CredentialIdConverter
    : ValueConverter<CredentialId, Guid>
{
    public CredentialIdConverter()
        : base(
            (instance) => instance.Value,
            (id) => CredentialId.Create(id).Value)
    {
    }
}
