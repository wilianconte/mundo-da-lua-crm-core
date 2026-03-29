using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCRM.Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonIdToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PersonId",
                schema: "auth",
                table: "users",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_TenantId_PersonId",
                schema: "auth",
                table: "users",
                columns: new[] { "TenantId", "PersonId" },
                unique: true,
                filter: "\"PersonId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_TenantId_PersonId",
                schema: "auth",
                table: "users");

            migrationBuilder.DropColumn(
                name: "PersonId",
                schema: "auth",
                table: "users");
        }
    }
}
