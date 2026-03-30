using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCRM.CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStudentStatusColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_students_TenantId_Status",
                schema: "crm",
                table: "students");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "crm",
                table: "students");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "crm",
                table: "students",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_students_TenantId_Status",
                schema: "crm",
                table: "students",
                columns: new[] { "TenantId", "Status" });
        }
    }
}
