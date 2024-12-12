using Microsoft.EntityFrameworkCore;
using Services.Auth.Domain.Entities;
using Services.Auth.Domain.StrongIds;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Value.Objects.Helper.FluentApiConverters.Domain;
using Value.Objects.Helper.FluentApiConverters.Primitives;

namespace Services.Auth.Persistence.FluentConfiguration;

internal sealed class CredentialFluentConfiguration
     : IEntityTypeConfiguration<Credential>
{
    public void Configure(EntityTypeBuilder<Credential> builder)
    {
        builder.Property(e => e.Id)
            .IsRequired()
            .HasConversion<CredentialIdConverter>();

        builder.Property(e => e.IdAsGuid)
            .IsRequired()
            .HasConversion<GuidObjectConverter>();

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(150)
            .HasConversion<EmailAddressConverter>();

        builder.Property(e => e.Password)
            .IsRequired()
            .HasConversion<StringObjectConverter>();

        builder.ComplexProperty(i => i.AuditDates, navProps =>
        {
            navProps.Property(p => p.CreatedOnUtc)
                .IsRequired();

            navProps.Property(p => p.ModifiedOnUtc)
                .IsRequired();
        });

        builder.Property(e => e.PrivateKey)
            .IsRequired()
            .HasConversion<StringObjectConverter>();

        builder.Property<uint>("version").IsRowVersion();


        builder.HasKey(o => o.Id);
        builder.HasIndex(i => i.Email).IsUnique();

        builder.Metadata.SetSchema("identity");
    }
}