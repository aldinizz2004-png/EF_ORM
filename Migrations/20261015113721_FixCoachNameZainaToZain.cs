using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entety_ORM.Migrations
{
    /// <summary>
    /// Data migration: fixes a coach name that was entered incorrectly
    /// during onboarding ("Zaina" -> "Zain").
    /// No schema change - it only runs a safe UPDATE on the existing row
    /// (no table or record is dropped, truncated or deleted).
    /// </summary>
    public partial class FixCoachNameZainaToZain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // UPDATE [Coaches] SET [Name] = N'Zain' WHERE [Name] = N'Zaina';
            migrationBuilder.UpdateData(
                table: "Coaches",
                keyColumn: "Name",
                keyValue: "Zaina",
                column: "Name",
                value: "Zain");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback: restore the original value
            migrationBuilder.UpdateData(
                table: "Coaches",
                keyColumn: "Name",
                keyValue: "Zain",
                column: "Name",
                value: "Zaina");
        }
    }
}
