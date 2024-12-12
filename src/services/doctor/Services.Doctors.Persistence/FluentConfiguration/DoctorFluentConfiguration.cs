using Microsoft.EntityFrameworkCore;
using Services.Doctors.Domain.Entities;
using Services.Doctors.Domain.StrongIds;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Value.Objects.Helper.FluentApiConverters.Primitives;

namespace Services.Doctors.Persistence.FluentConfiguration;

internal sealed class DoctorFluentConfiguration
     : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.Property(e => e.Id)
            .IsRequired()
            .HasConversion<DoctorIdConverter>();

        builder.Property(e => e.IdAsGuid)
            .IsRequired()
            .HasConversion<GuidObjectConverter>();
        
        builder.Property(e => e.CredentialId)
            .IsRequired()
            .HasConversion<GuidObjectConverter>();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100)
            .HasConversion<StringObjectConverter>();

        builder.Property(e => e.NormalizedName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Specialty)
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion<StringObjectConverter>();

        builder.Property(e => e.ExperienceInYears)
            .IsRequired()
            .HasConversion<IntegerObjectConverter>();

        builder.ComplexProperty(i => i.AuditDates, navProps =>
        {
            navProps.Property(p => p.CreatedOnUtc)
                .IsRequired();

            navProps.Property(p => p.ModifiedOnUtc)
                .IsRequired();
        });

        builder.Property<uint>("version").IsRowVersion();


        builder.HasKey(o => o.Id);
        builder.HasIndex(i => new
        {
            i.CredentialId,
            i.Name,
            i.Specialty,
            i.ExperienceInYears
        });
        
        builder.Metadata.SetSchema("doctors");
    }
}