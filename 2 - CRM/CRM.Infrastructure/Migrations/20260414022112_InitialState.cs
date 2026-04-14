using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCRM.CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "crm");

            migrationBuilder.CreateTable(
                name: "companies",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LegalName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    TradeName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    RegistrationNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    StateRegistration = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    MunicipalRegistration = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    PrimaryPhone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    SecondaryPhone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    WhatsAppNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Website = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ContactPersonName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    ContactPersonEmail = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    ContactPersonPhone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CompanyType = table.Column<int>(type: "integer", nullable: true),
                    Industry = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ProfileImageUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    address_street = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    address_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    address_complement = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    address_neighborhood = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    address_city = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    address_state = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    address_zip_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    address_country = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true, defaultValue: "BR"),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_companies", x => x.Id);
                });

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
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_courses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "customers",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    Phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Document = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    address_street = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    address_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    address_complement = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    address_neighborhood = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    address_city = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    address_state = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    address_zip_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    address_country = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true, defaultValue: "BR"),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "people",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    PreferredName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    DocumentNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Gender = table.Column<int>(type: "integer", nullable: true),
                    MaritalStatus = table.Column<int>(type: "integer", nullable: true),
                    Nationality = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Occupation = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    PrimaryPhone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    SecondaryPhone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    WhatsAppNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ProfileImageUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_people", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "employees",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    HireDate = table.Column<DateOnly>(type: "date", nullable: true),
                    TerminationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Position = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Department = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ContractType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    WorkSchedule = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    WorkloadHours = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    PayrollNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ManagerEmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    UnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    CostCenter = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_employees_employees_ManagerEmployeeId",
                        column: x => x.ManagerEmployeeId,
                        principalSchema: "crm",
                        principalTable: "employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_employees_people_PersonId",
                        column: x => x.PersonId,
                        principalSchema: "crm",
                        principalTable: "people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "students",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
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
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
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
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
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
                name: "IX_companies_IsDeleted",
                schema: "crm",
                table: "companies",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_companies_TenantId_Email",
                schema: "crm",
                table: "companies",
                columns: new[] { "TenantId", "Email" },
                unique: true,
                filter: "\"Email\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_companies_TenantId_RegistrationNumber",
                schema: "crm",
                table: "companies",
                columns: new[] { "TenantId", "RegistrationNumber" },
                unique: true,
                filter: "\"RegistrationNumber\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_companies_TenantId_Status",
                schema: "crm",
                table: "companies",
                columns: new[] { "TenantId", "Status" });

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
                name: "IX_customers_IsDeleted",
                schema: "crm",
                table: "customers",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_customers_TenantId_Email",
                schema: "crm",
                table: "customers",
                columns: new[] { "TenantId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customers_TenantId_Status",
                schema: "crm",
                table: "customers",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_employees_IsDeleted",
                schema: "crm",
                table: "employees",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_employees_ManagerEmployeeId",
                schema: "crm",
                table: "employees",
                column: "ManagerEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_employees_PersonId",
                schema: "crm",
                table: "employees",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_employees_TenantId_EmployeeCode",
                schema: "crm",
                table: "employees",
                columns: new[] { "TenantId", "EmployeeCode" },
                unique: true,
                filter: "\"EmployeeCode\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_employees_TenantId_IsActive",
                schema: "crm",
                table: "employees",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_employees_TenantId_PersonId",
                schema: "crm",
                table: "employees",
                columns: new[] { "TenantId", "PersonId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_employees_TenantId_Status",
                schema: "crm",
                table: "employees",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_people_IsDeleted",
                schema: "crm",
                table: "people",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_people_TenantId_DocumentNumber",
                schema: "crm",
                table: "people",
                columns: new[] { "TenantId", "DocumentNumber" },
                unique: true,
                filter: "\"DocumentNumber\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_people_TenantId_Email",
                schema: "crm",
                table: "people",
                columns: new[] { "TenantId", "Email" },
                unique: true,
                filter: "\"Email\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_people_TenantId_Status",
                schema: "crm",
                table: "people",
                columns: new[] { "TenantId", "Status" });

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "companies",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "customers",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "employees",
                schema: "crm");

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

            migrationBuilder.DropTable(
                name: "people",
                schema: "crm");
        }
    }
}
