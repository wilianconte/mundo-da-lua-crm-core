using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCRM.Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditCreatedByUpdatedBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                schema: "auth",
                table: "users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                schema: "auth",
                table: "users",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "auth",
                table: "users");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "auth",
                table: "users");
        }
    }
}
