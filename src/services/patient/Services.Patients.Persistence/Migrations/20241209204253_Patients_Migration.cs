using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Services.Patients.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Patients_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "patients");

            migrationBuilder.CreateTable(
                name: "Patient",
                schema: "patients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdAsGuid = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Age = table.Column<int>(type: "integer", nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    AuditDates_CreatedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AuditDates_ModifiedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patient", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Patient_DoctorId_Name",
                schema: "patients",
                table: "Patient",
                columns: new[] { "DoctorId", "Name" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Patient",
                schema: "patients");
        }
    }
}
