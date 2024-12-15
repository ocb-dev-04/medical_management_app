﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#pragma warning disable 219, 612, 618
#nullable disable

namespace Services.Auth.Persistence.CompiledEntities
{
    public partial class AppDbContextModel
    {
        private AppDbContextModel()
            : base(skipDetectChanges: false, modelId: new Guid("633f2e77-83ea-42a8-949b-e3a35962f856"), entityTypeCount: 1)
        {
        }

        partial void Initialize()
        {
            var credential = CredentialEntityType.Create(this);

            CredentialEntityType.CreateAnnotations(credential);

            AddAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
            AddAnnotation("ProductVersion", "9.0.0");
            AddAnnotation("Relational:MaxIdentifierLength", 63);
        }
    }
}
