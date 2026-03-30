using MyCRM.Shared.Kernel.Entities;

namespace MyCRM.CRM.Domain.Entities;

/// <summary>
/// Student is a role-specific extension of Person for the educational context.
///
/// This entity does NOT duplicate personal data (name, phone, email, document).
/// All identity data remains in the referenced Person entity.
/// Student stores only student-specific data such as unit reference,
/// status, and notes.
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

    /// <summary>Reference to the unit/branch where the student is enrolled.</summary>
    public Guid? UnitId { get; private set; }

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
        Guid? unitId = null,
        string? notes = null)
    {
        if (personId == Guid.Empty)
            throw new ArgumentException("PersonId is required.", nameof(personId));

        return new Student
        {
            TenantId = tenantId,
            PersonId = personId,
            UnitId   = unitId,
            Status   = StudentStatus.Active,
            Notes    = notes?.Trim(),
        };
    }

    // ── Domain Methods ────────────────────────────────────────────────────────

    public void UpdateInfo(
        Guid? unitId,
        string? notes)
    {
        UnitId = unitId;
        Notes  = notes?.Trim();
        Touch();
    }

    public void Activate()    { Status = StudentStatus.Active;      Touch(); }
    public void Deactivate()  { Status = StudentStatus.Inactive;    Touch(); }
    public void Graduate()    { Status = StudentStatus.Graduated;   Touch(); }
    public void Transfer()    { Status = StudentStatus.Transferred; Touch(); }
    public void Suspend()     { Status = StudentStatus.Suspended;   Touch(); }
}
