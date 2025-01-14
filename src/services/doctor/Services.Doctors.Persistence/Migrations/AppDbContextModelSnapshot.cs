﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Services.Doctors.Persistence.Context;

#nullable disable

namespace Services.Doctors.Persistence.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Services.Doctors.Domain.Entities.Doctor", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<Guid>("CredentialId")
                        .HasColumnType("uuid");

                    b.Property<int>("ExperienceInYears")
                        .HasColumnType("integer");

                    b.Property<Guid>("IdAsGuid")
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("NormalizedName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("Specialty")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<uint>("version")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("xid")
                        .HasColumnName("xmin");

                    b.ComplexProperty<Dictionary<string, object>>("AuditDates", "Services.Doctors.Domain.Entities.Doctor.AuditDates#AuditDates", b1 =>
                        {
                            b1.IsRequired();

                            b1.Property<DateTimeOffset>("CreatedOnUtc")
                                .HasColumnType("timestamp with time zone");

                            b1.Property<DateTimeOffset>("ModifiedOnUtc")
                                .HasColumnType("timestamp with time zone");
                        });

                    b.HasKey("Id");

                    b.HasIndex("CredentialId", "Name", "Specialty", "ExperienceInYears");

                    b.ToTable("Doctor", "doctors");
                });
#pragma warning restore 612, 618
        }
    }
}
