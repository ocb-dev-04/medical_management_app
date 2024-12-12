using Services.Auth.Domain.Entities;

namespace Services.Auth.Application.UseCases;

public sealed record CredentialResponse(
    Guid Id,
    string Email,
    DateTimeOffset CreatedOnUtc,
    DateTimeOffset ModifiedOnUtc)
{
    public static CredentialResponse Map(Credential entity)
        => new(
            entity.Id.Value,
            entity.Email.Value,
            entity.AuditDates.CreatedOnUtc,
            entity.AuditDates.ModifiedOnUtc);
}