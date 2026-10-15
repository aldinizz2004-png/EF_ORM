using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entety_ORM.Migrations
{
    /// <summary>
    /// Schema migration: adds the monthly Salary column to Coaches.
    /// defaultValue: 1500 means SQL Server fills the new column with 1500
    /// for every existing (legacy) coach, and keeps 1500 as the column default.
    /// </summary>
    public partial class AddCoachSalary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Salary",
                table: "Coaches",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 1500m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Salary",
                table: "Coaches");
        }
    }
}
