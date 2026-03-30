using MyCRM.Shared.Kernel.Entities;

namespace MyCRM.CRM.Domain.Entities;

/// <summary>
/// Student is a role-specific extension of Person for the educational context.
///
/// This entity does NOT duplicate personal data (name, phone, email, document).
/// All identity data remains in the referenced Person entity.
/// Student stores only student-specific data such as registration number,
/// school info, class, enrollment dates, and academic observations.
///
/// Relationship:
///   Person 1──0..1 Student  (FK on Student.PersonId)
///
/// A Person should not have duplicate Student records within the same tenant
/// unless the domain explicitly allows re-enrollment after graduation/transfer.
/// </summary>
public sealed class Student : TenantEntity
{
    // ── Person Reference ─────────────────────────────────────────────────────

    /// <summary>Reference to the master Person entity. Never store personal data here.</summary>
    public Guid PersonId { get; private set; }

    /// <summary>Navigation property for EF Core — do not use directly in domain logic.</summary>
    public Person? Person { get; private set; }

    // ── Enrollment ────────────────────────────────────────────────────────────

    /// <summary>Unique registration number within the tenant (e.g. school enrollment code).</summary>
    public string? RegistrationNumber { get; private set; }

    /// <summary>Name of the school or educational institution.</summary>
    public string? SchoolName { get; private set; }

    /// <summary>Current grade or class (e.g. "3rd Grade", "High School Year 2").</summary>
    public string? GradeOrClass { get; private set; }

    /// <summary>Type of enrollment (e.g. "Regular", "Transfer", "Scholarship").</summary>
    public string? EnrollmentType { get; private set; }

    /// <summary>Reference to the unit/branch where the student is enrolled.</summary>
    public Guid? UnitId { get; private set; }

    /// <summary>Class group identifier (e.g. "A", "B", "Morning").</summary>
    public string? ClassGroup { get; private set; }

    /// <summary>Date when the student started at the school/program.</summary>
    public DateOnly? StartDate { get; private set; }

    // ── Status & Notes ────────────────────────────────────────────────────────

    public StudentStatus Status { get; private set; }

    public string? Notes { get; private set; }

    // ── Navigation ────────────────────────────────────────────────────────────

    /// <summary>Guardian relationships for this student.</summary>
    public ICollection<StudentGuardian> Guardians { get; private set; } = [];

    // ─────────────────────────────────────────────────────────────────────────

    private Student() { }

    /// <summary>
    /// Factory method — the only way to create a new Student.
    /// Requires a valid PersonId. Personal data is NOT stored here.
    /// </summary>
    public static Student Create(
        Guid tenantId,
        Guid personId,
        string? registrationNumber = null,
        string? schoolName = null,
        string? gradeOrClass = null,
        string? enrollmentType = null,
        Guid? unitId = null,
        string? classGroup = null,
        DateOnly? startDate = null,
        string? notes = null)
    {
        if (personId == Guid.Empty)
            throw new ArgumentException("PersonId is required.", nameof(personId));

        return new Student
        {
            TenantId            = tenantId,
            PersonId            = personId,
            RegistrationNumber  = registrationNumber?.Trim(),
            SchoolName          = schoolName?.Trim(),
            GradeOrClass        = gradeOrClass?.Trim(),
            EnrollmentType      = enrollmentType?.Trim(),
            UnitId              = unitId,
            ClassGroup          = classGroup?.Trim(),
            StartDate           = startDate,
            Status              = StudentStatus.Active,
            Notes               = notes?.Trim(),
        };
    }

    // ── Domain Methods ────────────────────────────────────────────────────────

    public void UpdateInfo(
        string? registrationNumber,
        string? schoolName,
        string? gradeOrClass,
        string? enrollmentType,
        Guid? unitId,
        string? classGroup,
        DateOnly? startDate,
        string? notes)
    {
        RegistrationNumber  = registrationNumber?.Trim();
        SchoolName          = schoolName?.Trim();
        GradeOrClass        = gradeOrClass?.Trim();
        EnrollmentType      = enrollmentType?.Trim();
        UnitId              = unitId;
        ClassGroup          = classGroup?.Trim();
        StartDate           = startDate;
        Notes               = notes?.Trim();
        Touch();
    }

    public void Activate()    { Status = StudentStatus.Active;      Touch(); }
    public void Deactivate()  { Status = StudentStatus.Inactive;    Touch(); }
    public void Graduate()    { Status = StudentStatus.Graduated;   Touch(); }
    public void Transfer()    { Status = StudentStatus.Transferred; Touch(); }
    public void Suspend()     { Status = StudentStatus.Suspended;   Touch(); }
}
