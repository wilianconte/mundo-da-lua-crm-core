using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCRM.CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseAndStudentEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "courses",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ScheduleDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Capacity = table.Column<int>(type: "integer", nullable: true),
                    Workload = table.Column<int>(type: "integer", nullable: true),
                    UnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_courses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "students",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegistrationNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SchoolName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    GradeOrClass = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EnrollmentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClassGroup = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AcademicObservation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_students", x => x.Id);
                    table.ForeignKey(
                        name: "FK_students_people_PersonId",
                        column: x => x.PersonId,
                        principalSchema: "crm",
                        principalTable: "people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "student_courses",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnrollmentDate = table.Column<DateOnly>(type: "date", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CancellationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CompletionDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ClassGroup = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Shift = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ScheduleDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_courses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_student_courses_courses_CourseId",
                        column: x => x.CourseId,
                        principalSchema: "crm",
                        principalTable: "courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_student_courses_students_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "crm",
                        principalTable: "students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "student_guardians",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    GuardianPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelationshipType = table.Column<int>(type: "integer", nullable: false),
                    IsPrimaryGuardian = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsFinancialResponsible = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ReceivesNotifications = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CanPickupChild = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_guardians", x => x.Id);
                    table.ForeignKey(
                        name: "FK_student_guardians_people_GuardianPersonId",
                        column: x => x.GuardianPersonId,
                        principalSchema: "crm",
                        principalTable: "people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_student_guardians_students_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "crm",
                        principalTable: "students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_courses_IsDeleted",
                schema: "crm",
                table: "courses",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_courses_TenantId_Code",
                schema: "crm",
                table: "courses",
                columns: new[] { "TenantId", "Code" },
                unique: true,
                filter: "\"Code\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_courses_TenantId_Status",
                schema: "crm",
                table: "courses",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_courses_TenantId_Type",
                schema: "crm",
                table: "courses",
                columns: new[] { "TenantId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_student_courses_CourseId",
                schema: "crm",
                table: "student_courses",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_student_courses_IsDeleted",
                schema: "crm",
                table: "student_courses",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_student_courses_StudentId",
                schema: "crm",
                table: "student_courses",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_student_courses_TenantId_CourseId",
                schema: "crm",
                table: "student_courses",
                columns: new[] { "TenantId", "CourseId" });

            migrationBuilder.CreateIndex(
                name: "IX_student_courses_TenantId_Status",
                schema: "crm",
                table: "student_courses",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_student_courses_TenantId_StudentId",
                schema: "crm",
                table: "student_courses",
                columns: new[] { "TenantId", "StudentId" });

            migrationBuilder.CreateIndex(
                name: "IX_student_courses_TenantId_StudentId_CourseId",
                schema: "crm",
                table: "student_courses",
                columns: new[] { "TenantId", "StudentId", "CourseId" },
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_student_guardians_GuardianPersonId",
                schema: "crm",
                table: "student_guardians",
                column: "GuardianPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_student_guardians_IsDeleted",
                schema: "crm",
                table: "student_guardians",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_student_guardians_StudentId",
                schema: "crm",
                table: "student_guardians",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_student_guardians_TenantId_StudentId",
                schema: "crm",
                table: "student_guardians",
                columns: new[] { "TenantId", "StudentId" });

            migrationBuilder.CreateIndex(
                name: "IX_student_guardians_TenantId_StudentId_GuardianPersonId",
                schema: "crm",
                table: "student_guardians",
                columns: new[] { "TenantId", "StudentId", "GuardianPersonId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_students_IsDeleted",
                schema: "crm",
                table: "students",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_students_PersonId",
                schema: "crm",
                table: "students",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_students_TenantId_PersonId",
                schema: "crm",
                table: "students",
                columns: new[] { "TenantId", "PersonId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_students_TenantId_RegistrationNumber",
                schema: "crm",
                table: "students",
                columns: new[] { "TenantId", "RegistrationNumber" },
                unique: true,
                filter: "\"RegistrationNumber\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_students_TenantId_Status",
                schema: "crm",
                table: "students",
                columns: new[] { "TenantId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "student_courses",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "student_guardians",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "courses",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "students",
                schema: "crm");
        }
    }
}
