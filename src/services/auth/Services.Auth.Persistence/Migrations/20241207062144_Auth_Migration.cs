using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Services.Auth.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Auth_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.CreateTable(
                name: "Credential",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdAsGuid = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    PrivateKey = table.Column<string>(type: "text", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    AuditDates_CreatedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AuditDates_ModifiedOnUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Credential", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Credential_Email",
                schema: "identity",
                table: "Credential",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Credential",
                schema: "identity");
        }
    }
}
