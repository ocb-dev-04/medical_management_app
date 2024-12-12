using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Services.Doctors.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Doctors_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "doctors");

            migrationBuilder.CreateTable(
                name: "Doctor",
                schema: "doctors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdAsGuid = table.Column<Guid>(type: "uuid", nullable: false),
                    CredentialId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Specialty = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ExperienceInYears = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    AuditDates_CreatedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AuditDates_ModifiedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doctor", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Doctor_CredentialId_Name_Specialty_ExperienceInYears",
                schema: "doctors",
                table: "Doctor",
                columns: new[] { "CredentialId", "Name", "Specialty", "ExperienceInYears" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Doctor",
                schema: "doctors");
        }
    }
}
