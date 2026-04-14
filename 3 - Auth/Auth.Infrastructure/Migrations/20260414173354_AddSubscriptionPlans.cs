using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCRM.Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Plan",
                schema: "auth",
                table: "tenants");

            migrationBuilder.CreateTable(
                name: "features",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_features", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "plans",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "plan_features",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    FeatureId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<int>(type: "integer", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plan_features", x => x.Id);
                    table.ForeignKey(
                        name: "FK_plan_features_features_FeatureId",
                        column: x => x.FeatureId,
                        principalSchema: "auth",
                        principalTable: "features",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_plan_features_plans_PlanId",
                        column: x => x.PlanId,
                        principalSchema: "auth",
                        principalTable: "plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tenant_plans",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsTrial = table.Column<bool>(type: "boolean", nullable: false),
                    FallbackPlanId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelledAt = table.Column<DateOnly>(type: "date", nullable: true),
                    PausedAt = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_plans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tenant_plans_plans_FallbackPlanId",
                        column: x => x.FallbackPlanId,
                        principalSchema: "auth",
                        principalTable: "plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tenant_plans_plans_PlanId",
                        column: x => x.PlanId,
                        principalSchema: "auth",
                        principalTable: "plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "billings",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReferenceMonth = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    InvoiceUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_billings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_billings_tenant_plans_TenantPlanId",
                        column: x => x.TenantPlanId,
                        principalSchema: "auth",
                        principalTable: "tenant_plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_billings_tenant_month_pending",
                schema: "auth",
                table: "billings",
                columns: new[] { "TenantId", "ReferenceMonth" },
                unique: true,
                filter: "\"Status\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_billings_TenantPlanId",
                schema: "auth",
                table: "billings",
                column: "TenantPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_features_Key",
                schema: "auth",
                table: "features",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_plan_features_FeatureId",
                schema: "auth",
                table: "plan_features",
                column: "FeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_plan_features_PlanId_FeatureId",
                schema: "auth",
                table: "plan_features",
                columns: new[] { "PlanId", "FeatureId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_plans_Name",
                schema: "auth",
                table: "plans",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenant_plans_FallbackPlanId",
                schema: "auth",
                table: "tenant_plans",
                column: "FallbackPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_plans_PlanId",
                schema: "auth",
                table: "tenant_plans",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_plans_tenantid_active",
                schema: "auth",
                table: "tenant_plans",
                column: "TenantId",
                unique: true,
                filter: "\"Status\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_tenant_plans_tenantid_paused",
                schema: "auth",
                table: "tenant_plans",
                column: "TenantId",
                unique: true,
                filter: "\"Status\" = 1");

            // ── Seed: Plans ──────────────────────────────────────────────────────
            var planFreeId    = new Guid("a1000000-0000-0000-0000-000000000001");
            var planBasicId   = new Guid("a1000000-0000-0000-0000-000000000002");
            var planProId     = new Guid("a1000000-0000-0000-0000-000000000003");

            migrationBuilder.InsertData(
                schema: "auth",
                table: "plans",
                columns: new[] { "Id", "Name", "DisplayName", "Price", "IsActive", "SortOrder", "IsDeleted", "CreatedAt" },
                values: new object[,]
                {
                    { planFreeId,  "free",  "Gratuito", 0m,  true, 1, false, DateTimeOffset.UtcNow },
                    { planBasicId, "basic", "Básico",   49m, true, 2, false, DateTimeOffset.UtcNow },
                    { planProId,   "pro",   "Pro",      99m, true, 3, false, DateTimeOffset.UtcNow },
                });

            // ── Seed: Features ───────────────────────────────────────────────────
            var featMaxStudentsId  = new Guid("b2000000-0000-0000-0000-000000000001");
            var featMaxEmployeesId = new Guid("b2000000-0000-0000-0000-000000000002");
            var featMaxCoursesId   = new Guid("b2000000-0000-0000-0000-000000000003");
            var featHasReportsId   = new Guid("b2000000-0000-0000-0000-000000000004");
            var featHasApiAccessId = new Guid("b2000000-0000-0000-0000-000000000005");

            migrationBuilder.InsertData(
                schema: "auth",
                table: "features",
                columns: new[] { "Id", "Key", "Description", "Type", "IsDeleted", "CreatedAt" },
                values: new object[,]
                {
                    { featMaxStudentsId,  "max_students",  "Número máximo de alunos ativos",      0 /* Numeric */, false, DateTimeOffset.UtcNow },
                    { featMaxEmployeesId, "max_employees", "Número máximo de funcionários ativos", 0 /* Numeric */, false, DateTimeOffset.UtcNow },
                    { featMaxCoursesId,   "max_courses",   "Número máximo de cursos ativos",       0 /* Numeric */, false, DateTimeOffset.UtcNow },
                    { featHasReportsId,   "has_reports",   "Acesso a relatórios avançados",        1 /* Boolean */, false, DateTimeOffset.UtcNow },
                    { featHasApiAccessId, "has_api_access","Acesso à API pública",                 1 /* Boolean */, false, DateTimeOffset.UtcNow },
                });

            // ── Seed: PlanFeatures ───────────────────────────────────────────────
            // Free: max_students=30, max_employees=2, max_courses=3, has_reports=0, has_api_access=0
            // Basic: max_students=200, max_employees=10, max_courses=20, has_reports=1, has_api_access=0
            // Pro: max_students=null, max_employees=null, max_courses=null, has_reports=1, has_api_access=1
            migrationBuilder.InsertData(
                schema: "auth",
                table: "plan_features",
                columns: new[] { "Id", "PlanId", "FeatureId", "Value", "IsDeleted", "CreatedAt" },
                values: new object[,]
                {
                    // Free
                    { new Guid("c3000000-0000-0000-0000-000000000001"), planFreeId,  featMaxStudentsId,  30,   false, DateTimeOffset.UtcNow },
                    { new Guid("c3000000-0000-0000-0000-000000000002"), planFreeId,  featMaxEmployeesId, 2,    false, DateTimeOffset.UtcNow },
                    { new Guid("c3000000-0000-0000-0000-000000000003"), planFreeId,  featMaxCoursesId,   3,    false, DateTimeOffset.UtcNow },
                    { new Guid("c3000000-0000-0000-0000-000000000004"), planFreeId,  featHasReportsId,   0,    false, DateTimeOffset.UtcNow },
                    { new Guid("c3000000-0000-0000-0000-000000000005"), planFreeId,  featHasApiAccessId, 0,    false, DateTimeOffset.UtcNow },
                    // Basic
                    { new Guid("c3000000-0000-0000-0000-000000000006"), planBasicId, featMaxStudentsId,  200,  false, DateTimeOffset.UtcNow },
                    { new Guid("c3000000-0000-0000-0000-000000000007"), planBasicId, featMaxEmployeesId, 10,   false, DateTimeOffset.UtcNow },
                    { new Guid("c3000000-0000-0000-0000-000000000008"), planBasicId, featMaxCoursesId,   20,   false, DateTimeOffset.UtcNow },
                    { new Guid("c3000000-0000-0000-0000-000000000009"), planBasicId, featHasReportsId,   1,    false, DateTimeOffset.UtcNow },
                    { new Guid("c3000000-0000-0000-0000-000000000010"), planBasicId, featHasApiAccessId, 0,    false, DateTimeOffset.UtcNow },
                    // Pro (null = ilimitado para numéricos)
                    { new Guid("c3000000-0000-0000-0000-000000000011"), planProId,   featMaxStudentsId,  null, false, DateTimeOffset.UtcNow },
                    { new Guid("c3000000-0000-0000-0000-000000000012"), planProId,   featMaxEmployeesId, null, false, DateTimeOffset.UtcNow },
                    { new Guid("c3000000-0000-0000-0000-000000000013"), planProId,   featMaxCoursesId,   null, false, DateTimeOffset.UtcNow },
                    { new Guid("c3000000-0000-0000-0000-000000000014"), planProId,   featHasReportsId,   1,    false, DateTimeOffset.UtcNow },
                    { new Guid("c3000000-0000-0000-0000-000000000015"), planProId,   featHasApiAccessId, 1,    false, DateTimeOffset.UtcNow },
                });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_plans_trial_unique",
                schema: "auth",
                table: "tenant_plans",
                columns: new[] { "TenantId", "PlanId" },
                unique: true,
                filter: "\"IsTrial\" = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "billings",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "plan_features",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "tenant_plans",
                schema: "auth");

            migrationBuilder.DropIndex(
                name: "IX_tenant_plans_tenantid_active",
                schema: "auth",
                table: "tenant_plans");

            migrationBuilder.DropTable(
                name: "features",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "plans",
                schema: "auth");

            migrationBuilder.AddColumn<int>(
                name: "Plan",
                schema: "auth",
                table: "tenants",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
