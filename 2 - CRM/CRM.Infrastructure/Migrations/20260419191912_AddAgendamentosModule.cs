using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCRM.CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAgendamentosModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "WalletId",
                schema: "crm",
                table: "payment_methods",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "appointment_recurrences",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Frequency = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    MaxOccurrences = table.Column<int>(type: "integer", nullable: true),
                    ParentAppointmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedOccurrences = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                    table.PrimaryKey("PK_appointment_recurrences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "patients",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
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
                    table.PrimaryKey("PK_patients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_patients_people_PersonId",
                        column: x => x.PersonId,
                        principalSchema: "crm",
                        principalTable: "people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "professional_specialties",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_professional_specialties", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "professionals",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommissionPercentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Bio = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    LicenseNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    WalletId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_professionals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_professionals_people_PersonId",
                        column: x => x.PersonId,
                        principalSchema: "crm",
                        principalTable: "people",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_professionals_wallets_WalletId",
                        column: x => x.WalletId,
                        principalSchema: "crm",
                        principalTable: "wallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "services",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DefaultPrice = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    DefaultDurationInMinutes = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_services", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "professional_schedules",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessionalId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_professional_schedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_professional_schedules_professionals_ProfessionalId",
                        column: x => x.ProfessionalId,
                        principalSchema: "crm",
                        principalTable: "professionals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "professional_specialty_links",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessionalId = table.Column<Guid>(type: "uuid", nullable: false),
                    SpecialtyId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_professional_specialty_links", x => x.Id);
                    table.ForeignKey(
                        name: "FK_professional_specialty_links_professional_specialties_Speci~",
                        column: x => x.SpecialtyId,
                        principalSchema: "crm",
                        principalTable: "professional_specialties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_professional_specialty_links_professionals_ProfessionalId",
                        column: x => x.ProfessionalId,
                        principalSchema: "crm",
                        principalTable: "professionals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "appointments",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessionalId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    address_street = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    address_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    address_complement = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    address_neighborhood = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    address_city = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    address_state = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    address_zip_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    address_country = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    MeetingLink = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PaymentReceiver = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PaymentMethodId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RecurrenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConfirmedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledWithLateNotice = table.Column<bool>(type: "boolean", nullable: false),
                    NoShowAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RescheduledFrom = table.Column<Guid>(type: "uuid", nullable: true),
                    RescheduledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_appointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_appointments_appointment_recurrences_RecurrenceId",
                        column: x => x.RecurrenceId,
                        principalSchema: "crm",
                        principalTable: "appointment_recurrences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_appointments_patients_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "crm",
                        principalTable: "patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_appointments_payment_methods_PaymentMethodId",
                        column: x => x.PaymentMethodId,
                        principalSchema: "crm",
                        principalTable: "payment_methods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_appointments_professionals_ProfessionalId",
                        column: x => x.ProfessionalId,
                        principalSchema: "crm",
                        principalTable: "professionals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_appointments_services_ServiceId",
                        column: x => x.ServiceId,
                        principalSchema: "crm",
                        principalTable: "services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "commission_rules",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessionalId = table.Column<Guid>(type: "uuid", nullable: true),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompanyPercentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_commission_rules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_commission_rules_professionals_ProfessionalId",
                        column: x => x.ProfessionalId,
                        principalSchema: "crm",
                        principalTable: "professionals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_commission_rules_services_ServiceId",
                        column: x => x.ServiceId,
                        principalSchema: "crm",
                        principalTable: "services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "professional_services",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessionalId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomPrice = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    CustomDurationInMinutes = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_professional_services", x => x.Id);
                    table.ForeignKey(
                        name: "FK_professional_services_professionals_ProfessionalId",
                        column: x => x.ProfessionalId,
                        principalSchema: "crm",
                        principalTable: "professionals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_professional_services_services_ServiceId",
                        column: x => x.ServiceId,
                        principalSchema: "crm",
                        principalTable: "services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "appointment_tasks",
                schema: "crm",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AssignedToUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedToRole = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Result = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_appointment_tasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_appointment_tasks_appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalSchema: "crm",
                        principalTable: "appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_payment_methods_WalletId",
                schema: "crm",
                table: "payment_methods",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_appointment_recurrences_IsDeleted",
                schema: "crm",
                table: "appointment_recurrences",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_appointment_recurrences_ParentAppointmentId",
                schema: "crm",
                table: "appointment_recurrences",
                column: "ParentAppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_appointment_tasks_AppointmentId_Status",
                schema: "crm",
                table: "appointment_tasks",
                columns: new[] { "AppointmentId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_appointment_tasks_AssignedToUserId",
                schema: "crm",
                table: "appointment_tasks",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_appointment_tasks_IsDeleted",
                schema: "crm",
                table: "appointment_tasks",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_appointments_IsDeleted",
                schema: "crm",
                table: "appointments",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_appointments_PatientId",
                schema: "crm",
                table: "appointments",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_appointments_PaymentMethodId",
                schema: "crm",
                table: "appointments",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_appointments_ProfessionalId",
                schema: "crm",
                table: "appointments",
                column: "ProfessionalId");

            migrationBuilder.CreateIndex(
                name: "IX_appointments_RecurrenceId",
                schema: "crm",
                table: "appointments",
                column: "RecurrenceId");

            migrationBuilder.CreateIndex(
                name: "IX_appointments_ServiceId",
                schema: "crm",
                table: "appointments",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_appointments_Status",
                schema: "crm",
                table: "appointments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_appointments_TenantId_PatientId",
                schema: "crm",
                table: "appointments",
                columns: new[] { "TenantId", "PatientId" });

            migrationBuilder.CreateIndex(
                name: "IX_appointments_TenantId_ProfessionalId_StartDateTime",
                schema: "crm",
                table: "appointments",
                columns: new[] { "TenantId", "ProfessionalId", "StartDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_commission_rules_IsDeleted",
                schema: "crm",
                table: "commission_rules",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_commission_rules_ProfessionalId",
                schema: "crm",
                table: "commission_rules",
                column: "ProfessionalId");

            migrationBuilder.CreateIndex(
                name: "IX_commission_rules_ServiceId",
                schema: "crm",
                table: "commission_rules",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_commission_rules_TenantId_ProfessionalId_ServiceId",
                schema: "crm",
                table: "commission_rules",
                columns: new[] { "TenantId", "ProfessionalId", "ServiceId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_patients_IsDeleted",
                schema: "crm",
                table: "patients",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_patients_PersonId",
                schema: "crm",
                table: "patients",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_patients_Status",
                schema: "crm",
                table: "patients",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_patients_TenantId_PersonId",
                schema: "crm",
                table: "patients",
                columns: new[] { "TenantId", "PersonId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_professional_schedules_IsDeleted",
                schema: "crm",
                table: "professional_schedules",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_professional_schedules_ProfessionalId_DayOfWeek",
                schema: "crm",
                table: "professional_schedules",
                columns: new[] { "ProfessionalId", "DayOfWeek" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_professional_services_IsDeleted",
                schema: "crm",
                table: "professional_services",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_professional_services_ProfessionalId_ServiceId",
                schema: "crm",
                table: "professional_services",
                columns: new[] { "ProfessionalId", "ServiceId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_professional_services_ServiceId",
                schema: "crm",
                table: "professional_services",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_professional_specialties_IsDeleted",
                schema: "crm",
                table: "professional_specialties",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_professional_specialties_TenantId_Name",
                schema: "crm",
                table: "professional_specialties",
                columns: new[] { "TenantId", "Name" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_professional_specialty_links_ProfessionalId_SpecialtyId",
                schema: "crm",
                table: "professional_specialty_links",
                columns: new[] { "ProfessionalId", "SpecialtyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_professional_specialty_links_SpecialtyId",
                schema: "crm",
                table: "professional_specialty_links",
                column: "SpecialtyId");

            migrationBuilder.CreateIndex(
                name: "IX_professionals_IsDeleted",
                schema: "crm",
                table: "professionals",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_professionals_PersonId",
                schema: "crm",
                table: "professionals",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_professionals_Status",
                schema: "crm",
                table: "professionals",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_professionals_TenantId_PersonId",
                schema: "crm",
                table: "professionals",
                columns: new[] { "TenantId", "PersonId" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_professionals_WalletId",
                schema: "crm",
                table: "professionals",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_services_IsActive",
                schema: "crm",
                table: "services",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_services_IsDeleted",
                schema: "crm",
                table: "services",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_services_TenantId_Name",
                schema: "crm",
                table: "services",
                columns: new[] { "TenantId", "Name" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.AddForeignKey(
                name: "FK_payment_methods_wallets_WalletId",
                schema: "crm",
                table: "payment_methods",
                column: "WalletId",
                principalSchema: "crm",
                principalTable: "wallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_payment_methods_wallets_WalletId",
                schema: "crm",
                table: "payment_methods");

            migrationBuilder.DropTable(
                name: "appointment_tasks",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "commission_rules",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "professional_schedules",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "professional_services",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "professional_specialty_links",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "appointments",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "professional_specialties",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "appointment_recurrences",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "patients",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "professionals",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "services",
                schema: "crm");

            migrationBuilder.DropIndex(
                name: "IX_payment_methods_WalletId",
                schema: "crm",
                table: "payment_methods");

            migrationBuilder.DropColumn(
                name: "WalletId",
                schema: "crm",
                table: "payment_methods");
        }
    }
}
