using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCRM.CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStudentEnrollmentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_students_TenantId_RegistrationNumber",
                schema: "crm",
                table: "students");

            migrationBuilder.DropColumn(
                name: "ClassGroup",
                schema: "crm",
                table: "students");

            migrationBuilder.DropColumn(
                name: "EnrollmentType",
                schema: "crm",
                table: "students");

            migrationBuilder.DropColumn(
                name: "GradeOrClass",
                schema: "crm",
                table: "students");

            migrationBuilder.DropColumn(
                name: "RegistrationNumber",
                schema: "crm",
                table: "students");

            migrationBuilder.DropColumn(
                name: "SchoolName",
                schema: "crm",
                table: "students");

            migrationBuilder.DropColumn(
                name: "StartDate",
                schema: "crm",
                table: "students");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClassGroup",
                schema: "crm",
                table: "students",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnrollmentType",
                schema: "crm",
                table: "students",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GradeOrClass",
                schema: "crm",
                table: "students",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistrationNumber",
                schema: "crm",
                table: "students",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SchoolName",
                schema: "crm",
                table: "students",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "StartDate",
                schema: "crm",
                table: "students",
                type: "date",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_students_TenantId_RegistrationNumber",
                schema: "crm",
                table: "students",
                columns: new[] { "TenantId", "RegistrationNumber" },
                unique: true,
                filter: "\"RegistrationNumber\" IS NOT NULL");
        }
    }
}
