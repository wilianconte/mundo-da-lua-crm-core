using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCRM.CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixProfessionalSpecialtyLinkSoftDeleteFilter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_professional_specialty_links_ProfessionalId_SpecialtyId",
                schema: "crm",
                table: "professional_specialty_links");

            migrationBuilder.CreateIndex(
                name: "IX_professional_specialty_links_ProfessionalId_SpecialtyId",
                schema: "crm",
                table: "professional_specialty_links",
                columns: new[] { "ProfessionalId", "SpecialtyId" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_professional_specialty_links_ProfessionalId_SpecialtyId",
                schema: "crm",
                table: "professional_specialty_links");

            migrationBuilder.CreateIndex(
                name: "IX_professional_specialty_links_ProfessionalId_SpecialtyId",
                schema: "crm",
                table: "professional_specialty_links",
                columns: new[] { "ProfessionalId", "SpecialtyId" },
                unique: true);
        }
    }
}
