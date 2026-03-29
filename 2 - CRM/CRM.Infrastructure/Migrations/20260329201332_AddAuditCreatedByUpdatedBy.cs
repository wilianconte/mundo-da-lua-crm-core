using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCRM.CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditCreatedByUpdatedBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                schema: "crm",
                table: "students",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                schema: "crm",
                table: "students",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                schema: "crm",
                table: "student_guardians",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                schema: "crm",
                table: "student_guardians",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                schema: "crm",
                table: "student_courses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                schema: "crm",
                table: "student_courses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                schema: "crm",
                table: "people",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                schema: "crm",
                table: "people",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                schema: "crm",
                table: "employees",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                schema: "crm",
                table: "employees",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                schema: "crm",
                table: "customers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                schema: "crm",
                table: "customers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                schema: "crm",
                table: "courses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                schema: "crm",
                table: "courses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                schema: "crm",
                table: "companies",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                schema: "crm",
                table: "companies",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "crm",
                table: "students");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "crm",
                table: "students");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "crm",
                table: "student_guardians");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "crm",
                table: "student_guardians");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "crm",
                table: "student_courses");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "crm",
                table: "student_courses");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "crm",
                table: "people");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "crm",
                table: "people");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "crm",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "crm",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "crm",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "crm",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "crm",
                table: "courses");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "crm",
                table: "courses");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "crm",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "crm",
                table: "companies");
        }
    }
}
