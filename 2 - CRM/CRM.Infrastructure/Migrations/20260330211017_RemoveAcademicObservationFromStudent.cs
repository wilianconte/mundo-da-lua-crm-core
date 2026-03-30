using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCRM.CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAcademicObservationFromStudent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcademicObservation",
                schema: "crm",
                table: "students");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AcademicObservation",
                schema: "crm",
                table: "students",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);
        }
    }
}
