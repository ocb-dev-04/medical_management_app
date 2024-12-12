using Microsoft.EntityFrameworkCore;
using Services.Patients.Domain.Entities;
using Services.Patients.Domain.StrongIds;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Value.Objects.Helper.FluentApiConverters.Primitives;
using Value.Objects.Helper.Values.Primitives;

namespace Services.Patients.Persistence.FluentConfiguration;

internal sealed class PatientFluentConfiguration
     : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.Property(e => e.Id)
            .IsRequired()
            .HasConversion<PatientIdConverter>();

        builder.Property(e => e.IdAsGuid)
            .IsRequired()
            .HasConversion<GuidObjectConverter>();
        
        builder.Property(e => e.DoctorId)
            .IsRequired()
            .HasConversion<GuidObjectConverter>();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100)
            .HasConversion<StringObjectConverter>();

        builder.Property(e => e.Age)
            .IsRequired()
            .HasConversion<IntegerObjectConverter>();

        builder.Property(e => e.Deleted)
            .IsRequired()
            .HasConversion<BooleanObjectConverter>();

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
            i.DoctorId,
            i.Name
        });

        builder.HasQueryFilter(p
            => p.Deleted.Equals(BooleanObject.CreateAsFalse()));

        builder.Metadata.SetSchema("patients");
    }
}